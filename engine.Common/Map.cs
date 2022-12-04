using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using engine.Common.Entities;

// todo rename WorldState
// todo move background update out into world

namespace engine.Common
{
    public class Map : IMap
    {
        public Map() { }

        public Map(int width, int height, int depth, Player[] players, Element[] objects, Background background)
        {
            Initialize(width: width, height: height, depth: depth, players: players, objects: objects, background: background);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public bool IsPaused { get; set; }

        public Background Background { get; private set; }

        public event Action<Element, Element> OnElementHit;
        public event Action<Element> OnElementDied;
        public event Action<EphemerialElement> OnAddEphemerial;

        public IEnumerable<Element> WithinWindow(float x, float y, float z, float width, float height, float depth)
        {
            // return objects that are within the bounding box
            var x1 = x - width / 2;
            var y1 = y - height / 2;
            var x2 = x + width / 2;
            var y2 = y + height / 2;
            var z1 = z - depth / 2;
            var z2 = z + depth / 2;

            // iterate through all objects (obstacles + items)
            foreach (var region in new RegionCollection[] {Items, Obstacles })
            {
                foreach (var elem in region.Values(x1, y1, z1, x2, y2, z2))
                {
                    if (elem.IsDead) continue;

                    // check that they within the bounds of z
                    if (((depth / 2) + (elem.Depth / 2)) < Math.Abs(z - elem.Z)) continue;

                    // check they are within the bounds of x,y
                    // NOTE: there is a race here, elements may be moving while comparing
                    var ex = elem.X;
                    var ey = elem.Y;
                    var x3 = ex - elem.Width / 2;
                    var y3 = ey - elem.Height / 2;
                    var x4 = ex + elem.Width / 2;
                    var y4 = ey + elem.Height / 2;

                    if (Collision.IntersectingRectangles(x1, y1, x2, y2,
                        x3, y3, x4, y4))
                    {
                        yield return elem;
                    }
                }
            }
        }

        public bool Move(Player player, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace)
        {
            var guard = GetLock(player);
            lock (guard)
            {
                if (WhatWouldPlayerTouch(player, ref xdelta, ref ydelta, ref zdelta, out touching, pace))
                {
                    // successfully checked, and there is no object touching
                    if (touching == null)
                    {
                        return MoveAbsolute(player, player.X + xdelta, player.Y + ydelta, player.Z + zdelta);
                    }
                }

                return false;
            }
        }

        public bool MoveAbsolute(Player player, float x, float y, float z)
        {
            var guard = GetLock(player);
            lock (guard)
            {
                // get region before move
                var beforeRegion = Obstacles.GetRegion(player);

                // move the player
                player.X = x;
                player.Y = y;
                player.Z = z;

                // get region after move
                var afterRegion = Obstacles.GetRegion(player);
                if (!beforeRegion.Equals(afterRegion))
                {
                    Obstacles.Move(player.Id, beforeRegion, afterRegion);
                }
            }
            return true;
        }

        public bool WhatWouldPlayerTouch(Element elem, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace = 0)
        {
            // return no object
            touching = null;

            if (elem.IsDead) return false;
            if (IsPaused) return false;

            if (pace == Constants.DefaultPace) pace = Background.Pace(elem.X, elem.Y);
            if (pace < Constants.MinSpeedMultiplier) pace = Constants.MinSpeedMultiplier;
            float speed = Constants.Speed * pace;

            // allow for player defined speed
            if (elem is Player player) speed = player.Speed * pace;

            // check if the delta is legal
            if (Math.Abs(xdelta) + Math.Abs(ydelta) + Math.Abs(zdelta) > 1.00001) return false;

            // adjust for speed
            xdelta *= speed;
            ydelta *= speed;
            zdelta *= speed;

            // return if the player would collide with an object
            touching = RetrieveWhatPlayerIsTouching(elem, false /* consider acquirable */, xdelta, ydelta, zdelta);

            // return that we actually checked
            return true;
        }

        public List<Player> WhatPlayersAreTouching(Element elem)
        {
            var players = new List<Player>();
            try
            {
                PlayersLock.EnterReadLock();
                // check if one of the bots is hidden by this object
                foreach (var player in Players.Values)
                {
                    // don't care about dead players
                    if (player.IsDead) continue;
                    // check if touching
                    if (IsTouching(player, elem)) players.Add(player);
                }
            }
            finally
            {
                PlayersLock.ExitReadLock();
            }

            return players;
        }

        public Type Pickup(Player player)
        {
            if (player.IsDead) return null;
            if (IsPaused) return null;

            var guard = GetLock(player);
            lock (guard)
            {
                // see if we are over an item
                Element item = RetrieveWhatPlayerIsTouching(player, true /* consider acquirable */);

                if (item != null)
                {
                    // remove as an atomic operation
                    if (Items.Remove(item.Id, item))
                    {
                        // pickup the item
                        if (player.Take(item))
                        {
                            return item.GetType();
                        }

                        // todo should the player drop primary to pick this up?

                        // failed to pick up the item... add it back
                        Items.Add(item.Id, item);
                    }
                }

                return null;
            }
        }

        // player is the one attacking
        public AttackStateEnum Attack(Player player)
        {
            if (player.IsDead) return AttackStateEnum.None;
            if (IsPaused) return AttackStateEnum.None;

            var guard = GetLock(player);
            lock (guard)
            {
                List<Element> hit = null;
                List<ShotTrajectory> trajectories = null;
                var state = AttackStateEnum.None;
                Tool weapon = null;

                state = player.Attack();

                // apply state change
                if (state == AttackStateEnum.Fired)
                {
                    if (player.Primary == null || !(player.Primary is RangeWeapon)) throw new Exception("Must have a RangeWeapon to fire");
                    weapon = player.Primary as RangeWeapon;

                    // fire the weapon (via a trajectory)
                    TrackAttackTrajectory(player, weapon, out hit, out trajectories);
                }
                else if (state == AttackStateEnum.Melee)
                {
                    // use either fists, or if the Primary provides damage
                    weapon = (player.Primary != null && player.Primary is Tool) ? player.Primary as Tool : player.Fists;

                    // swing the tool (via a trajectory)
                    TrackAttackTrajectory(player, weapon, out hit, out trajectories);

                    // disregard any trajectories (there is no visible sign of the tool)
                    trajectories.Clear();
                }

                // send notifications
                bool targetDied = false; // used to change the fired state
                bool targetHit = false;
                if (hit != null && hit.Count > 0)
                {
                    foreach (var elem in hit)
                    {
                        // skip elements that have died
                        if (elem.IsDead) continue;

                        // apply damage
                        if (elem.TakesDamage) elem.ReduceHealth(weapon.Damage);

                        // indicate that there was a successful hit and notify
                        targetHit = true;
                        if (OnElementHit != null) OnElementHit(player, elem);

                        // if the damage killed the element, then notify
                        if (elem.IsDead)
                        {
                            targetDied = true;

                            // increment kills
                            if (elem is Player) player.Kills++;

                            if (OnElementDied != null) OnElementDied(elem);
                        }
                    }
                }

                // add bullet trajectories
                if (trajectories != null && trajectories.Count > 0)
                {
                    foreach (var t in trajectories)
                    {
                        if (OnAddEphemerial != null) OnAddEphemerial(t);
                    }
                }

                // adjust state accordingly
                if (state == AttackStateEnum.Melee)
                {
                    // used fists
                    if (targetDied) state = AttackStateEnum.MeleeAndKilled;
                    else if (targetHit) state = AttackStateEnum.MeleeWithContact;
                }
                else
                {
                    // used a gun
                    if (targetDied) state = AttackStateEnum.FiredAndKilled;
                    else if (targetHit) state = AttackStateEnum.FiredWithContact;
                }

                return state;
            }
        }

        public Type Drop(Player player)
        {
            if (IsPaused) return null;
            // this action is allowed for a dead player

            var guard = GetLock(player);
            lock (guard)
            {
                var item = player.DropPrimary();

                if (item != null)
                {
                    item.X = player.X;
                    item.Y = player.Y;
                    item.Z = player.Z;
                    Items.Add(item.Id, item);

                    return item.GetType();
                }

                return null;
            }
        }

        public bool Place(Player player)
        {
            if (IsPaused) return false;
            if (player.IsDead) return false;

            var guard = GetLock(player);
            lock (guard)
            {
                var item = player.DropPrimary();

                if (item != null)
                {
                    // modify the item to be stationary
                    item.MakeStationary();

                    // todo: Z?
                    // place the item a the end of their hand
                    float x1, y1, x2, y2;
                    Collision.CalculateLineByAngle(player.X, player.Y, player.Angle, player.Width, out x1, out y1, out x2, out y2);

                    item.Move(xDelta: x2 - item.X, yDelta: y2 - item.Y, zDelta: item.Z);

                    // add it to the map as stationary
                    AddItem(item);

                    return true;
                }

                return false;
            }
        }

        public bool ReduceHealth(Player player, float damage)
        {
            if (IsPaused) return false;
            var guard = GetLock(player);
            lock (guard)
            {
                player.ReduceHealth(damage);
                return true;
            }
        }

        public AttackStateEnum Reload(Player player)
        {
            if (IsPaused) return AttackStateEnum.None;
            var guard = GetLock(player);
            lock (guard)
            {
                return player.Reload();
            }
        }

        public bool SwitchPrimary(Player player)
        {
            if (IsPaused) return false;
            var guard = GetLock(player);
            lock (guard)
            {
                return player.SwitchPrimary();
            }
        }

        public bool Turn(Player player, float yaw, float pitch, float roll)
        {
            if (IsPaused) return false;
            var guard = GetLock(player);
            lock (guard)
            {
                player.Angle = yaw;
                player.PitchAngle = pitch;
                return true;
            }
        }

        public bool UpdateBackground(bool applydamage)
        {
            if (IsPaused) return false;

            // update
            Background.Update();

            // apply damages
            if (applydamage)
            {
                var deceased = new List<Element>();
                try
                {
                    PlayersLock.EnterReadLock();
                    foreach (var player in Players.Values)
                    {
                        if (player == null || player.IsDead) continue;
                        var damage = Background.Damage(player.X, player.Y);
                        if (damage > 0)
                        {
                            ReduceHealth(player, damage);

                            if (player.IsDead)
                            {
                                deceased.Add(player);
                            }
                        }
                    }
                }
                finally
                {
                    PlayersLock.ExitReadLock();
                }

                // notify the deceased
                foreach (var elem in deceased)
                {
                    // this player has died as a result of taking damage from the zone
                    if (OnElementDied != null) OnElementDied(elem);
                }
            }

            return true;
        }

        public bool AddItem(Element item)
        {
            if (item != null)
            {
                if (item is EphemerialElement eph)
                {
                    if (OnAddEphemerial != null) OnAddEphemerial(eph);
                }
                else if (item.CanAcquire)
                {
                    Items.Add(item.Id, item);
                }
                else
                {
                    Obstacles.Add(item.Id, item);
                }

                // check if this is a player
                if (item is Player)
                {
                    try
                    {
                        PlayersLock.EnterWriteLock();
                        Players.Add(item.Id, item as Player);
                    }
                    finally
                    {
                        PlayersLock.ExitWriteLock();
                    }
                }

                return true;
            }

            return false;
        }

        public bool RemoveItem(Element item)
        {
            if (item != null)
            {
                if (item.CanAcquire)
                {
                    Items.Remove(item.Id, item);
                }
                else
                {
                    Obstacles.Remove(item.Id, item);
                }

                // check if this is a player
                if (item is Player)
                {
                    try
                    {
                        PlayersLock.EnterWriteLock();
                        Players.Remove(item.Id);
                    }
                    finally
                    {
                        PlayersLock.ExitWriteLock();
                    }
                }

                return true;
            }

            return false;
        }

        public Player GetPlayer(int id)
        {
            try
            {
                PlayersLock.EnterReadLock();
                if (!Players.TryGetValue(id, out Player player)) return null;
                return player;
            }
            finally
            {
                PlayersLock.ExitReadLock();
            }
        }

        public MapStats GetStats()
        {
            var stats = new MapStats();
            try
            {
                PlayersLock.EnterReadLock();
                stats.PlayerCount = Players.Keys.Count;
                foreach (var player in Players.Values) if (!player.IsDead) stats.PlayersAlive++;
            }
            finally
            {
                PlayersLock.ExitReadLock();
            }
            return stats;
        }

        public virtual bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            // check that they intersect on the depth plane
            if (((elem1.Depth / 2) + (elem2.Depth / 2)) < Math.Abs((elem1.Z + z1delta) - elem2.Z)) return false;

            float x1 = (elem1.X + x1delta) - (elem1.Width / 2);
            float y1 = (elem1.Y + y1delta) - (elem1.Height / 2);
            float x2 = (elem1.X + x1delta) + (elem1.Width / 2);
            float y2 = (elem1.Y + y1delta) + (elem1.Height / 2);

            float x3 = (elem2.X) - (elem2.Width / 2);
            float y3 = (elem2.Y) - (elem2.Height / 2);
            float x4 = (elem2.X) + (elem2.Width / 2);
            float y4 = (elem2.Y) + (elem2.Height / 2);

            return Collision.IntersectingRectangles(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        #region private
        // objects that have hit boxes
        private RegionCollection Obstacles;
        // items that can be acquired
        private RegionCollection Items;
        // palyers
        private Dictionary<int /*id*/, Player> Players;
        private ReaderWriterLockSlim PlayersLock;
        private ReaderWriterLockSlim LocksLock;
        private Dictionary<int, object> Locks;

        protected void Initialize(int width, int height, int depth, Player[] players, Element[] objects, Background background)
        {
            if (width <= 0 || height <= 0 || depth <= 0) throw new Exception("Must specific a valid Width, Height, and Depth");
            if (background == null) throw new Exception("Must create a background");

            // init
            IsPaused = true;
            Width = width;
            Height = height;
            Depth = depth;
            Background = background;
            Locks = new Dictionary<int, object>();
            LocksLock = new ReaderWriterLockSlim();
            PlayersLock = new ReaderWriterLockSlim();

            // seperate the items from the obstacles (used to reduce what is considered, and for ordering)
            var obstacles = new List<Element>();
            var items = new List<Element>();
            foreach(var o in objects)
            {
                if (o.CanAcquire) items.Add(o);
                else obstacles.Add(o);
            }

            // add all things to the map
            Obstacles = new RegionCollection(obstacles, Width, Height, Depth);
            Items = new RegionCollection(items, Width, Height, Depth);

            // add all the players
            Players = new Dictionary<int, Player>();
            foreach(var player in players)
            {
                Obstacles.Add(player.Id, player);
                Players.Add(player.Id, player);
            }
        }

        protected virtual bool TrackAttackTrajectory(Player player, Tool weapon, out List<Element> hit, out List<ShotTrajectory> trajectories)
        {
            // init
            hit = new List<Element>();
            trajectories = new List<ShotTrajectory>();

            // if this is a spread weapon, apply additional trajectories
            var angles = new List<float>() { player.Angle };
            if (weapon.Spread != 0)
            {
                angles.Add(player.Angle - (weapon.Spread / 2) );
                angles.Add(player.Angle + (weapon.Spread / 2));
            }

            // apply trajectories
            foreach (var angle in angles)
            {
                // calcualte the line that represents the trajectory
                float x1, y1, x2, y2;
                Collision.CalculateLineByAngle(player.X, player.Y, angle, weapon.Distance, out x1, out y1, out x2, out y2);

                // find what was hit
                var elem = LineIntersectingRectangle(player, x1, y1, x2, y2, out float distance);

                if (elem != null)
                {
                    // reduce the visual shot on screen based on where the bullet hit
                    Collision.CalculateLineByAngle(player.X, player.Y, angle, distance, out x1, out y1, out x2, out y2);

                    hit.Add(elem);
                }

                // add bullet effect
                trajectories.Add(new ShotTrajectory()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Damage = weapon.Damage
                });
            }

            return hit.Count > 0;
        }

        private Element RetrieveWhatPlayerIsTouching(Element primaryElem, bool considerAquireable = false, float xdelta = 0, float ydelta = 0, float zdelta = 0)
        {
            float x1 = (primaryElem.X + xdelta) - (primaryElem.Width / 2);
            float y1 = (primaryElem.Y + ydelta) - (primaryElem.Height / 2);
            float z1 = (primaryElem.Z + zdelta) - (primaryElem.Depth / 2);
            float x2 = (primaryElem.X + xdelta) + (primaryElem.Width / 2);
            float y2 = (primaryElem.Y + ydelta) + (primaryElem.Height / 2);
            float z2 = (primaryElem.Z + zdelta) + (primaryElem.Depth / 2);

            // either choose to iterate through solid objects (obstacles) or items
            RegionCollection objects = Obstacles;
            if (considerAquireable) objects = Items;

            // check collisions
            foreach (var elem in objects.Values(x1, y1, z1, x2, y2, z2))
            {
                // check if we should consider this object
                if (elem.Id == primaryElem.Id) continue;
                if (elem.IsDead) continue;
                if (!considerAquireable)
                {
                    if (!elem.IsSolid || elem.CanAcquire) continue;
                }
                else
                {
                    if (!elem.CanAcquire) continue;
                }

                // check that they intersect on the depth plane
                if (IsTouching(primaryElem, elem, xdelta, ydelta, zdelta)) return elem;
            }

            return null;
        }

        private Element LineIntersectingRectangle(Player player, float x1, float y1, float x2, float y2, out float distance)
        {
            // init
            distance = Single.MaxValue;

            // must ensure to find the closest object that intersects
            Element item = null;
            float z1 = Constants.Ground;
            float z2 = Constants.Sky;

            // check collisions
            foreach (var elem in Obstacles.Values(x1, y1, z1, x2, y2, z2))
            {
                if (elem.Id == player.Id) continue;
                if (elem.IsDead) continue;
                if (!elem.IsSolid || elem.CanAcquire) continue;

                // check if the line intersections this objects hit box
                // after it has moved
                var tdistance = Collision.LineIntersectingRectangle(
                    x1, y1, x2, y2, // line
                    elem.X, elem.Y, elem.Width, elem.Height // element
                    );
                if (tdistance > 0f)
                {
                    // check if this is the closest collision
                    if (tdistance < distance)
                    {
                        item = elem;
                        distance = tdistance;
                    }
                }
            }

            return item;
        }

        private object GetLock(Element elem)
        {
            if (elem == null) throw new Exception("Must provide an non-null element");

            object obj = null;
            try
            {
                LocksLock.EnterUpgradeableReadLock();

                if (!Locks.TryGetValue(elem.Id, out obj))
                {
                    try
                    {
                        LocksLock.EnterWriteLock();
                        obj = new object();
                        Locks.Add(elem.Id, obj);
                    }
                    finally
                    {
                        LocksLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                LocksLock.ExitUpgradeableReadLock();
            }

            return obj;
        }
        #endregion
    }
}

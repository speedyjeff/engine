using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using engine.Common.Entities;

namespace engine.Common
{
    class Map
    {
        public Map(int width, int height, Element[] objects, Background background)
        {
            if (width <= 0 || height <= 0) throw new Exception("Must specific a valid Width and Height");
            if (background == null) throw new Exception("Must create a background");

            // init
            Width = width;
            Height = height;
            Background = background;

            // seperate the items from the obstacles (used to reduce what is considered, and for ordering)
            var obstacles = (objects != null) ? objects.Where(o => !o.CanAcquire) : new List<Element>();
            var items = (objects != null) ? objects.Where(o => o.CanAcquire) : new List<Element>();

            // add all things to the map
            Obstacles = new RegionCollection(obstacles, Width, Height, depth: (int)Constants.Sky);
            Items = new RegionCollection(items, Width, Height, depth: (int)Constants.Sky);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsPaused { get; set; }

        public Background Background { get; private set; }

        public event Action<EphemerialElement> OnEphemerialEvent;
        public event Action<Player, Element> OnElementHit;
        public event Action<Element> OnElementDied;

        public IEnumerable<Element> WithinWindow(float x, float y, float width, float height)
        {
            // do not take Z into account, as the view should be unbostructed (top down)

            // return objects that are within the window
            var x1 = x - width / 2;
            var y1 = y - height / 2;
            var x2 = x + width / 2;
            var y2 = y + height / 2;
            var z1 = Constants.Ground;
            var z2 = Constants.Sky;

            // iterate through all objects (obstacles + items)
            foreach (var region in new RegionCollection[] {Items, Obstacles })
            {
                foreach (var elem in region.Values(x1, y1, x2, y2, z1, z2))
                {
                    if (elem.IsDead) continue;

                    var x3 = elem.X - elem.Width / 2;
                    var y3 = elem.Y - elem.Height / 2;
                    var x4 = elem.X + elem.Width / 2;
                    var y4 = elem.Y + elem.Height / 2;

                    if (Collision.IntersectingRectangles(x1, y1, x2, y2,
                        x3, y3, x4, y4))
                    {
                        yield return elem;
                    }
                }
            }
        }

        public bool Move(Player player, ref float xdelta, ref float ydelta, out Element touching, float pace = 0)
        {
            if (WhatWouldPlayerTouch(player, ref xdelta, ref ydelta, out touching, pace))
            {
                // successfully checked, and there is no object touching
                if (touching == null)
                {
                    // get region before move
                    var beforeRegion = Obstacles.GetRegion(player);

                    // move the player
                    player.Move(xdelta, ydelta);

                    // get region after move
                    var afterRegion = Obstacles.GetRegion(player);
                    if (!beforeRegion.Equals(afterRegion))
                    {
                        Obstacles.Move(player.Id, beforeRegion, afterRegion);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool WhatWouldPlayerTouch(Player player, ref float xdelta, ref float ydelta, out Element touching, float pace = 0)
        {
            // return no object
            touching = null;

            if (player.IsDead) return false;
            if (IsPaused) return false;

            if (pace == 0) pace = Background.Pace(player.X, player.Y);
            if (pace < Constants.MinSpeedMultiplier) pace = Constants.MinSpeedMultiplier;
            if (pace > Constants.MaxSpeedMultiplier) pace = Constants.MaxSpeedMultiplier;
            float speed = Constants.Speed * pace;

            // check if the delta is legal
            if (Math.Abs(xdelta) + Math.Abs(ydelta) > 1.00001) return false;

            // adjust for speed
            xdelta *= speed;
            ydelta *= speed;

            // return if the player would collide with an object
            touching = IntersectingRectangles(player, false /* consider acquirable */, xdelta, ydelta);

            // return that we actually checked
            return true;
        }

        public Type Pickup(Player player)
        {
            if (player.Z != Constants.Ground) return null;
            if (player.IsDead) return null;
            if (IsPaused) return null;

            // see if we are over an item
            Element item = IntersectingRectangles(player, true /* consider acquirable */);

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
                }
            }

            return null;
        }

        // player is the one attacking
        public AttackStateEnum Attack(Player player)
        {
            if (player.Z != Constants.Ground) return AttackStateEnum.None;
            if (player.IsDead) return AttackStateEnum.None;
            if (IsPaused) return AttackStateEnum.None;

            var hit = new HashSet<Element>();
            var state = AttackStateEnum.None;
            var trajectories = new List<ShotTrajectory>();

            state = player.Attack();

            // apply state change
            if (state == AttackStateEnum.Fired)
            {
                if (!(player.Primary is RangeWeapon)) throw new Exception("Must have a Gun to fire");
                var gun = player.Primary as RangeWeapon;
                Element elem = null;

                // apply the bullet via the trajectory
                elem = TrackAttackTrajectory(player, gun, player.X, player.Y, player.Angle, trajectories);
                if (elem != null) hit.Add(elem);
                if (gun.Spread != 0)
                {
                    elem = TrackAttackTrajectory(player, gun, player.X, player.Y, player.Angle - (gun.Spread / 2), trajectories);
                    if (elem != null) hit.Add(elem);
                    elem = TrackAttackTrajectory(player, gun, player.X, player.Y, player.Angle + (gun.Spread / 2), trajectories);
                    if (elem != null) hit.Add(elem);
                }
            }
            else if (state == AttackStateEnum.Melee)
            {
                // project out a short range and check if there was contact
                Element elem = null;

                // use either fists, or if the Primary provides damage
                Tool weapon = (player.Primary != null && player.Primary is Tool) ? player.Primary as Tool : player.Fists;

                // apply the bullet via the trajectory
                elem = TrackAttackTrajectory(player, weapon, player.X, player.Y, player.Angle, trajectories);
                if (elem != null) hit.Add(elem);

                // disregard any trajectories
                trajectories.Clear();
            }

            // send notifications
            bool targetDied = false; // used to change the fired state
            bool targetHit = false;
            foreach (var elem in hit)
            {
                targetHit = true;

                if (OnElementHit != null) OnElementHit(player, elem);

                if (elem.IsDead)
                {
                    // increment kills
                    if (elem is Player) player.Kills++;

                    if (OnElementDied != null) OnElementDied(elem);

                    if (OnEphemerialEvent != null)
                    {
                        OnEphemerialEvent(new OnScreenText()
                        {
                            Text = string.Format("Player {0} killed {1}", player.Name, elem.Name)
                        });
                    }
                }
            }

            // add bullet trajectories
            foreach(var t in trajectories)
            {
                if (OnEphemerialEvent != null)
                {
                    OnEphemerialEvent(t);
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

        public Type Drop(Player player)
        {
            if (player.Z != Constants.Ground) return null;
            if (IsPaused) return null;
            // this action is allowed for a dead player

            var item = player.DropPrimary();

            if (item != null)
            {
                item.X = player.X;
                item.Y = player.Y;
                Items.Add(item.Id, item);

                return item.GetType();
            }

            return null;
        }

        public bool AddItem(Element item)
        {
            if (item != null)
            {
                if (item.CanAcquire)
                {
                    Items.Add(item.Id, item);
                }
                else
                {
                    Obstacles.Add(item.Id, item);
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

                return true;
            }

            return false;
        }

        public bool IsTouching(Element elem1, Element elem2)
        {
            if (elem1.Z != elem2.Z) return false;

            float x1 = (elem1.X) - (elem1.Width / 2);
            float y1 = (elem1.Y) - (elem1.Height / 2);
            float x2 = (elem1.X) + (elem1.Width / 2);
            float y2 = (elem1.Y) + (elem1.Height / 2);

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

        private Element IntersectingRectangles(Player player, bool considerAquireable = false, float xdelta = 0, float ydelta = 0)
        {
            float x1 = (player.X + xdelta) - (player.Width / 2);
            float y1 = (player.Y + ydelta) - (player.Height / 2);
            float x2 = (player.X + xdelta) + (player.Width / 2);
            float y2 = (player.Y + ydelta) + (player.Height / 2);
            float z1 = Constants.Ground;
            float z2 = Constants.Sky;

            // either choose to iterate through solid objects (obstacles) or items
            RegionCollection objects = Obstacles;
            if (considerAquireable) objects = Items;

            // check collisions
            foreach (var elem in objects.Values(x1, y1, x2, y2, z1, z2))
            {
                if (elem.Id == player.Id) continue;
                if (elem.IsDead) continue;
                if (!considerAquireable)
                {
                    if (!elem.IsSolid || elem.CanAcquire) continue;
                }
                else
                {
                    if (!elem.CanAcquire) continue;
                }

                // only consider items that are within the same plane
                if (elem.Z >= player.Z)
                {
                    float x3 = elem.X - (elem.Width / 2);
                    float y3 = elem.Y - (elem.Height / 2);
                    float x4 = elem.X + (elem.Width / 2);
                    float y4 = elem.Y + (elem.Height / 2);

                    // check if these collide
                    if (Collision.IntersectingRectangles(x1, y1, x2, y2, x3, y3, x4, y4)) return elem;
                }
            }

            return null;
        }

        private float DistanceToObject(Element elem1, Element elem2)
        {
            return Collision.DistanceToObject(elem1.X, elem1.Y, elem1.Width, elem1.Height,
                elem2.X, elem2.Y, elem2.Width, elem2.Height);
        }

        private Element LineIntersectingRectangle(Player player, float x1, float y1, float x2, float y2)
        {
            // must ensure to find the closest object that intersects
            Element item = null;
            float prvDistance = 0;
            float z1 = Constants.Ground;
            float z2 = Constants.Sky;

            // check collisions
            foreach (var elem in Obstacles.Values(x1, y1, x2, y2, z1, z2))
            {
                if (elem.Id == player.Id) continue;
                if (elem.IsDead) continue;
                if (!elem.IsSolid || elem.CanAcquire) continue;

                // check if the line intersections this objects hit box
                // after it has moved
                var collision = Collision.LineIntersectingRectangle(
                    x1, y1, x2, y2, // line
                    elem.X, elem.Y, // element
                    elem.Width, elem.Health);

                if (collision)
                {
                    // check if this is the closest collision
                    var distance = DistanceToObject(player, elem);
                    if (item == null || distance < prvDistance)
                    {
                        item = elem;
                        prvDistance = distance;
                    }
                }
            }

            return item;
        }

        private Element TrackAttackTrajectory(Player player, Tool weapon, float x, float y, float angle, List<ShotTrajectory> trajectories)
        {
            float x1, y1, x2, y2;
            Collision.CalculateLineByAngle(x, y, angle, weapon.Distance, out x1, out y1, out x2, out y2);

            // determine damage
            var elem = LineIntersectingRectangle(player, x1, y1, x2, y2);

            if (elem != null)
            {
                // apply damage
                if (elem.TakesDamage)
                {
                    elem.ReduceHealth(weapon.Damage);
                }

                // reduce the visual shot on screen based on where the bullet hit
                var distance = DistanceToObject(player, elem);
                Collision.CalculateLineByAngle(x, y, angle, distance, out x1, out y1, out x2, out y2);
            }

            // add bullet effect
            trajectories.Add( new ShotTrajectory()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Damage = weapon.Damage
                });
 
            return elem;
        }
        #endregion
    }
}

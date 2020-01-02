using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using engine.Common.Entities;
using engine.Common.Entities.AI;

namespace engine.Common
{
    public struct WorldConfiguration
    {
        public int Width;
        public int Height;
        public bool CenterIndicator;
        public Menu StartMenu;
        public Menu EndMenu;
        public Menu HUD;
        public bool EnableZoom;
        public bool DisplayStats;
        public bool ShowCoordinates;
        public bool ApplyForces;
    }

    public class World : IUserInteraction
    {
        // Configuration
        //   Center marker
        //   HUD
        //   Zoom
        public World(WorldConfiguration config, Player[] players, Element[] objects, Background background)
        {
            // sanity check the parameters
            if (config.Width <= 0 || config.Width <= 0) throw new Exception("Must specify a Width and Height for the world");
            if (background == null) throw new Exception("Must specify a background");
            if (players == null) throw new Exception("Must specify a list of players");

            // init
            Ephemerial = new List<EphemerialElement>();
            ZoomFactor = 1;
            Config = config;
            Details = new Dictionary<int, PlayerDetails>();
            Alive = 0;
            UniquePlayers = new HashSet<int>();

            // setup map
            Map = new Map(Config.Width, Config.Height, objects, background);

            // hook up map callbacks
            Map.OnEphemerialEvent += AddEphemerialElement;
            Map.OnElementHit += HitByAttack;
            Map.OnElementDied += PlayerDied;

            // setup the human first
            foreach (var player in players)
            {
                // assign human
                if (Human == null && !(player is AI))
                {
                    Human = player;
                    break;
                }
            }

            if (Human == null) throw new Exception("Must add at least 1 player (as human)");

            // add all the players
            foreach (var player in players) AddItem(player);

            // setup window (based on placement on the map)
            WindowX = Human.X;
            WindowY = Human.Y;
            if (Human.Z > Constants.Ground) ZoomFactor = 0.05f;

            // start paused
            if (Config.StartMenu != null)
            {
                Menu = Config.StartMenu;
                Map.IsPaused = true;
            }

            // setup the background update timer
            BackgroundTimer = new Timer(BackgroundUpdate, null, 0, Constants.GlobalClock);
        }

        public void InitializeGraphics(IGraphics surface, ISounds sounds)
        {
            // graphics
            Surface = surface;
            Surface.SetTranslateCoordinates(TranslateCoordinates);

            // sounds
            Sounds = sounds;

            // initially render all the elements
            Paint();
        }

        public delegate bool BeforeKeyPressedDelegate(Player player, ref char key);

        public event Func<Menu> OnPaused;
        public event Action OnResumed;
        public event Action<Player, Element> OnContact;
        public event Action<Player, Element> OnAttack;
        public event Action<Element> OnDeath;
        public event BeforeKeyPressedDelegate OnBeforeKeyPressed;
        public event Func<Player, char, bool> OnAfterKeyPressed;
        public event Action<Player, ActionDetails> OnBeforeAction;
        public event Action<Player, ActionEnum, bool> OnAfterAction;

        public int Width { get { return Map.Width;  } }
        public int Height {  get { return Map.Height;  } }

        public Player Human { get; private set; }
        public int Alive { get; private set; }
        public int Players { get { return UniquePlayers.Count; } }

        public void Paint()
        {
            // exit early if there is no Surface to draw too
            if (Surface == null) return;

            // draw the map
            Map.Background.Draw(Surface);

            // add center indicator
            if (Human.Z == Constants.Ground && Config.CenterIndicator)
            {
                var centerAngle = Collision.CalculateAngleFromPoint(Human.X, Human.Y, Map.Width / 2, Map.Height / 2);
                float x1, y1, x2, y2;
                var distance = Math.Min(Surface.Width, Surface.Height) * 0.9f;
                Collision.CalculateLineByAngle(Surface.Width / 2, Surface.Height / 2, centerAngle, (distance / 2), out x1, out y1, out x2, out y2);
                Surface.DisableTranslation();
                {
                    // draw an arrow
                    var endX = x2;
                    var endY = y2;
                    x1 = endX;
                    y1 = endY;
                    Collision.CalculateLineByAngle(x1, y1, (centerAngle + 180) % 360, 50, out x1, out y1, out x2, out y2);
                    Surface.Line(RGBA.Black, x1, y1, x2, y2, 10);

                    x1 = endX;
                    y1 = endY;
                    Collision.CalculateLineByAngle(x1, y1, (centerAngle + 135) % 360, 25, out x1, out y1, out x2, out y2);
                    Surface.Line(RGBA.Black, x1, y1, x2, y2, 10);

                    x1 = endX;
                    y1 = endY;
                    Collision.CalculateLineByAngle(x1, y1, (centerAngle + 225) % 360, 25, out x1, out y1, out x2, out y2);
                    Surface.Line(RGBA.Black, x1, y1, x2, y2, 10);
                }
                Surface.EnableTranslation();
            }

            // draw all elements
            var hidden = new HashSet<int>();
            var visiblePlayers = new List<Player>();
            foreach (var elem in Map.WithinWindow(Human.X, Human.Y, Human.Z, Surface.Width * (1 / ZoomFactor), Surface.Height * (1 / ZoomFactor), depth: Constants.Sky))
            {
                if (elem.IsDead) continue;
                else if (elem is Player)
                {
                    visiblePlayers.Add(elem as Player);
                    continue;
                }
                else if (elem.IsTransparent)
                {
                    // if the player is intersecting with this item, then do not display it
                    if (Map.IsTouching(Human, elem)) continue;

                    lock (Details)
                    {
                        // check if one of the bots is hidden by this object
                        foreach (var detail in Details.Values)
                        {
                            // don't care about dead players
                            if (detail.Player.IsDead) continue;
                            // already hidden, do not need to recheck
                            if (hidden.Contains(detail.Player.Id)) continue;
                            // check
                            if (detail.Player is AI)
                            {
                                if (Map.IsTouching(detail.Player, elem))
                                {
                                    // this player is hidden
                                    hidden.Add(detail.Player.Id);
                                }
                            }
                        }
                    } // lock(Details)
                }

                // draw
                elem.Draw(Surface);
            }

            // draw the players
            foreach (var player in visiblePlayers)
            {
                if (hidden.Contains(player.Id)) continue;
                player.Draw(Surface);
            }

            // add any ephemerial elements
            lock (Ephemerial)
            {
                var toremove = new List<EphemerialElement>();
                var messageShown = false;
                foreach (var b in Ephemerial)
                {
                    if (b is OnScreenText)
                    {
                        // only show one message at a time
                        if (messageShown) continue;
                        messageShown = true;
                    }
                    b.Draw(Surface);
                    b.Duration--;
                    if (b.Duration < 0) toremove.Add(b);
                }
                foreach (var b in toremove)
                {
                    Ephemerial.Remove(b);
                }
            }

            // display the player counts
            if (Config.HUD != null)
            {
                Surface.DisableTranslation();
                {
                    Config.HUD.Draw(Surface);
                }
                Surface.EnableTranslation();
            }

            // display the player counts
            if (Config.DisplayStats)
            {
                Surface.DisableTranslation();
                {
                    Surface.Text(RGBA.Black, Surface.Width - 200, 10, string.Format("Alive {0} of {1}", Alive, Players));
                    Surface.Text(RGBA.Black, Surface.Width - 200, 30, string.Format("Kills {0}", Human.Kills));
                }
                Surface.EnableTranslation();
            }

            if (Config.ShowCoordinates)
            {
                Surface.DisableTranslation();
                {
                    Surface.Text(RGBA.Black, 200, 10, string.Format("X {0}", Human.X));
                    Surface.Text(RGBA.Black, 200, 30, string.Format("Y {0}", Human.Y));
                }
                Surface.EnableTranslation();
            }

            // show a menu if present
            if (Map.IsPaused && Menu != null)
            {
                Menu.Draw(Surface);
            }
        }

        public void Resize()
        {

        }

        public void KeyPress(char key)
        {
            // inputs that are accepted while a menu is displaying
            if (Map.IsPaused)
            {
                switch(key)
                {
                    // menu
                    case Constants.Esc:
                        HideMenu();
                        break;
                }

                return;
            }

            // menu
            if (key == Constants.Esc)
            {
                ShowMenu();
                return;
            }

            // handle the user input
            bool result = false;
            Type item = null;
            float xdelta = 0;
            float ydelta = 0;

            // pass the key off to the caller to see if they know what to 
            // do in this case
            if (OnBeforeKeyPressed != null)
            {
                if (OnBeforeKeyPressed(Human, ref key)) return;
            }

            // movement determination
            switch (key)
            {
                // move
                case Constants.Down:
                case Constants.Down2:
                case Constants.DownArrow:
                    ydelta = 1;
                    break;
                case Constants.Left:
                case Constants.Left2:
                case Constants.LeftArrow:
                    xdelta = -1;
                    break;
                case Constants.Right:
                case Constants.Right2:
                case Constants.RightArrow:
                    xdelta = 1;
                    break;
                case Constants.Up:
                case Constants.Up2:
                case Constants.UpArrow:
                    ydelta = -1;
                    break;
                case Constants.RightMouse:
                    // use the mouse to move in the direction of the angle
                    float r = (Human.Angle % 90) / 90f;
                    xdelta = 1 * r;
                    ydelta = 1 * (1 - r);
                    if (Human.Angle > 0 && Human.Angle < 90) ydelta *= -1;
                    else if (Human.Angle > 180 && Human.Angle <= 270) xdelta *= -1;
                    else if (Human.Angle > 270) { ydelta *= -1; xdelta *= -1; }
                    break;
            }

            if (OnBeforeAction != null)
            {
                // NOTE: Do not apply the ZoomFactor (to keep the view fair)
                List<Element> elements = Map.WithinWindow(Human.X, Human.Y, Human.Z, Constants.ProximityViewWidth, Constants.ProximityViewHeight, depth: Constants.Sky).ToList();
                var angleToCenter = Collision.CalculateAngleFromPoint(Human.X, Human.Y, Config.Width / 2, Config.Height / 2);
                var inZone = Map.Background.Damage(Human.X, Human.Y) > 0;

                // provide details for telemetry
                OnBeforeAction(Human, new ActionDetails()
                {
                    Elements = elements,
                    AngleToCenter = angleToCenter,
                    InZone = inZone,
                    XDelta = xdelta,
                    YDelta = ydelta,
                    Angle = Human.Angle
                });
            }

            // determine action
            var action = ActionEnum.Move;
            switch (key)
            {
                case Constants.Switch:
                    action = ActionEnum.SwitchPrimary;
                    break;

                case Constants.Pickup:
                case Constants.Pickup2:
                    action = ActionEnum.Pickup;
                    break;

                case Constants.Drop3:
                case Constants.Drop2:
                case Constants.Drop4:
                case Constants.Drop:
                    action = ActionEnum.Drop;
                    break;

                case Constants.Reload:
                case Constants.MiddleMouse:
                    action = ActionEnum.Reload;
                    break;

                case Constants.Space:
                case Constants.LeftMouse:
                    action = ActionEnum.Attack;
                    break;

                case Constants.Jump:
                case Constants.Jump2:
                    action = ActionEnum.Jump;
                    break;
            }

            // take action
            switch (action)
            {
                case ActionEnum.SwitchPrimary:
                    result = SwitchPrimary(Human, out item);
                    Human.Feedback(action, item, result);
                    break;

                case ActionEnum.Pickup:
                    result = Pickup(Human, out item);
                    Human.Feedback(action, item, result);
                    break;

                case ActionEnum.Drop:
                    result = Drop(Human, out item);
                    Human.Feedback(action, item, result);
                    break;

                case ActionEnum.Reload:
                    result = Reload(Human, out AttackStateEnum reloaded);
                    Human.Feedback(action, reloaded, result);
                    break;

                case ActionEnum.Attack:
                    result = Attack(Human, out AttackStateEnum attack);
                    Human.Feedback(action, attack, result);
                    break;

                case ActionEnum.Jump:
                    // ActionEnum.Jump (special)
                    result = Jump(Human);
                    Human.Feedback(action, null, result);
                    break;
            }

            // send after telemetry
            if (OnAfterAction != null && action != ActionEnum.Move) OnAfterAction(Human, action, result);

            // pass the key off to the caller to see if they know what to 
            // do in this case
            if (OnAfterKeyPressed != null)
            {
                if (OnAfterKeyPressed(Human, key)) return;
            }

            // if a move command, then move
            if (xdelta != 0 || ydelta != 0)
            {
                // ActionEnum.Move;
                result = Move(Human, xdelta, ydelta);
                Human.Feedback(ActionEnum.Move, null, result);
                if (OnAfterAction != null) OnAfterAction(Human, ActionEnum.Move, result);
            }
        }

        public void Mousewheel(float delta)
        {
            // disabled by the developer
            if (!Config.EnableZoom) return;

            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // only if on the ground
            if (Human.Z != Constants.Ground) return;

            // adjust the zoom
            if (delta < 0) ZoomFactor -= Constants.ZoomStep;
            else if (delta > 0) ZoomFactor += Constants.ZoomStep;

            // cap the zoom capability
            if (ZoomFactor < Constants.ZoomStep) ZoomFactor = Constants.ZoomStep;
            if (ZoomFactor > Constants.MaxZoomIn) ZoomFactor = Constants.MaxZoomIn;
        }

        public void Mousemove(float x, float y, float angle)
        {
            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // use the angle to turn the human player
            Turn(Human, angle);
        }

        public void Mousedown(MouseButton btn, float x, float y)
        {
            // ignore
        }

        public void Mouseup(MouseButton btn, float x, float y)
        {
            // ignore
        }

        public void AddItem(Element item)
        {
            Map.AddItem(item);

            if (item is Player)
            {
                var details = new PlayerDetails() { Player = (item as Player) };

                // add another alive player
                Alive++;

                lock (Details)
                {
                    // add to the set
                    Details.Add(item.Id, details);
                    UniquePlayers.Add(item.Id);
                }

                // setup humans and AI
                if (item is AI || Config.ApplyForces)
                {
                    details.UpdatePlayerTimer = new Timer(UpdatePlayer, item.Id, 0, Constants.GlobalClock);
                }

                // check that the player is not above the sky
                if (item.Z > Constants.Sky) item.Z = Constants.Sky;

                // initialize parachute (if necessary)
                if (item.Z > Constants.Ground)
                {
                    details.ParachuteTimer = new Timer(PlayerParachute, item.Id, 0, Constants.GlobalClock);
                }
            }
        }

        public void RemoveAllItems(Type type)
        {
            // iterate through players and remove all of this type
            var toRemove = new List<Player>();

            lock (Details)
            {
                foreach (var detail in Details.Values)
                {
                    if (detail.Player != null && detail.Player.GetType() == type)
                    {
                        toRemove.Add(detail.Player);
                    }
                }
            }

            foreach (var player in toRemove)
            {
                RemoveItem(player);
            }
        }

        public void RemoveItem(Element item)
        {
            if (item is Player)
            {
                var player = (item as Player);
                Type type = null;

                // remove an active player
                Alive--;

                // remove from map
                Map.RemoveItem(item);

                // drop ALL the players goodies
                Drop(player, out type);
                for (int i = 0; i < player.HandCapacity; i++)
                {
                    if (SwitchPrimary(player, out type))
                    {
                        Drop(player, out type);
                    }
                }

                // set the players current ranking
                Human.Ranking = Alive;

                // clean up the dead players
                lock (Details)
                {
                    Details.Remove(player.Id);
                }
            }
            else throw new Exception("Invalid item to remove");
        }

        public void Play(string path)
        {
            // play sound
            Stream stream = null;
            if (Media.Sounds.TryGetValue(path, out stream))
            {
                // use the embedded resource
                Sounds.Play(path, stream);
            }
            else
            {
                // load from disk and play
                Sounds.Play(path);
            }
        }

        public void Music(string path, bool repeat)
        {
            // play music
            Sounds.PlayMusic(path, repeat);
        }

        public void ShowMenu(Menu menu)
        {
            if (menu != null)
            {
                Menu = menu;
                ShowMenu();
            }
        }

        public void Teleport(Player player, float x, float y)
        {
            if (player == null || player.IsDead) throw new Exception("Cannot teleport an invalid player");

            // remove the player
            RemoveItem(player);

            // move
            player.X = x;
            player.Y = y;

            if (player.Id == Human.Id)
            {
                WindowX = x;
                WindowY = y;
            }

            // add them back
            AddItem(player);
        }

        #region private
        class PlayerDetails
        {
            public Player Player;
            public Timer ParachuteTimer;
            public Timer UpdatePlayerTimer;
            public int UpdatePlayerLock; // to avoid reentrancy in the timer
            public int PlayerParachuteLock; // to avoid reentrancy in the timer
        }

        private IGraphics Surface;
        private List<EphemerialElement> Ephemerial;
        private float ZoomFactor;
        private ISounds Sounds;
        private Map Map;
        private float WindowX;
        private float WindowY;
        private Dictionary<int, PlayerDetails> Details;
        private Menu Menu;
        private WorldConfiguration Config;
        private Timer BackgroundTimer;
        private HashSet<int> UniquePlayers;

        private const string NothingSoundPath = "nothing";
        private const string PickupSoundPath = "pickup";

        // menu items
        private void ShowMenu()
        {
            if (OnPaused != null)
            {
                Menu = OnPaused();
            }

            if (Menu != null) Map.IsPaused = true;
        }

        private void HideMenu()
        {
            if (OnResumed != null)
            {
                OnResumed();
            }

            Map.IsPaused = false;
        }

        // callbacks to support time lapse actions
        private void PlayerParachute(object state)
        {
            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // execute the parachute
            int id = (int)state;

            // grab the details
            PlayerDetails detail = null;
            lock (Details)
            {
                if (!Details.TryGetValue(id, out detail))
                {
                    // the player must be dead and caused the record to be cleaned up
                    return;
                }
            }

            // the timer is reentrant, so only allow one instance to run
            if (System.Threading.Interlocked.CompareExchange(ref detail.PlayerParachuteLock, 1, 0) != 0) return;

            if (detail.Player.Z <= Constants.Ground)
            {
                // ensure the player is on the ground
                detail.Player.Z = Constants.Ground;
                detail.ParachuteTimer.Dispose();

                // check if the player is touching an object, if so then move
                int count = 100;
                float xstep = 0.01f;
                float xmove = 10f;
                if (detail.Player.X > Map.Width / 2)
                {
                    // move the other way
                    xstep *= -1;
                    xmove *= -1;
                }
                do
                {
                    // apply parachute
                    float xdelta = xstep;
                    float ydelta = 0;
                    if (Move(detail.Player, xdelta, ydelta))
                    {
                        break;
                    }

                    // move over
                    Teleport(detail.Player, detail.Player.X + xmove, detail.Player.Y);
                }
                while (count-- > 0);

                if (count <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to move after parachute");
                }

                return;
            }

            // decend
            detail.Player.Z -= (Constants.ZoomStep/10);

            if (detail.Player.Id == Human.Id)
            {
                // zoom in
                ZoomFactor += (Constants.ZoomStep / 10);
            }

            // set state back to not running
            System.Threading.Volatile.Write(ref detail.PlayerParachuteLock, 0);
        }

        // AI and other updates for players
        private void UpdatePlayer(object state)
        {
            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // move the AI
            int id = (int)state;
            Stopwatch timer = new Stopwatch();

            // grab the details
            PlayerDetails detail = null;
            lock (Details)
            {
                if (!Details.TryGetValue(id, out detail))
                {
                    // the player must be dead and caused the record to be cleaned up
                    return;
                }
            }

            // the timer is reentrant, so only allow one instance to run
            if (System.Threading.Interlocked.CompareExchange(ref detail.UpdatePlayerLock, 1, 0) != 0) return;

            timer.Start();
            // if AI, then query for movement
            if (detail.Player is AI)
            {
                AI ai = detail.Player as AI;
                float xdelta = 0;
                float ydelta = 0;
                float angle = 0;

                if (ai.IsDead)
                {
                    // remove the AI player
                    RemoveItem(ai);

                    // stop the timer
                    detail.UpdatePlayerTimer.Dispose();
                    return;
                }

                // NOTE: Do not apply the ZoomFactor (as it distorts the AI when debugging)
                List<Element> elements = Map.WithinWindow(ai.X, ai.Y, ai.Z, Constants.ProximityViewWidth, Constants.ProximityViewHeight, depth: Constants.Sky).ToList();
                var angleToCenter = Collision.CalculateAngleFromPoint(ai.X, ai.Y, Config.Width / 2, Config.Height / 2);
                var inZone = Map.Background.Damage(ai.X, ai.Y) > 0;

                // get action from AI

                var action = ai.Action(elements, angleToCenter, inZone, ref xdelta, ref ydelta, ref angle);

                // provide details for telemetry
                if (OnBeforeAction != null)
                {
                    OnBeforeAction(ai, new ActionDetails()
                    {
                        Elements = elements,
                        AngleToCenter = angleToCenter,
                        InZone = inZone,
                        XDelta = xdelta,
                        YDelta = ydelta,
                        Angle = angle
                    });
                }

                // turn
                ai.Angle = angle;

                // perform action
                bool result = false;
                Type item = null;
                switch (action)
                {
                    case ActionEnum.Drop:
                        result = Drop(ai, out item);
                        ai.Feedback(action, item, result);
                        break;
                    case ActionEnum.Pickup:
                        result = Pickup(ai, out item);
                        ai.Feedback(action, item, result);
                        break;
                    case ActionEnum.Reload:
                        result = Reload(ai, out AttackStateEnum reloaded);
                        ai.Feedback(action, reloaded, result);
                        break;
                    case ActionEnum.Attack:
                        result = Attack(ai, out AttackStateEnum attack);
                        ai.Feedback(action, attack, result);
                        break;
                    case ActionEnum.SwitchPrimary:
                        result = SwitchPrimary(ai, out item);
                        ai.Feedback(action, item, result);
                        break;
                    case ActionEnum.Jump:
                        result = Jump(ai);
                        ai.Feedback(action, null, result);
                        break;
                    case ActionEnum.Move:
                    case ActionEnum.None:
                        break;
                    default: throw new Exception("Unknown ai action : " + action);
                }

                // send after telemetry
                if (OnAfterAction != null && action != ActionEnum.Move) OnAfterAction(ai, action, result);

                // have the AI move
                float oxdelta = xdelta;
                float oydelta = ydelta;
                var moved = Move(ai, xdelta, ydelta);
                ai.Feedback(ActionEnum.Move, null, moved);
                if (OnAfterAction != null) OnAfterAction(ai, ActionEnum.Move, result);
            }

            // apply forces, if necessary
            if (Config.ApplyForces && detail.Player.CanMove)
            {
                bool inAir = false;
                int retries;
                float dist;
                float pace;

                // apply upward force
                if (detail.Player.YForcePercentage > 0)
                {
                    retries = 3;
                    pace = Constants.YForcePace;
                    dist = -1 * detail.Player.YForcePercentage;
                    do
                    {
                        // degrade the y delta
                        detail.Player.YForcePercentage -= Constants.YForceDegrade;
                        if (detail.Player.YForcePercentage < 0) detail.Player.YForcePercentage = 0;

                        // apply
                        if (Move(detail.Player, 0, dist, pace))
                        {
                            inAir = true;
                            break;
                        }

                        // we are too close, reduce how far we try
                        dist /= 2;
                        pace /= 2;
                    }
                    while (retries-- > 0);
                }

                // apply downward force
                else
                {
                    retries = 3;
                    dist = 1;
                    pace = Constants.YForcePace;
                    do
                    {
                        // apply
                        if (Move(detail.Player, 0, dist, pace))
                        {
                            inAir = true;
                            break;
                        }

                        // we are too close, reduce how far we try
                        dist /= 2;
                        pace /= 2;
                    }
                    while (retries-- > 0);
                }

                // apply a horizontal force, if necessary
                if (inAir && detail.Player.XForcePercentage != 0)
                {
                    retries = 3;
                    pace = Constants.XForcePace;
                    dist = (detail.Player.XForcePercentage < 0) ? -1 : 1;
                    do
                    {
                        // degrade the x delta
                        if (detail.Player.XForcePercentage > 0)
                        {
                            detail.Player.XForcePercentage -= Constants.XForceDegrade;
                            if (detail.Player.XForcePercentage < 0)
                                detail.Player.XForcePercentage = 0;
                        }
                        else if (detail.Player.XForcePercentage < 0)
                        {
                            detail.Player.XForcePercentage += Constants.XForceDegrade;
                            if (detail.Player.XForcePercentage > 0)
                                detail.Player.XForcePercentage = 0;
                        }

                        if (Move(detail.Player, dist, 0, pace))
                            break;
                        // we are too close, reduce how far we try
                        dist /= 2;
                        pace /= 2;
                    }
                    while (retries-- > 0);
                }
            }
            timer.Stop();

            // set state back to not running
            System.Threading.Volatile.Write(ref detail.UpdatePlayerLock, 0);

            if (timer.ElapsedMilliseconds > 100) System.Diagnostics.Debug.WriteLine("**UpdatePlayer Duration {0} ms", timer.ElapsedMilliseconds);
        }

        private void BackgroundUpdate(object state)
        {
            if (Map.IsPaused) return;
            var deceased = new List<Element>();

            // update the map
            Map.Background.Update();

            // apply any necessary damage to the players
            lock (Details)
            {
                foreach (var detail in Details.Values)
                {
                    if (detail.Player == null || detail.Player.IsDead) continue;
                    var damage = Map.Background.Damage(detail.Player.X, detail.Player.Y);
                    if (damage > 0)
                    {
                        detail.Player.ReduceHealth(damage);

                        if (detail.Player.IsDead)
                        {
                            deceased.Add(detail.Player);
                        }
                    }
                }
            }

            // notify the deceased
            foreach (var elem in deceased)
            {
                // this player has died as a result of taking damage from the zone
                PlayerDied(elem);
                AddEphemerialElement(new OnScreenText()
                    {
                        Text = string.Format("Player {0} died in the zone", elem.Name)
                    });
            }
        }

        // support
        private bool TranslateCoordinates(bool autoScale, float x, float y, float width, float height, float other, out float tx, out float ty, out float twidth, out float theight, out float tother)
        {
            // transform the world x,y coordinates into scaled and screen coordinates
            tx = ty = twidth = theight = tother = 0;

            float zoom = (autoScale) ? ZoomFactor : 1;

            // determine scaling factor
            float scale = (1 / zoom);
            width *= zoom;
            height *= zoom;

            // Surface.Width & Surface.Height are the current windows width & height
            float windowHWidth = Surface.Width / 2.0f;
            float windowHHeight = Surface.Height / 2.0f;

            // now translate to the window
            tx = ((x - WindowX) * zoom) + windowHWidth;
            ty = ((y - WindowY) * zoom) + windowHHeight;
            twidth = width;
            theight = height;
            tother = other * zoom;

            return true;
        }

        private void AddEphemerialElement(EphemerialElement element)
        {
            lock (Ephemerial)
            {
                Ephemerial.Add(element);
            }
        }

        // human movements
        private bool SwitchPrimary(Player player, out Type type)
        {
            // no indication of what was switched
            type = null;

            if (player.IsDead) return false;
            return player.SwitchPrimary();
        }

        private bool Pickup(Player player, out Type type)
        {
            // indicate what was picked up
            type = null;

            if (player.IsDead) return false;
            type = Map.Pickup(player);
            if (type != null && player.Id == Human.Id)
            {
                // play sound
                Play(PickupSoundPath);
            }
            return (type != null);
        }

        private bool Drop(Player player, out Type type)
        {
            // indicidate what was dropped
            type = Map.Drop(player);
            return (type != null);
        }

        private bool Reload(Player player, out AttackStateEnum reloaded)
        {
            // return the state
            reloaded = AttackStateEnum.None;

            if (player.IsDead) return false;
            reloaded = player.Reload();

            if (player.Id == Human.Id)
            {
                switch (reloaded)
                {
                    case AttackStateEnum.Reloaded:
                        if (player.Primary is RangeWeapon) Play((player.Primary as RangeWeapon).ReloadSoundPath());
                        else throw new Exception("Invalid action for a non-gun");
                        break;
                    case AttackStateEnum.None:
                    case AttackStateEnum.NoRounds:
                        Play(NothingSoundPath);
                        break;
                    case AttackStateEnum.FullyLoaded:
                        // no sound
                        break;
                    default: throw new Exception("Unknown GunState : " + reloaded);
                }
            }

            return (reloaded == AttackStateEnum.Reloaded); 
        }

        private bool Attack(Player player, out AttackStateEnum attack)
        {
            // return the attack state
            attack = AttackStateEnum.None;

            if (player.IsDead) return false;
            attack = Map.Attack(player);

            if (player.Id == Human.Id)
            {
                // play sounds
                switch (attack)
                {
                    case AttackStateEnum.Melee:
                    case AttackStateEnum.MeleeWithContact:
                    case AttackStateEnum.MeleeAndKilled:
                        Play(player.Fists.UsedSoundPath());
                        break;
                    case AttackStateEnum.FiredWithContact:
                    case AttackStateEnum.FiredAndKilled:
                    case AttackStateEnum.Fired:
                        if (player.Primary is RangeWeapon) Play((player.Primary as RangeWeapon).FiredSoundPath());
                        else throw new Exception("Invalid action for a non-gun");
                        break;
                    case AttackStateEnum.NoRounds:
                    case AttackStateEnum.NeedsReload:
                        if (player.Primary is RangeWeapon) Play((player.Primary as RangeWeapon).EmptySoundPath());
                        else throw new Exception("Invalid action for a non-gun");
                        break;
                    case AttackStateEnum.LoadingRound:
                    case AttackStateEnum.None:
                        Play(NothingSoundPath);
                        break;
                    default: throw new Exception("Unknown GunState : " + attack);
                }
            }

            return (attack == AttackStateEnum.MeleeAndKilled ||
                attack == AttackStateEnum.MeleeWithContact ||
                attack == AttackStateEnum.FiredAndKilled ||
                attack == AttackStateEnum.FiredWithContact);
        }

        private bool Move(Player player, float xdelta, float ydelta, float pace = 0)
        {
            if (player.IsDead) return false;

            Element touching;
            if (Map.Move(player, ref xdelta, ref ydelta, out touching, pace))
            {
                // the move completed
                if (touching != null) throw new Exception("There should not be an object touching");

                if (player.Id == Human.Id)
                {
                    // move the screen
                    WindowX += xdelta;
                    WindowY += ydelta;
                }

                return true;
            }
            else
            {
                // notify that this object has been touched
                if (touching != null && OnContact != null) OnContact(player, touching);

                // TODO may want to move back a bit in the opposite direction
                return false;
            }
        }

        private void Turn(Player player, float angle)
        {
            if (player.IsDead) return;
            player.Angle = angle;
        }

        private bool Jump(Player player)
        {
            if (player.IsDead) return false;
            if (!Config.ApplyForces) return false;

            // check that the player is touching something below them
            var xdelta = 0f;
            var ydelta = Constants.IsTouchingDistance;
            Element touching;
            if (Map.WhatWouldPlayerTouch(player, ref xdelta, ref ydelta, out touching))
            {
                // successfully checked
                if (touching != null && player.Y < touching.Y)
                {
                    // this player is standing on something
                    player.YForcePercentage = player.MaxYForcePercentage; // 0-100%
                }
            }
            return false;
        }

        // callbacks
        private void HitByAttack(Player player, Element element)
        {
            // play sound if the human is hit
            if (element is Player && element.Id == Human.Id)
            {
                Play(Human.HurtSoundPath);
            }

            // notify the outside world that we hit something
            if (OnAttack != null) OnAttack(player, element);
        }

        private void PlayerDied(Element element)
        {
            // check for winner/death (element may be any element that can take damage)
            if (element is Player)
            {
                // remove this player
                RemoveItem(element);

                // track how many players are alive
                lock (Details)
                {
                    var alive = 0;
                    foreach (var elem in Details.Values)
                    {
                        if (elem.Player.IsDead) continue;
                        alive++;
                    }
                    Alive = alive;
                }

                // callback
                if (OnDeath != null)
                {
                    OnDeath(element);
                }
            }
        }
        #endregion
    }
}

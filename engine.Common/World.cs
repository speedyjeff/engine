using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using engine.Common.Entities;
using engine.Common.Entities.AI;
using engine.Common.Entities3D;
using engine.Common.Networking;

namespace engine.Common
{
    public struct WorldConfiguration
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Menu StartMenu { get; set; }
        public Menu HUD { get; set; }
        public bool EnableZoom { get; set; }
        public bool ShowCoordinates { get; set; }
        public int ForcesApplied { get; set; }
        public float HorizonX { get; set; }
        public float HorizonY { get; set; }
        public float HorizonZ { get; set; }
        public bool Is3D { get; set; }
        public float CameraX { get; set; }
        public float CameraY { get; set; }
        public float CameraZ { get; set; }
        public string ServerUrl { get; set; }
        public bool EnableFastPlayerUpdate { get; set; }
    }

    public enum Forces { None = 0, X = 1, Y = 2, Z = 4};

    public class World : IUserInteraction
    {
        // NOTE: DO NOT Cache any content from the Map locally... it may not be the most current 
        public World(WorldConfiguration config, Player[] players, Element[] objects, Background background)
        {
            // sanity check the parameters
            if (config.Width <= 0 || config.Width <= 0) throw new Exception("Must specify a Width and Height for the world");
            if (background == null) throw new Exception("Must specify a background");
            if (players == null) throw new Exception("Must specify a list of players");

            // setup map
            IMap map = null;
            if (!string.IsNullOrWhiteSpace(config.ServerUrl)) map = new RemoteMap(config, (int)Constants.ProximityViewDepth, players, objects, background);
            else if (config.Is3D) map = new Map3D(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);
            else map = new Map(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);

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

            Initialize(config, map, players);

            if (Config.StartMenu != null)
            {
                Menu = Config.StartMenu;
            }
        }

        public World(WorldConfiguration config, IMap map, Player[] players)
        {
            // sanity check the parameters
            if (config.Width <= 0 || config.Width <= 0) throw new Exception("Must specify a Width and Height for the world");
            if (players == null) throw new Exception("Must specify a list of players");

            // there is no the human
            Human = null;

            // the map is already setup
            Initialize(config, map, players);
        }

        public void InitializeGraphics(IGraphics surface, ISounds sounds)
        {
            // graphics
            Surface = new WorldTranslationGraphics(surface);

            // setup ImageSource
            ImageSource.SetGraphics(Surface);

            // sounds
            Sounds = sounds;

            // initially render all the elements
            Paint();
        }

        public delegate bool BeforeKeyPressedDelegate(Player player, ref char key);
        public delegate bool BeforeMousedownDelegate(Element elem, MouseButton btn, float sx, float sy, float wx, float wy, float wz, ref char key);

        public event Func<Menu> OnPaused;
        public event Action OnResumed;
        public event Action<Element, Element> OnContact;
        public event Action<Element, Element> OnAttack;
        public event Action<Element> OnDeath;
        public event BeforeKeyPressedDelegate OnBeforeKeyPressed;
        public event Func<Player, char, bool> OnAfterKeyPressed;
        public event Action<Player, ActionDetails> OnBeforeAction;
        public event Action<Player, ActionEnum, bool> OnAfterAction;
        public event BeforeMousedownDelegate OnBeforeMousedown;
        public event Action<Element, char, bool> OnAfterMousedown;

        public int Width { get { return Map.Width;  } }
        public int Height {  get { return Map.Height;  } }
        public int Depth { get { return Map.Depth; } }

        public Player Human { get; private set; }
        public int Alive { get; private set; }
        public int Players { get { return UniquePlayers.Count; } }

        public void Paint()
        {
            // exit early if there is no Surface to draw too
            if (Surface == null) return;

            // set graphics the perspective
            Surface.SetPerspective(is3D: Config.Is3D,
                centerX: Human.X, centerY: Human.Y, centerZ: Human.Z,
                yaw: Human.Angle, pitch: Human.PitchAngle, roll: 0f,
                cameraX: Config.CameraX, cameraY: Config.CameraY, cameraZ: Config.CameraZ,
                horizon: Config.HorizonZ * 2,
                lod: (Human.Z+Config.CameraZ) / Constants.Sky);

            // draw the map
            Map.Background.Draw(Surface);

            // if 3d, defer the rendering of the polygons to ensure proper ordering
            if (Config.Is3D) Surface.CapturePolygons();

            // draw all elements
            var hidden = new HashSet<int>();
            var visiblePlayers = new List<Player>();

            var ratio = (Human.Z + Config.CameraZ);
            if (Config.Is3D || ratio < 1f) ratio = 1f;
            foreach (var elem in Map.WithinWindow(Human.X, Human.Y, Human.Z,
                Surface.Width * ratio + Config.HorizonX, Surface.Height * ratio + Config.HorizonY, depth: Constants.ProximityViewDepth + Config.HorizonZ))
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
                    // get players that are touching
                    var players = Map.WhatPlayersAreTouching(elem);
                    foreach (var player in players) hidden.Add(player.Id);
                }

                // draw
                elem.Draw(Surface);
            }

            // draw the players (todo - if not on the same plane, they may be draw in wrong order)
            foreach (var player in visiblePlayers)
            {
                if (hidden.Contains(player.Id)) continue;
                player.Draw(Surface);
            }

            // get ephemerals
            try
            {
                EphemerialLock.EnterReadLock();

                // add any ephemeral elements (non-text)
                foreach (var b in Ephemerial)
                {
                    // skip all the messages
                    if (b is OnScreenText) continue;
                    // draw
                    b.Draw(Surface);
                }
            }
            finally
            {
                EphemerialLock.ExitReadLock();
            }

            // if 3d, then render all the polygons (in order)
            if (Config.Is3D) Surface.RenderPolygons();

            try
            {
                EphemerialLock.EnterReadLock();

                // add any ephemeral elements (text only)
                foreach (var b in Ephemerial)
                {
                    // show only 1 message at a time
                    if (b is OnScreenText)
                    {
                        // draw
                        b.Draw(Surface);
                        break;
                    }
                }
            }
            finally
            {
                EphemerialLock.ExitReadLock();
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

            if (Config.ShowCoordinates)
            {
                Surface.DisableTranslation();
                {
                    Surface.Text(RGBA.Black, 20, 5, $"X {Human.X}");
                    Surface.Text(RGBA.Black, 20, 45, $"Y {Human.Y}");
                    Surface.Text(RGBA.Black, 20, 85, $"Z {Human.Z}");
                    Surface.Text(RGBA.Black, 20, 125, $"A {Human.Angle}");
                    Surface.Text(RGBA.Black, 20, 165, $"P {Human.PitchAngle}");
                }
                Surface.EnableTranslation();
            }

            // show a menu if present
            if (Map.IsPaused && Menu != null)
            {
                Surface.DisableTranslation();
                {
                    Menu.Draw(Surface);
                }
                Surface.EnableTranslation();
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

            // must have a human player for input
            if (Human == null) return;

            // move this logic off the ui thread
            Task.Run(() =>
            {
                // handle the user input
                bool result = false;
                Type item = null;
                float xdelta = 0;
                float ydelta = 0;
                float zdelta = 0;

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
                        if (!Config.Is3D) ydelta = 1;
                        else DirectionByAngle(Human.Angle + 180, out xdelta, out zdelta);
                        break;
                    case Constants.Left:
                    case Constants.Left2:
                    case Constants.LeftArrow:
                        if (!Config.Is3D) xdelta = -1;
                        else DirectionByAngle(Human.Angle - 90, out xdelta, out zdelta);
                        break;
                    case Constants.Right:
                    case Constants.Right2:
                    case Constants.RightArrow:
                        if (!Config.Is3D) xdelta = 1;
                        else DirectionByAngle(Human.Angle + 90, out xdelta, out zdelta);
                        break;
                    case Constants.Up:
                    case Constants.Up2:
                    case Constants.UpArrow:
                        if (!Config.Is3D) ydelta = -1;
                        else DirectionByAngle(Human.Angle, out xdelta, out zdelta);
                        break;
                    case Constants.Forward:
                    case Constants.Forward2:
                        zdelta -= 1;
                        break;
                    case Constants.Back:
                    case Constants.Back2:
                        zdelta += 1;
                        break;
                    case Constants.RightMouse:
                        // use the mouse to move in the direction of the angle
                        if (!Config.Is3D) DirectionByAngle(Human.Angle, out xdelta, out ydelta);
                        else DirectionByAngle(Human.Angle, out xdelta, out zdelta);
                        break;
                }

                if (OnBeforeAction != null)
                {
                    List<Element> elements = Map.WithinWindow(Human.X, Human.Y, Human.Z, Constants.ProximityViewWidth, Constants.ProximityViewHeight, depth: Constants.ProximityViewDepth).ToList();
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
                        ZDelta = zdelta,
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

                    case Constants.Place:
                    case Constants.Place2:
                        action = ActionEnum.Place;
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
                        result = Jump(Human);
                        Human.Feedback(action, null, result);
                        break;

                    case ActionEnum.Place:
                        result = Place(Human);
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
                if (xdelta != 0 || ydelta != 0 || zdelta != 0)
                {
                    // ActionEnum.Move;
                    result = Move(Human, xdelta, ydelta, zdelta, Constants.DefaultPace);
                    Human.Feedback(ActionEnum.Move, null, result);
                    if (OnAfterAction != null) OnAfterAction(Human, ActionEnum.Move, result);
                }
            });
        }

        public void Mousewheel(float delta)
        {
            // disabled by the developer
            if (!Config.EnableZoom) return;
            if (Config.Is3D) return;

            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // must have a human player for input
            if (this.Human == null) return;

            // adjust the zoom
            if (delta > 0) Config.CameraZ -= 1f;
            else if (delta < 0) Config.CameraZ += 1f;

            // cap the zoom capability
            if (Config.CameraZ < Constants.Ground) Config.CameraZ = Constants.Ground;
            if (Config.CameraZ > Constants.Sky) Config.CameraZ = Constants.Sky;
        }

        public void Mousemove(float x, float y, float angle)
        {
            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // must have a human player for input
            if (this.Human == null) return;

            if (Human.IsDead) return;

            // use the angle to turn the human player
            if (!Config.Is3D)
            {
                // rotate based on unit circle
                Map.Turn(Human, yaw: angle, pitch: 0f, roll: 0f);
            }

            // if (Config.Is3D)
            else
            {
                //       45
                //        |
                //        0
                // 0---359|0---359
                //       359
                //        |
                //       315

                // horizonal (angle)
                var hwidth = Surface.Width / 2f;
                if (x >= hwidth) x -= hwidth;
                var yaw = (x / hwidth) * 360;

                // vertical (pitchAngle)
                var hheight = Surface.Height / 2f;
                var foundation = 0f;
                if (y >= hheight) y -= hheight;
                else foundation = 315f;
                var pitch = ((y / hheight) * 45) + foundation;

                Map.Turn(Human, yaw, pitch, roll: 0f);
            }

            // todo move events
        }

        public void Mousedown(MouseButton btn, float x, float y)
        {
            // block usage if a menu is being displayed
            if (Map.IsPaused) return;

            // must have a human player for input
            if (Human == null || Human.IsDead) return;

            // before callbacks
            var key = '\0';
            Element elem = null;
            if (OnBeforeMousedown != null)
            {
                // see what is touching the mouse pointer
                TryGetElementAtScreenCoordinate(x, y, out float wx, out float wy, out float wz, out elem);

                // send the notification
                if (OnBeforeMousedown(elem, btn, sx: x, sy: y, wx, wy, wz, ref key)) return;
            }

            // ignore
            var result = false;

            // after callbacks
            if (OnAfterMousedown != null) OnAfterMousedown(elem, key, result);
        }

        public void Mouseup(MouseButton btn, float x, float y)
        {
            // ignore
        }

        public void AddItem(Element item)
        {
            // do not add a dead item
            if (item.IsDead) return;

            Map.AddItem(item);

            if (item is Player player)
            {
                AddPlayer(player);
            }
        }

        public void RemoveItem(Element item)
        {
            if (item is Player player)
            {
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

                // clean up players
                try
                {
                    DetailsLock.EnterWriteLock();
                    Details.Remove(player.Id);
                }
                finally
                {
                    DetailsLock.ExitWriteLock();
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

        public void Teleport(Player player, float x, float y, float z = 0)
        {
            if (player == null) throw new Exception("Cannot teleport an invalid player");

            Map.MoveAbsolute(player, x, y, z);
        }

        public void ApplyForce(Player player, Forces axis, float percentage)
        {
            PlayerDetails detail = null;
            try
            {
                DetailsLock.EnterReadLock();
                if (!Details.TryGetValue(player.Id, out detail)) throw new Exception("Unknown player");
            }
            finally
            {
                DetailsLock.ExitReadLock();
            }

            // add the force
            switch(axis)
            {
                case Forces.X: AddForce(detail, TimeAxis.X, percentage); break;
                case Forces.Y: AddForce(detail, TimeAxis.Y, percentage); break;
                case Forces.Z: AddForce(detail, TimeAxis.Z, percentage); break;
            }
        }

        #region private
        // NOTE: DO NOT Cache any content from the Map locally... it may not be the most current 
        class PlayerDetails
        {
            public int Id;

            // physics
            public object ForceLock;
            public bool[] ForceInMotion;
            public float[] ForceTime;
            public float[] ForceBaseline;
            public float[] Forces;

            public PlayerDetails()
            {
                ForceLock = new object();
                // x,y,z t for force calculations
                // index by TimeAxis'
                ForceTime = new float[3];
                ForceBaseline = new float[3];
                Forces = new float[3];
                ForceInMotion = new bool[3];
            }
        }

        private enum TimeAxis { X = 0, Y = 1, Z = 2};

        private WorldTranslationGraphics Surface;
        private ISounds Sounds;
        private IMap Map;
        private Dictionary<int, PlayerDetails> Details;
        private Menu Menu;
        private WorldConfiguration Config;
        private HashSet<int> UniquePlayers;
        private ReaderWriterLockSlim DetailsLock;
        private Element MouseSelectRegion;
        private bool EnableFastPlayerUpdate;

        private List<EphemerialElement> Ephemerial;
        private ReaderWriterLockSlim EphemerialLock;

        private const string NothingSoundPath = "nothing";
        private const string PickupSoundPath = "pickup";

        private void Initialize(WorldConfiguration config, IMap map, Player[] players)
        {
            Map = map;

            // init
            Config = config;
            Details = new Dictionary<int, PlayerDetails>();
            DetailsLock = new ReaderWriterLockSlim();
            Alive = 0;
            UniquePlayers = new HashSet<int>();
            Ephemerial = new List<EphemerialElement>();
            EphemerialLock = new ReaderWriterLockSlim();
            EnableFastPlayerUpdate = config.EnableFastPlayerUpdate;

            // 3D shaders
            Element3D.SetShader(Element3DShader);

            // start paused
            if (Config.StartMenu != null)
            {
                Menu = Config.StartMenu;
                Map.IsPaused = true;
            }
            else
            {
                Map.IsPaused = false;
            }

            // add all the players
            foreach (var player in players) AddPlayer(player);

            // hook up map callbacks
            Map.OnElementHit += HitByAttack;
            Map.OnElementDied += PlayerDied;
            Map.OnAddEphemerial += EphemerialAdded;

            // setup the background updates
            if (string.IsNullOrWhiteSpace(Config.ServerUrl))
            {
                // ServerUrl == null: (local) (non-remote server) case AND the server of the client-SERVER configuration (Updates should happen)
                // ServerUrl != null: (remote) remote clients of the CLIENT-server configuration (Updates should NOT happen)

                // start the update thread
                Task.Run(UpdateAllPlayersAndBackground);
            }
        }

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

        // AI and other updates for ALL players, and background
        private void UpdateAllPlayersAndBackground()
        {
            // continually update
            while (true)
            {
                // block usage if a menu is being displayed
                while (Map.IsPaused)
                {
                    // spin wait for it to be unpaused
                    System.Threading.Thread.Sleep(1000);
                }

                // update the player
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // get a snapshot of the player ids
                List<int> ids;
                try
                {
                    DetailsLock.EnterReadLock();

                    // todo - is this thread safe?
                    ids = Details.Keys.ToList();
                }
                finally
                {
                    DetailsLock.ExitReadLock();
                }

                // iterate through all the players and update
                Parallel.ForEach(ids, (id) =>
                {
                    // grab the details
                    PlayerDetails detail = null;
                    try
                    {
                        DetailsLock.EnterReadLock();
                        if (!Details.TryGetValue(id, out detail))
                        {
                            // the player must be dead and caused the record to be cleaned up
                            return;
                        }
                    }
                    finally
                    {
                        DetailsLock.ExitReadLock();
                    }

                    Player player = Map.GetPlayer(detail.Id);

                    // todo when can the player be null?
                    if (player != null)
                    {
                        // for all players, call update function (consistent clock to allow players to make updates)
                        player.Update();

                        // if AI, then query for movement
                        if (player is AI ai)
                        {
                            float xdelta = 0;
                            float ydelta = 0;
                            float zdelta = 0;
                            float angle = 0;

                            if (ai.IsDead)
                            {
                                // remove the AI
                                RemoveItem(ai);
                                return;
                            }

                            List<Element> elements = Map.WithinWindow(ai.X, ai.Y, ai.Z, Constants.ProximityViewWidth, Constants.ProximityViewHeight, depth: Constants.ProximityViewDepth).ToList();
                            var angleToCenter = Collision.CalculateAngleFromPoint(ai.X, ai.Y, Config.Width / 2, Config.Height / 2);
                            var inZone = Map.Background.Damage(ai.X, ai.Y) > 0;

                            // get action from AI

                            var action = ai.Action(elements, angleToCenter, inZone, ref xdelta, ref ydelta, ref zdelta, ref angle);

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
                                    ZDelta = zdelta,
                                    Angle = angle
                                });
                            }

                            // turn
                            Map.Turn(ai, yaw: angle, pitch: 0f, roll: 0f);

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
                                case ActionEnum.Place:
                                    result = Place(ai);
                                    ai.Feedback(action, null, result);
                                    break;
                                default: throw new Exception("Unknown ai action : " + action);
                            }

                            // send after telemetry
                            if (OnAfterAction != null && action != ActionEnum.Move) OnAfterAction(ai, action, result);

                            // have the AI move
                            var moved = Move(ai, xdelta, ydelta, zdelta, Constants.DefaultPace);
                            ai.Feedback(ActionEnum.Move, null, moved);
                            if (OnAfterAction != null) OnAfterAction(ai, ActionEnum.Move, result);
                        }

                        // apply forces, if necessary
                        if (Config.ForcesApplied > 0 && player.CanMove)
                        {
                            // apply 'jump' force
                            if ((Config.ForcesApplied & (int)Forces.Y) > 0)
                            {
                                var result = ApplyForce(detail, player, TimeAxis.Y, force: detail.Forces[(int)TimeAxis.Y], opposingForce: Constants.Gravity);
                                // we are in the air if the force was successfully applied OR if we were heading up (negative) and were unsuccessful
                                detail.ForceInMotion[(int)TimeAxis.Y] = (result & ForceState.Success) != 0 || ((result & ForceState.Failed) != 0 && (result & ForceState.Negative) != 0);
                            }

                            // apply a horizontal force, if necessary
                            if ((Config.ForcesApplied & (int)Forces.X) > 0)
                            {
                                if (detail.ForceInMotion[(int)TimeAxis.Y] && detail.Forces[(int)TimeAxis.X] != 0)
                                {
                                    var result = ApplyForce(detail, player, TimeAxis.X, force: detail.Forces[(int)TimeAxis.X], opposingForce: 0f);
                                    detail.ForceInMotion[(int)TimeAxis.X] = (result & ForceState.Success) != 0;
                                }
                            }

                            // apply z force, if necessary
                            if ((Config.ForcesApplied & (int)Forces.Z) > 0)
                            {
                                if (detail.Forces[(int)TimeAxis.Z] != 0)
                                {
                                    // decend
                                    var result = ApplyForce(detail, player, TimeAxis.Z, force: 0f, opposingForce: -0.03f * Constants.Gravity);
                                    if (player.Z <= Constants.IsTouchingDistance ||
                                        ((int)result & (int)ForceState.Failed) != 0)
                                    {
                                        // ensure the player is on the ground
                                        Teleport(player, x: player.X, player.Y, z: Constants.Ground);

                                        // remove the force
                                        RemoveForce(detail, TimeAxis.Z);

                                        // check if the player is touching an object, if so then move
                                        int count = 100;
                                        float xstep = 0.01f;
                                        float xmove = 10f;
                                        if (player.X > Map.Width / 2)
                                        {
                                            // move the other way
                                            xstep *= -1;
                                            xmove *= -1;
                                        }

                                        // check that we are in a safe place to land
                                        do
                                        {
                                            float xdelta = xstep;
                                            float ydelta = 0;
                                            float zdelta = 0;
                                            if (Move(player, xdelta, ydelta, zdelta, Constants.DefaultPace))
                                            {
                                                break;
                                            }

                                            // move over
                                            Teleport(player, player.X + xmove, player.Y, player.Z);
                                        }
                                        while (count-- > 0);

                                        if (count <= 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine("Failed to move after ZForce");
                                            // notify that this player did not make it (dead on impact)
                                            if (OnDeath != null) OnDeath(player);
                                        }
                                    }
                                }
                            } // if z force
                        } // if apply force
                    } // if (player != null)
                });

                // do a background update
                BackgroundUpdate();

                timer.Stop();

                if (timer.ElapsedMilliseconds > Constants.GlobalClock) System.Diagnostics.Debug.WriteLine($"**UpdateAllPlayers Duration {timer.ElapsedMilliseconds} ms");

                // check if we should stall before our next round
                if (!EnableFastPlayerUpdate && timer.ElapsedMilliseconds < Constants.GlobalClock)
                {
                    var delta = (int)Math.Ceiling((Constants.GlobalClock - timer.ElapsedMilliseconds) * 0.9f);
                    System.Threading.Thread.Sleep(delta);
                }
            }
        }

        // background callback
        private void BackgroundUpdate()
        {
            // update the ephemeral items
            var toremove = new List<EphemerialElement>();
            try
            {
                EphemerialLock.EnterReadLock();
                foreach (var b in Ephemerial)
                {
                    // advance
                    if (!b.IsDead && b.Action(out float xdelta, out float ydelta, out float zdelta))
                    {
                        if (Map.WhatWouldPlayerTouch(b, ref xdelta, ref ydelta, ref zdelta, out Element touching, b.BasePace))
                        {
                            if (touching == null)
                            {
                                // would not hit anything, ok to move
                                b.Move(xdelta, ydelta, zdelta);
                                b.Feedback(result: true);
                            }
                            else
                            {
                                // would hit something, notify that an attack occurred
                                if (string.IsNullOrWhiteSpace(Config.ServerUrl))
                                {
                                    // this happens in the local (non-remote server) case AND the server of the client-SERVER configuration

                                    // only notify if not within a server context
                                    HitByAttack(b, touching);
                                }
                                b.Feedback(result: false);
                            }
                        }
                    }

                    // advance and remove when it hits the end
                    if (++b.CurrentDuration >= b.Duration) toremove.Add(b);
                }
            }
            finally
            {
                EphemerialLock.ExitReadLock();
            }

            try
            {
                EphemerialLock.EnterWriteLock();
                // remove any as necessary
                foreach (var b in toremove)
                {
                    Ephemerial.Remove(b);
                }
            }
            finally
            {
                EphemerialLock.ExitWriteLock();
            }

            // apply any necessary damage to the players
            Map.UpdateBackground(applydamage: string.IsNullOrWhiteSpace(Config.ServerUrl));
        }

        // support
        private bool TryGetElementAtScreenCoordinate(float x, float y, out float wx, out float wy, out float wz, out Element elem)
        {
            elem = null;
            wx = wy = wz = 0f;

            // todo 3d

            if (Human == null) return false;

            // convert screen x,y into world x,y
            if (MouseSelectRegion == null) MouseSelectRegion = new Element() { Width = 50, Height = 50 };

            // calculate how far the distance should be scaled (from WorldTranslationGraphics)
            var zoom = Human.Z + Config.CameraZ;
            if (zoom == 0) zoom = 1;

            // scale the sx,sy - the Human player is at the center of the screen, which is relative positioning
            var xdelta = x - (Surface.Width / 2f);
            var ydelta = y - (Surface.Height / 2f);
            wx = Human.X + (xdelta * zoom);
            wy = Human.Y + (ydelta * zoom);

            // identify what element is touching is point (only will consider obstacles)
            xdelta = ydelta = 0f;
            var zdelta = 0f;
            lock (MouseSelectRegion)
            {
                MouseSelectRegion.X = wx;
                MouseSelectRegion.Y = wy;
                MouseSelectRegion.Z = 0;
                Map.WhatWouldPlayerTouch(MouseSelectRegion, ref xdelta, ref ydelta, ref zdelta, out elem);
            }

            return elem != null;
        }

        private void AddPlayer(Player player)
        {
            // add another alive player
            Alive++;

            PlayerDetails detail = null;
            try
            {
                DetailsLock.EnterUpgradeableReadLock();
                // try to get this player
                if (Details.ContainsKey(player.Id)) return;

                try
                {
                    DetailsLock.EnterWriteLock();
                    // add it
                    detail = new PlayerDetails() { Id = (player as Player).Id };
                    Details.Add(player.Id, detail);
                    UniquePlayers.Add(player.Id);
                }
                finally
                {
                    DetailsLock.ExitWriteLock();
                }
            }
            finally
            {
                DetailsLock.ExitUpgradeableReadLock();
            }

            // finish configuration (if this is the first time)
            if ((Config.ForcesApplied & (int)Forces.Z) > 0)
            {
                // apply an initial Z force
                AddForce(detail, TimeAxis.Z, percentage: 1f);
            }
        }

        private void AddForce(PlayerDetails detail, TimeAxis axis, float percentage)
        {
            lock (detail.ForceLock)
            {
                // setup supporting details
                detail.ForceTime[(int)axis] = 0f;
                detail.ForceBaseline[(int)axis] = 0f;
                detail.ForceInMotion[(int)axis] = false;

                // normalize -100%-0%-100%
                percentage = (percentage < -1f) ? -1f : (percentage > 1f ? 1f : percentage);

                // set percentage
                switch (axis)
                {
                    case TimeAxis.X: detail.Forces[(int)TimeAxis.X] = percentage * Math.Abs(Constants.Force); break;
                    case TimeAxis.Y: detail.Forces[(int)TimeAxis.Y] = percentage * Constants.Force; break;
                    case TimeAxis.Z: detail.Forces[(int)TimeAxis.Z] = percentage * Constants.Gravity; break;
                }
            }
        }

        private void RemoveForce(PlayerDetails detail, TimeAxis axis)
        {
            lock (detail.ForceLock)
            {
                // cleanup supporting details
                detail.ForceTime[(int)axis] = 0f;
                detail.ForceBaseline[(int)axis] = 0f;
                detail.Forces[(int)axis] = 0f;
                detail.ForceInMotion[(int)axis] = false;
            }
        }

        private enum ForceState { None = 0, Success = 1, Failed = 2, Negative = 4, Positive = 8 };
        private ForceState ApplyForce(PlayerDetails detail, Player player, TimeAxis axis, float force, float opposingForce)
        {
            lock (detail.ForceLock)
            {
                // advance time
                detail.ForceTime[(int)axis] += (Constants.GlobalClock / 1000f); // ms

                // init
                var t = detail.ForceTime[(int)axis];
                var result = ForceState.None;

                // calculate the distance (d=1/2*g*t^2 + v*t)
                var distance = 0.5f * opposingForce * (t * t) + force * t;

                // compute how far the player should move
                var pace = (distance - detail.ForceBaseline[(int)axis]);

                // determine direction
                result = ForceState.Positive;
                var direction = 1f;
                if (pace < 0f)
                {
                    result = ForceState.Negative;
                    direction = -1f;
                    pace *= -1;
                }

                // apply (
                var retries = 3;
                do
                {
                    // apply movement equivalent to the force
                    if (Move(player,
                        xdelta: (axis == TimeAxis.X) ? direction : 0f,
                        ydelta: (axis == TimeAxis.Y) ? direction : 0f,
                        zdelta: (axis == TimeAxis.Z) ? direction : 0f,
                        pace))
                    {
                        // retain baseline
                        detail.ForceBaseline[(int)axis] += (direction * pace);

                        // degrade the force
                        detail.Forces[(int)axis] *= 0.99f;

                        return result | ForceState.Success;
                    }

                    // we are too close, reduce how far we try
                    pace /= 2f;
                }
                while (retries-- > 0);

                // remove the force
                RemoveForce(detail, axis);

                return result | ForceState.Failed;
            }
        }

        private RGBA Element3DShader(Element3D elem, Point[] points, RGBA color)
        {
            // validate
            if (points == null || elem == null) throw new Exception("Invalid input to shader");

            // todo - shade appropriately
            var avgY = points.Average(p => p.Y);
            var ratio = 1 - ((elem.Y - (elem.Height / 2)) - (avgY * elem.Height)) / elem.Height;

            color.R = (byte)(color.R * ratio);
            color.G = (byte)(color.G * ratio);
            color.B = (byte)(color.B * ratio);

            return color;
        }

        private static void DirectionByAngle(float angle, out float x, out float y)
        {
            // get line based on angle
            Collision.CalculateLineByAngle(x: 0, y: 0, angle: angle, distance: 1f, out float x1, out float y1, out x, out y);

            // normalize
            var sum = (Math.Abs(x) + Math.Abs(y));
            x /= sum;
            y /= sum;
        }

        // human movements
        private bool SwitchPrimary(Player player, out Type type)
        {
            // no indication of what was switched
            type = null;

            if (player.IsDead) return false;
            return Map.SwitchPrimary(player);
        }

        private bool Pickup(Player player, out Type type)
        {
            // indicate what was picked up
            type = null;

            if (player.IsDead) return false;
            type = Map.Pickup(player);
            if (type != null && Human != null && player.Id == Human.Id)
            {
                // play sound
                Play(PickupSoundPath);
            }
            return (type != null);
        }

        private bool Drop(Player player, out Type type)
        {
            // indicate what was dropped
            type = Map.Drop(player);
            return (type != null);
        }

        private bool Reload(Player player, out AttackStateEnum reloaded)
        {
            // return the state
            reloaded = AttackStateEnum.None;

            if (player.IsDead) return false;
            reloaded = Map.Reload(player);

            if (Human != null && player.Id == Human.Id)
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

            if (!Config.Is3D && player.Z > Constants.Ground) return false;

            attack = Map.Attack(player);

            if (Human != null && player.Id == Human.Id)
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

        private bool Move(Player player, float xdelta, float ydelta, float zdelta, float pace)
        {
            if (player.IsDead) return false;

            Element touching;
            if (Map.Move(player, ref xdelta, ref ydelta, ref zdelta, out touching, pace))
            {
                // the move completed
                if (touching != null) throw new Exception("There should not be an object touching");

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

        private bool Jump(Player player)
        {
            if (player.IsDead) return false;
            if ((Config.ForcesApplied & (int)Forces.Y) == 0) return false;

            // check that the player is touching something below them
            var xdelta = 0f;
            var ydelta = Constants.IsTouchingDistance;
            var zdelta = 0f;
            Element touching;
            if (Map.WhatWouldPlayerTouch(player, ref xdelta, ref ydelta, ref zdelta, out touching))
            {
                // successfully checked
                if (touching != null && player.Y < touching.Y)
                {
                    // this player is standing on something
                    ApplyForce(player, Forces.Y, player.MaxYForcePercentage);
                    return true;
                }
            }
            return false;
        }

        private bool Place(Player player)
        {
            if (player.IsDead) return false;
            if (player.Primary == null) return false;

            return Map.Place(player);
        }

        // callbacks
        private void HitByAttack(Element primaryElement, Element element)
        {
            // play sound if the human is hit
            if (element is Player && Human != null && element.Id == Human.Id)
            {
                // get the human player so we can play the sound
                Play(Human.HurtSoundPath);
            }

            // notify the outside world that we hit something
            if (OnAttack != null) OnAttack(primaryElement, element);
        }

        private void PlayerDied(Element element)
        {
            // check for winner/death (element may be any element that can take damage)
            if (element is Player)
            {
                // remove this player
                RemoveItem(element);

                // track how many players are alive
                Alive = Map.GetStats().PlayersAlive;

                // callback
                if (OnDeath != null) OnDeath(element);
            }
        }

        private void EphemerialAdded(EphemerialElement elem)
        {
            try
            {
                EphemerialLock.EnterWriteLock();
                Ephemerial.Add(elem);
            }
            finally
            {
                EphemerialLock.ExitWriteLock();
            }
        }
        #endregion
    }
}

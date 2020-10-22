using engine.Common.Entities;
using engine.Common.Entities3D;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Networking
{
    // SignalR protocol
    //  Send_<MethodName> and Receive_<MethodName>
    //   send to server
    //   receive from server
    // 
    //  Example: Send_IsPaused, Received_IsPaused
    //
    // Certain portions of code must not hit the network, so as many of the properties of the Map should operate on local copies

    class RemoteMap : IMap
    {
        public RemoteMap(bool isHost, WorldConfiguration config, Player[] players, Element[] objects, Background background)
        {
            // create the underlying Map
            if (config.Is3D) Map = new Map3D(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);
            else Map = new Map(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);

            // create connection
            Connection = new HubConnectionBuilder()
                .WithUrl($"{config.ServerUrl}/remotemap")
                .WithAutomaticReconnect()
                .Build();

            // setup (receive methods)
            Connection.On<bool>("Receive_IsPaused", (isPaused) => Receive_IsPaused(isPaused));
            Connection.On<Element, bool>("Receive_AddItem", (elem, outcome) => Receive_AddItem(elem, outcome));
            Connection.On<Element, bool>("Receive_RemoveItem", (elem, outcome) => Receive_RemoveItem(elem, outcome));
            Connection.On<Player, AttackStateEnum>("Receive_Attack", (player, outcome) => Receive_Attack(player, outcome));
            Connection.On<Player, Type>("Receive_Drop", (player, outcome) => Receive_Drop(player, outcome));
            Connection.On<Player, Type>("Receive_Pickup", (player, outcome) => Receive_Pickup(player, outcome));
            Connection.On<Player, float, float, float, float, Tuple<Element, float, float, float, bool>>("Receive_Move", (player, xdelta, ydelta, zdelta, pace, outcome) => Receive_Move(player, xdelta, ydelta, zdelta, pace, outcome));

            /*
            Connection.On<List<EphemerialElement>>("Receive_GetEphemerials", (lst) => Receive_GetEphemerials(lst));
            Connection.On<Element, Element>("OnElementHitPull", (elem1, elem2) => { if (OnElementHit != null) OnElementHit(elem1, elem2); });
            Connection.On<Element>("OnElementDied", (elem) => { if (OnElementDied != null) OnElementDied(elem); });
            */

            // start
            Task.Run(() => { Start(isHost, config, players, objects, background); });
        }

        public async void Start(bool isHost, WorldConfiguration config, Player[] players, Element[] objects, Background background)
        {
            // start
            await  Connection.StartAsync();

            // temporary
            var result = await Connection.InvokeAsync<bool>("Send_IsAlive");
            System.Diagnostics.Debug.WriteLine($"Result = {result}");

            // initialize remote copy of this map
            if (isHost)
            {
                // serialize objects to json (needed, due to inheritance)
                var playersJson = ToJson<Player>(players);
                var objectsJson = ToJson<Element>(objects);
                var backgroundJson = ToJson<Background>(background);

                // send the initial information
                await Connection.InvokeAsync("Send_Initialize", config, playersJson, objectsJson, backgroundJson);
            }
        }

        //
        // Local Caches that do not change
        //
        public int Width { get { return Map.Width; } }
        public int Height { get { return Map.Height; } }
        public int Depth { get { return Map.Depth; } }
        public Background Background { get { return Map.Background; } }

        //
        // Local are updated async
        //

        public List<EphemerialElement> GetEphemerials()
        {
            // todo
            // updated Via Receive_GetEphemerials
            return new List<EphemerialElement>();
        }

        private List<EphemerialElement> _Ephemerials;
        private void Receive_GetEphemerials(List<EphemerialElement> ephemerials)
        {
            _Ephemerials = ephemerials;
        }

        public Player GetPlayer(int id)
        {
            // updated via Receive_AddItem/Receive_RemoveItem
            return Map.GetPlayer(id);
        }

        public MapStats GetStats()
        {
            // updated via various
            return Map.GetStats();
        }

        public bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return Map.IsTouching(elem1, elem2, x1delta, y1delta, z1delta);
        }

        public List<Player> WhatPlayersAreTouching(Element elem)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return Map.WhatPlayersAreTouching(elem);
        }

        public bool WhatWouldPlayerTouch(Element elem, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace = 0)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return Map.WhatWouldPlayerTouch(elem, ref xdelta, ref ydelta, ref zdelta, out touching, pace);
        }

        public IEnumerable<Element> WithinWindow(float x, float y, float z, float width, float height, float depth)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return Map.WithinWindow(x, y, z, width, height, depth);
        }

        #region ispaused
        public bool IsPaused
        {
            get
            {
                return _IsPaused;
            }
            set
            {
                // todo? Not sure this makes sense
                // send to server
                Connection.InvokeAsync<bool>("Send_IsPaused", value);
                // can set right away, as there is no conditionality on setting this
                _IsPaused = value;
            }
        }

        private bool _IsPaused;
        private void Receive_IsPaused(bool isPaused)
        {
            _IsPaused = isPaused;
        }
        #endregion

        // events
        // TODO!
        public event Action<Element, Element> OnElementHit;
        public event Action<Element> OnElementDied;

        #region additem
        // methods
        public bool AddItem(Element item)
        {
            return Connection.InvokeAsync<bool>("Send_AddItem", item).Result;
        }

        private void Receive_AddItem(Element item, bool outcome)
        {
            // add the item locally
            var result = Map.AddItem(item);
            if (result != outcome) throw new Exception("AddItem returned the wrong result : " + item.Id + " (" + result + " != " + outcome + ")");
        }
        #endregion

        #region removeitem
        public bool RemoveItem(Element item)
        {
            return Connection.InvokeAsync<bool>("Send_RemoveItem").Result;
        }

        private void Receive_RemoveItem(Element item, bool outcome)
        {
            // remove the item locally
            var result = Map.RemoveItem(item);
            if (result != outcome) throw new Exception("RemoveItem returned the wrong result : " + item.Id + " (" + result + " != " + outcome + ")");
        }
        #endregion

        #region attack
        public AttackStateEnum Attack(Player player)
        {
            return Connection.InvokeAsync<AttackStateEnum>("Send_Attack", player).Result;
        }

        private void Receive_Attack(Player player, AttackStateEnum outcome)
        {
            // attack locally
            var result = Map.Attack(player);
            if (result != outcome) throw new Exception("Attack returned the wrong result : " + player.Id + " (" + result + " != " + outcome + ")");
        }
        #endregion

        #region drop
        public Type Drop(Player player)
        {
            return Connection.InvokeAsync<Type>("Send_Drop", player).Result;
        }

        private void Receive_Drop(Player player, Type outcome)
        {
            // drop locally
            var result = Map.Drop(player);
            if (result != outcome) throw new Exception("Drop returned the wrong result : " + player.Id + " (" + result + " != " + outcome + ")");
        }
        #endregion

        #region pickup
        public Type Pickup(Player player)
        {
            return Connection.InvokeAsync<Type>("PickupCall").Result;
        }

        private void Receive_Pickup(Player player, Type outcome)
        {
            // pickup locally
            var result = Map.Pickup(player);
            if (result != outcome) throw new Exception("Pickup returned the wrong result : " + player.Id + " (" + result + " != " + outcome + ")");
        }
        #endregion

        #region move
        public bool Move(Player player, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace)
        {
            var result = Connection.InvokeAsync<Tuple<Element,float,float,float,bool>>("Send_Move", player, xdelta, ydelta, zdelta, pace).Result;
            xdelta = result.Item2;
            ydelta = result.Item3;
            zdelta = result.Item4;
            touching = result.Item1;
            return result.Item5;
        }

        private void Receive_Move(Player player, float xdelta, float ydelta, float zdelta, float pace, Tuple<Element, float, float, float, bool> outcome)
        {
            // move locally
            var result = Map.Move(player, ref xdelta, ref ydelta, ref zdelta, out Element touching, pace);
            if (result != outcome.Item5) throw new Exception("Move returned the wrong result : " + player.Id + " (" + result + " != " + outcome + ")");
            if (Math.Abs(xdelta - outcome.Item2) > 0.0001f
                || Math.Abs(ydelta - outcome.Item3) > 0.0001f
                || Math.Abs(zdelta - outcome.Item4) > 0.0001f) throw new Exception("Move had invalid deltas");
            if ((outcome.Item1 == null && touching != null)
                || (outcome.Item1 != null && touching == null)
                || (outcome.Item1.Id != touching.Id)) throw new Exception("Received the wrong touching element");
        }
        #endregion

        #region private
        private IMap Map;
        private HubConnection Connection;
        #endregion

        #region utilities
        public static string ToJson<T>(T[] items)
        {
            var allJson = new StringBuilder();
            foreach (var item in items) allJson.AppendLine(ToJson<T>(item));
            return allJson.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToJson<T>(T item)
        {
            var type = item.GetType();
            var assem = type.Assembly;
            var json = System.Text.Json.JsonSerializer.Serialize(item, type);
            return $"{type.ToString()}\t{assem.FullName}\t{json}";
        }

        public static List<T> FromJsons<T>(string allJson)
        {
            var items = new List<T>();
            foreach(var line in allJson.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                items.Add(FromJson<T>(line));
            }

            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FromJson<T>(string text)
        {
            var parts = text.Split(new char[] { '\t' }, 3);
            if (parts.Length != 3) throw new Exception("Invalid line : " + text);
            var typename = parts[0];
            var assemname = parts[1];
            var json = parts[2];
            var assem = System.AppDomain.CurrentDomain.Load(assemname);
            var type = assem.GetType(typename);
            return (T)System.Text.Json.JsonSerializer.Deserialize(json, type);
        }

        #endregion
    }
}

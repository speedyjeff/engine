using engine.Common.Entities;
using engine.Common.Entities3D;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// todo Receive_* messages may be received out of order
// todo GetPlayer can return null

namespace engine.Common.Networking
{
    // todo update
    // SignalR protocol
    //  Send_<MethodName> and Receive_<MethodName>
    //   send to server
    //   receive from server
    // 
    //  Example: Send_IsPaused, Receive_IsPaused
    //
    // Certain portions of code must not hit the network, so as many of the properties of the Map should operate on local copies

    public class RemoteMap : IMap
    {
        public RemoteMap(WorldConfiguration config, int depth, Player[] players, Element[] objects, Background background, string group = "")
        {
            // create local map
            if (config.Is3D) ActualMap = new Map3D(config.Width, config.Height, depth, players, objects, background);
            else ActualMap = new Map(config.Width, config.Height, depth, players, objects, background);

            // connect the events
            ActualMap.OnElementDied += (elem) => { if (OnElementDied != null) OnElementDied(elem); };
            ActualMap.OnElementHit += (elem1, elem2) => { if (OnElementHit != null) OnElementHit(elem1, elem2); };

            // todo OnAddEphemerial

            // initialize remote connection
            Initialize(config, depth, players, objects, background, group);
        }

 #region initialize
        private async void Initialize(WorldConfiguration config, int depth, Player[] players, Element[] objects, Background background, string group)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Initalize");
            if (ActualMap == null) throw new Exception("Must initialize a map before calling connect");
            if (string.IsNullOrWhiteSpace(config.ServerUrl)) throw new Exception("Must provide a non-empty server url");

            // remove trailing slashes
            var serverUrl = config.ServerUrl;
            while (serverUrl.EndsWith("/")) serverUrl = serverUrl.Substring(0, serverUrl.Length - 1);

            // create connection
            Connection = new HubConnectionBuilder()
                .WithUrl($"{serverUrl}/remotemap")
                .Build();

            // setup (receive methods)
            Connection.On<bool>("Receive_IsPaused", (isPaused) => Receive_IsPaused(isPaused));
            Connection.On<int, float, float, float, float, MoveResult>("Receive_Move", (playerId, xdelta, ydelta, zdelta, pace, outcome) => Receive_Move(playerId, xdelta, ydelta, zdelta, pace, outcome));
            Connection.On<int, float, float, float, bool>("Receive_MoveAbsolute", (playerId, x, y, z, outcome) => Receive_MoveAbsolute(playerId, x, y, z, outcome));
            Connection.On<string, bool>("Receive_AddItem", (json, outcome) => Receive_AddItem(json, outcome));
            Connection.On<string, bool>("Receive_RemoveItem", (json, outcome) => Receive_RemoveItem(json, outcome));
            Connection.On<int, AttackStateEnum>("Receive_Attack", (playerId, outcome) => Receive_Attack(playerId, outcome));
            Connection.On<int, string>("Receive_Drop", (playerId, json) => Receive_Drop(playerId, json));
            Connection.On<int, string>("Receive_Pickup", (playerId, json) => Receive_Pickup(playerId, json));
            Connection.On<int, float, bool>("Receive_ReduceHealth", (playerId, damage, outcome) => Receive_ReduceHealth(playerId, damage, outcome));
            Connection.On<int, AttackStateEnum>("Receive_Reload", (playerId, outcome) => Receive_Reload(playerId, outcome));
            Connection.On<int, bool>("Receive_SwitchPrimary", (playerId, outcome) => Receive_SwitchPrimary(playerId, outcome));
            Connection.On<int, float, float, float, bool>("Receive_Turn", (playerId, yaw, pitch, roll, outcome) => Receive_Turn(playerId, yaw, pitch, roll, outcome));

            // start
            await Connection.StartAsync();

            // initialize remote copy of this map
            if (string.IsNullOrWhiteSpace(group))
            {
                // serialize objects to json (needed, due to inheritance)
                var playersJson = ToJson<Player>(players);
                var objectsJson = ToJson<Element>(objects);
                var backgroundJson = ToJson<Background>(background);

                // todo config Menus need to be serialized

                // send the initial information and get a group name
                group = await Connection.InvokeAsync<string>("Send_Initialize", config, depth, playersJson, objectsJson, backgroundJson);
            }

            // join the group
            await Connection.SendAsync("Send_Join", group);

            // set group
            Group = group;
        }
 #endregion

        //
        // Local Caches that do not change
        //
        public int Width { get { return ActualMap.Width; } }
        public int Height { get { return ActualMap.Height; } }
        public int Depth { get { return ActualMap.Depth; } }
        public Background Background { get { return ActualMap.Background; } }

        // events
        public event Action<Element, Element> OnElementHit;
        public event Action<Element> OnElementDied;
        public event Action<EphemerialElement> OnAddEphemerial;

        //
        // Local are updated async
        //

        public Player GetPlayer(int id)
        {
            // updated via Receive_AddItem/Receive_RemoveItem
            return ActualMap.GetPlayer(id);
        }

        public MapStats GetStats()
        {
            // updated via ActualMap
            return ActualMap.GetStats();
        }

        public bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return ActualMap.IsTouching(elem1, elem2, x1delta, y1delta, z1delta);
        }

        public List<Player> WhatPlayersAreTouching(Element elem)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return ActualMap.WhatPlayersAreTouching(elem);
        }

        public bool WhatWouldPlayerTouch(Element elem, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace = 0)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return ActualMap.WhatWouldPlayerTouch(elem, ref xdelta, ref ydelta, ref zdelta, out touching, pace);
        }

        public IEnumerable<Element> WithinWindow(float x, float y, float z, float width, float height, float depth)
        {
            // updated via Receive_Move, Receive_AddItem, Receive_RemoveItem, Receive_Drop, Receive_Pickup
            return ActualMap.WithinWindow(x, y, z, width, height, depth);
        }

#region ispaused
        public bool IsPaused
        {
            get
            {
                return ActualMap.IsPaused;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(Group)) return;

                // change local pause
                ActualMap.IsPaused = value;
                // send to server
                Connection.SendAsync("Send_IsPaused", Group, value);
            }
        }

        private void Receive_IsPaused(bool isPaused)
        {
            ActualMap.IsPaused = isPaused;
        }
#endregion

#region reducehealth
        public bool ReduceHealth(Player player, float damage)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] ReduceHealth {player.Id}");

            // consult the server
            var result = Connection.InvokeAsync<bool>("Send_ReduceHealth", Group, player.Id, damage).Result;

            // apply locally
            if (result)
            {
                var localresult = ActualMap.ReduceHealth(player, damage);
                if (!localresult) throw new Exception("Failed to ReduceHealth");
            }

            return result;
        }

        private void Receive_ReduceHealth(int playerId, float damage, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_ReduceHealth");

            // add the item locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.ReduceHealth(player, damage);
            if (result != outcome) throw new Exception("RedcueHealth returned the wrong result : " + playerId + " (" + result + " != " + outcome + ")");
        }
#endregion

#region reload
        public AttackStateEnum Reload(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return AttackStateEnum.None;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Reload {player.Id}");

            // consult the server
            var result = Connection.InvokeAsync<AttackStateEnum>("Send_Reload", Group, player.Id).Result;

            // apply locally
            if (result != AttackStateEnum.None)
            {
                var localresult = ActualMap.Reload(player);
                if (result != localresult) throw new Exception("Failed to Reload");
            }

            return result;
        }

        private void Receive_Reload(int playerId, AttackStateEnum outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Reload");

            // add the item locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.Reload(player);
            if (result != outcome) throw new Exception("Reload returned the wrong result : " + playerId + " (" + result + " != " + outcome + ")");
        }
#endregion

#region switchprimary
        public bool SwitchPrimary(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] SwitchPrimary {player.Id}");

            // consult the server
            var result = Connection.InvokeAsync<bool>("Send_SwitchPrimary", Group, player.Id).Result;

            // apply locally
            if (result)
            {
                var localresult = ActualMap.SwitchPrimary(player);
                if (!localresult) throw new Exception("Failed to SwitchPrimary");
            }

            return result;
        }

        private void Receive_SwitchPrimary(int playerId, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_SwitchPrimary");

            // add the item locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.SwitchPrimary(player);
            if (!outcome) throw new Exception("SwitchPrimary returned the wrong result : " + playerId + " (" + result + " != " + outcome + ")");
        }
#endregion

#region turn
        public bool Turn(Player player, float yaw, float pitch, float roll)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Turn {player.Id}");

            // consult the server
            var result = Connection.InvokeAsync<bool>("Send_Turn", Group, player.Id, yaw, pitch, roll).Result;

            // apply locally
            if (result)
            {
                var localresult = ActualMap.Turn(player, yaw, pitch, roll);
                if (!localresult) throw new Exception("Failed to Turn");
            }

            return result;
        }

        private void Receive_Turn(int playerId, float yaw, float pitch, float roll, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Turn {playerId}");

            // add the item locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.Turn(player, yaw, pitch, roll);
            if (result != outcome) throw new Exception("RedcueHealth returned the wrong result : " + playerId + " (" + result + " != " + outcome + ")");
        }
#endregion

#region additem
        // methods
        public bool AddItem(Element item)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] AddItem {item.Id}");

            // serialize the item
            var json = ToJson<Element>(item);

            // consult the server
            var result = Connection.InvokeAsync<bool>("Send_AddItem", Group, json).Result;

            // apply locally
            if (result)
            {
                var localresult = ActualMap.AddItem(item);
                if (!localresult) throw new Exception("Failed to AddItem");
            }

            return result;
        }

        private void Receive_AddItem(string json, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_AddItem");

            // deserialize the item
            var element = FromJson<Element>(json);

            // add the item locally
            var result = ActualMap.AddItem(element);
            if (result != outcome) throw new Exception("AddItem returned the wrong result : " + element.Id + " (" + result + " != " + outcome + ")");
        }
#endregion

#region removeitem
        public bool RemoveItem(Element item)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] RemoveItem {item.Id}");

            // serialize item
            var json = ToJson<Element>(item);

            // send to server
            var result = Connection.InvokeAsync<bool>("Send_RemoveItem", Group, json).Result;

            // apply locally
            if (result)
            {
                var localresult = ActualMap.RemoveItem(item);
                if (!localresult) throw new Exception("Failed to RemoveItem");
            }

            return result;
        }

        private void Receive_RemoveItem(string elementJson, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_RemoveItem");

            // deserialize the element
            var element = FromJson<Element>(elementJson);

            // remove the item locally
            var result = ActualMap.RemoveItem(element);
            if (result != outcome) throw new Exception("RemoveItem returned the wrong result : " + element.Id + " (" + result + " != " + outcome + ")");
        }
#endregion

#region attack
        public AttackStateEnum Attack(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return AttackStateEnum.None;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Attack {player.Id}");

            // send to server
            var result = Connection.InvokeAsync<AttackStateEnum>("Send_Attack", Group, player.Id).Result;

            if (result != AttackStateEnum.None)
            {
                var localresult = ActualMap.Attack(player);
                if (result != localresult) throw new Exception($"Failed to get same result from Attack: {result} != {localresult}");
            }

            return result;
        }

        private void Receive_Attack(int playerId, AttackStateEnum outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Attack {playerId}");

            // attack locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.Attack(player);
            if (result != outcome) throw new Exception("Attack returned the wrong result : " + player.Id + " (" + result + " != " + outcome + ")");
        }
#endregion

#region drop
        public Type Drop(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return null;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Drop {player.Id}");

            // send to server
            var result = Connection.InvokeAsync<string>("Send_Drop", Group, player.Id).Result;
            Type type = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                // deserialize the type
                type = FromJson<Type>(result);

                var localType = ActualMap.Drop(player);
                if (localType == null || !type.Equals(localType)) throw new Exception($"Drop returned the wrong type: {localType} != {type}");
            }

            return type;
        }

        private void Receive_Drop(int playerId, string typeJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Drop {playerId}");

            // deserialize the type
            var type = FromJson<Type>(typeJson);

            // drop locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.Drop(player);
            if (result == null || !type.Equals(result)) throw new Exception("Drop returned the wrong result : " + player.Id + " (" + result + " != " + type + ")");
        }
        #endregion

#region place
        public bool Place(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Place {player.Id}");

            // send to server
            var result = Connection.InvokeAsync<string>("Send_Place", Group, player.Id).Result;
            bool remoteSuccess = false;

            if (!string.IsNullOrWhiteSpace(result))
            {
                // deserialize the type
                remoteSuccess = FromJson<bool>(result);

                // todo Place uses AddItem - an issue?

                var success = ActualMap.Place(player);
                if (success != remoteSuccess) throw new Exception($"Place failed: {remoteSuccess} != {success}");
            }

            return remoteSuccess;
        }

        private void Receive_Place(int playerId, string successJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Place {playerId}");

            // deserialize the success
            var remoteSuccess = FromJson<bool>(successJson);

            // drop locally
            var player = ActualMap.GetPlayer(playerId);
            var success = ActualMap.Place(player);
            if (success != remoteSuccess) throw new Exception($"Place failed: {remoteSuccess} != {success}");
        }
#endregion

#region pickup
        public Type Pickup(Player player)
        {
            if (string.IsNullOrWhiteSpace(Group)) return null;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Pickup {player.Id}");

            // send to the server
            var result = Connection.InvokeAsync<string>("Send_Pickup", Group, player.Id).Result;
            Type type = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                // deserialize the type
                type = FromJson<Type>(result);

                var localresult = ActualMap.Pickup(player);
                if (localresult == null || !type.Equals(localresult)) throw new Exception("Pickup returned the wrong type: {type} != {localresult}");
            }

            return type;
        }

        private void Receive_Pickup(int playerId, string typeJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Pickup {playerId}");

            // hydrate the type
            var type = FromJson<Type>(typeJson);

            // pickup locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.Pickup(player);
            if (result == null || !type.Equals(result)) throw new Exception("Pickup returned the wrong result : " + player.Id + " (" + result + " != " + type + ")");
        }
#endregion

#region move
        public bool Move(Player player, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace)
        {
            touching = null;
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Move {player.Id}");

            // send to server
            var result = Connection.InvokeAsync<MoveResult>("Send_Move", Group, player.Id, xdelta, ydelta, zdelta, pace).Result;

            if (result.Outcome)
            {
                var localresult = ActualMap.Move(player, ref xdelta, ref ydelta, ref zdelta, out touching, pace);
                if (!localresult) throw new Exception("Move did not return the same result");
                if ((result.ElementId <= -1 && touching != null)
                    || (result.ElementId >= 0 && touching == null)
                    || (result.ElementId >= 0 && touching != null && result.ElementId != touching.Id)) throw new Exception("Received the wrong touching element");
            }

            return result.Outcome;
        }

        private void Receive_Move(int playerId, float xdelta, float ydelta, float zdelta, float pace, MoveResult result)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_Move {playerId}");

            // move locally
            var player = ActualMap.GetPlayer(playerId);
            var outcome = ActualMap.Move(player, ref xdelta, ref ydelta, ref zdelta, out Element touching, pace);
            
            // validate that the results match
            if (result.Outcome != outcome) throw new Exception("Move returned the wrong result : " + player.Id + " (" + result.Outcome + " != " + outcome + ")");
            if ((result.ElementId <= -1 && touching != null)
                || (result.ElementId >= 0 && touching == null)
                || (result.ElementId >= 0 && touching != null && result.ElementId != touching.Id)) throw new Exception("Received the wrong touching element");
        }

        public bool MoveAbsolute(Player player, float x, float y, float z)
        {
            if (string.IsNullOrWhiteSpace(Group)) return false;
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] MoveAbsolute {player.Id}");

            // send to server
            var result = Connection.InvokeAsync<bool>("Send_MoveAbsolute", Group, player.Id, x, y, z).Result;

            if (result)
            {
                var localresult = ActualMap.MoveAbsolute(player, x, y, z);
                if (!localresult) throw new Exception("MoveAbsolute failed");
            }

            return result;
        }

        private void Receive_MoveAbsolute(int playerId, float x, float y, float z, bool outcome)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[In] Receive_MoveAbsolute {playerId}");

            // move locally
            var player = ActualMap.GetPlayer(playerId);
            var result = ActualMap.MoveAbsolute(player, x, y, z);
            if (result != outcome) throw new Exception($"Failed to get matching results : {playerId} ({result} != {outcome})");
        }
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
            if (typeof(T) == typeof(System.Type))
            {
                // special case for a type
                var type = Type.GetType(item.ToString());
                var assem = type.Assembly;
                return $"{type.ToString()}\t{assem.FullName}\t";
            }
            else
            {
                // an object not a type
                var type = item.GetType();
                var assem = type.Assembly;
                var json = System.Text.Json.JsonSerializer.Serialize(item, type);
                return $"{type.ToString()}\t{assem.FullName}\t{json}";
            }
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
            if (typeof(T) == typeof(System.Type)) return (T)(object)type;
            return (T)System.Text.Json.JsonSerializer.Deserialize(json, type);
        }

        public bool UpdateBackground(bool applydamage)
        {
            throw new NotImplementedException();
        }

        #endregion

#region private
        private HubConnection Connection;
        private string Group;
        private IMap ActualMap;
        private const bool DEBUG_TRACING = true;
#endregion
    }
}

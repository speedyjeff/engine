using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using engine.Common;
using engine.Common.Entities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Runtime.CompilerServices;
using engine.Common.Entities3D;

namespace engine.Server.Hubs
{
    public class RemoteMapHub : Hub
    {
        public bool Send_IsAlive()
        {
            return true;
        }

        public void Send_Initialize(WorldConfiguration config, string playersJson, string objectsJson, string backgroundJson)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Initialization {config.Width}x{config.Height} {playersJson.Length + objectsJson.Length}");

            var players = FromJsons<Player>(playersJson).ToArray();
            var objects = FromJsons<Element>(objectsJson).ToArray();
            var background = FromJson<Background>(backgroundJson);

            // create the map
            if (config.Is3D) Map = new Map3D(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);
            else Map = new Map(config.Width, config.Height, (int)Constants.ProximityViewDepth, players, objects, background);

            // create the broadcast map
            var map = new BroadCastMap(Map, this);

            // create the world (passing in the broadcast map)
            config.ServerUrl = "";
            World = new World(config, Map, players);

            /*
            // hook up events
            Map.OnElementDied += async (elem) =>
            {
                Logger.LogError("OnElementDiedPull");
                await Clients.All.SendAsync("OnElementDiedPull", elem);
            };

            Map.OnElementHit += async (elem1, elem2) =>
            {
                Logger.LogError("OnElementHitPull");
                await Clients.All.SendAsync("OnElementHitPull", elem1, elem2);
            };
            */
        }

        //
        // Send/Receive
        //

        public async Task<bool> Send_Paused(bool isPaused)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Paused {isPaused}");

            // broadcast this AddItem to all other clients
            await Clients.Others.SendAsync("Receive_IsPaused", isPaused);

            return isPaused;
        }

        public async Task<bool> Send_AddItem(Element item)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_AddItem {item.Id}");

            var result = Map.AddItem(item);

            // broadcast this AddItem to all other clients
            await Clients.Others.SendAsync("Receive_AddItem", item, result);

            return result;
        }

        public async Task<bool> Send_RemoveItem(Element item)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_RemoveItem {item.Id}");

            var result = Map.RemoveItem(item);

            // broadcast this RemoveItem to all other clients
            await Clients.Others.SendAsync("Receive_RemoveItem", item, result);

            return result;
        }

        public async Task<AttackStateEnum> Send_Attack(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Attack {player.Id}");

            var result = Map.Attack(player);

            // broadcast this RemoveItem to all other clients
            await Clients.Others.SendAsync("Receive_Attack", player, result);

            return result;
        }

        public async Task<Type> Send_Drop(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Drop {player.Id}");

            var result = Map.Drop(player);

            // broadcast this Drop to all other clients
            await Clients.Others.SendAsync("Receive_Drop", player, result);

            return result;
        }

        public async Task<Type> Send_Pickup(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Pickup {player.Id}");

            var result = Map.Pickup(player);

            // broadcast this Pickup to all other clients
            await Clients.Others.SendAsync("Receive_Pickup", player, result);

            return result;
        }

        public async Task<Tuple<Element, float, float, float, bool>> Send_Move(Player player, float xdelta, float ydelta, float zdelta, float pace)
        {
            System.Diagnostics.Debug.WriteLine($"[in] Send_Move {player.Id}");

            var orgXdelta = xdelta;
            var orgYdelta = ydelta;
            var orgZdelta = zdelta;
            var result = Map.Move(player, ref xdelta, ref ydelta, ref zdelta, out Element touching, pace);

            var outcome = new Tuple<Element,float,float,float,bool>(touching, xdelta, ydelta, zdelta, result);

            // broadcast this Pickup to all other clients
            await Clients.Others.SendAsync("Receive_Move", player, orgXdelta, orgYdelta, orgZdelta, pace, outcome);

            return outcome;
        }

        #region private
        private IMap Map;
        private IUserInteraction World;

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
            foreach (var line in allJson.Split('\n'))
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

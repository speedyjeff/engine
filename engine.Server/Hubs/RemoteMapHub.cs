using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using engine.Common;
using engine.Common.Entities;
using engine.Common.Networking;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Runtime.CompilerServices;
using engine.Common.Entities3D;
using System.Threading;

// Overview
//
// ---------------------------   ------------------------------------
// | Client                  |  | Server                            |
// |                         |  |                                   |
// | World                   |  |        ------------------> World  |
// |  |                      |  |        |                       |  |
// |  --> RemoteMap <--------------> RemoteHub <---> RemoteMap <--  |
// |       |                 |  |     |               |             |
// |       --> AcutalMap     |  |     |               --> ActualMap |
// |                         |  |     |                             |
// |                         |  |     --> SourceOfTruthMap          |
// ---------------------------  -------------------------------------
//
// Client:
//  - World - handles all user input, UI, and timer based updates for UI
//  - RemoteMap - bi-directional connection with server
//  - ActualMap - the storage for the client World
//
// Server
//  - RemoteHub - server listener and sender
//  - SourceOfTruthMap - this is the 'source of truth' of data for all games
//  - World - handles all AI/Player/Background updates
//  - RemoteMap - bi-directional connection with the local server
//  - ActualMap - the storage for the server World

namespace engine.Server.Hubs
{
    public class RemoteMapHub : Hub
    {
        public bool Send_IsAlive()
        {
            return true;
        }

        public string Send_Initialize(WorldConfiguration config, int depth, string playersJson, string objectsJson, string backgroundJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Initialization {config.Width}x{config.Height} {playersJson.Length + objectsJson.Length}");

            // generate a group id
            var group = GenerateUniqueGroup();

            // deserialize
            var players = RemoteMap.FromJsons<Player>(playersJson).ToArray();
            var objects = RemoteMap.FromJsons<Element>(objectsJson).ToArray();
            var background = RemoteMap.FromJson<Background>(backgroundJson);

            // this is a loopback map to this server
            // todo use the loop back for config.ServerUrl
            var remoteMap = new RemoteMap(config, depth, players, objects, background, applyBackgroundDamage: true, group);

            // create the world (passing in the remote map)
            config.ServerUrl = "";
            var world = new World(config, remoteMap, players);

            // create the source of truth map
            // make sure to clone the details (so that there is no object sharing with the remotemap above)
            players = RemoteMap.FromJsons<Player>(playersJson).ToArray();
            objects = RemoteMap.FromJsons<Element>(objectsJson).ToArray();
            background = RemoteMap.FromJson<Background>(backgroundJson);
            IMap sourceOfTruthMap = null;
            if (config.Is3D) sourceOfTruthMap = new Map3D(config.Width, config.Height, depth, players, objects, background, applyBackgroundDamage: false);
            else sourceOfTruthMap = new Map(config.Width, config.Height, depth, players, objects, background, applyBackgroundDamage: false);

            // store for access by the groups
            AddConnection(group, new ConnectionDetails()
                {
                    Group = group,
                    SourceOfTruthMap = sourceOfTruthMap,
                    World = world
                });

            return group;
        }

        public async Task<bool> Send_Join(string group)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Join {group}");

            // add this user to this group
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            return true;
        }

        //
        // Send/Receive
        //

        public async Task<bool> Send_IsPaused(string group, bool isPaused)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Paused {isPaused}");

            var map = GetSourceOfTruthMap(group);
            map.IsPaused = isPaused;

            // broadcast this AddItem to all other clients
            await Clients.Group(group).SendAsync("Receive_IsPaused", isPaused);

            return true;
        }

        public async Task<bool> Send_ReduceHealth(string group, int playerId, float damage)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_ReduceHealth {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.ReduceHealth(player, damage);

            // broadcast this AddItem to all other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_ReduceHealth", playerId, damage, result);

            return result;
        }

        public async Task<AttackStateEnum> Send_Reload(string group, int playerId)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Reload {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.Reload(player);

            // broadcast this AddItem to all other clients
            if (result != AttackStateEnum.None) await Clients.OthersInGroup(group).SendAsync("Receive_Reload", playerId, result);

            return result;
        }

        public async Task<bool> Send_SwitchPrimary(string group, int playerId)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_SwitchPrimary {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.SwitchPrimary(player);

            // broadcast this AddItem to all other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_SwitchPrimary", playerId, result);

            return result;
        }

        public async Task<bool> Send_Turn(string group, int playerId, float yaw, float pitch, float roll)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Turn {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.Turn(player, yaw, pitch, roll);

            // broadcast this AddItem to all other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_Turn", playerId, yaw, pitch, roll, result);

            return result;
        }

        public async Task<bool> Send_AddItem(string group, string elementJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_AddItem");

            // deserialize element
            var element = RemoteMap.FromJson<Element>(elementJson);

            var map = GetSourceOfTruthMap(group);
            var result = map.AddItem(element);

            // broadcast this AddItem to all other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_AddItem", elementJson, result);

            return result;
        }

        public async Task<bool> Send_RemoveItem(string group, string elementJson)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_RemoveItem");

            // deserialize element
            var element = RemoteMap.FromJson<Element>(elementJson); 

            var map = GetSourceOfTruthMap(group);
            var result = map.RemoveItem(element);

            // broadcast this RemoveItem to all other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_RemoveItem", elementJson, result);

            return result;
        }

        public async Task<AttackStateEnum> Send_Attack(string group, int playerId)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Attack {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.Attack(player);

            // broadcast this RemoveItem to all other clients
            await Clients.OthersInGroup(group).SendAsync("Receive_Attack", playerId, result);

            return result;
        }

        public async Task<string> Send_Drop(string group, int playerId)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Drop {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.Drop(player);

            // broadcast this Drop to all other clients
            var json = "";
            if (result != null)
            {     
                // serialize type
                json = RemoteMap.ToJson<Type>(result);

                if (string.IsNullOrWhiteSpace(json)) throw new Exception($"Failed to serialize type : {result}");

                await Clients.OthersInGroup(group).SendAsync("Receive_Drop", playerId, json);
            }

            return json;
        }

        public async Task<string> Send_Pickup(string group, int playerId)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Pickup {playerId}");

            var map = GetSourceOfTruthMap(group);
            var player = map.GetPlayer(playerId);
            var result = map.Pickup(player);

            var json = "";
            if (result != null)
            {   
                // serialize type
                json = RemoteMap.ToJson<Type>(result);

                if (string.IsNullOrWhiteSpace(json)) throw new Exception($"Failed to serialize type : {result}");

                // broadcast this Pickup to all other clients
                await Clients.OthersInGroup(group).SendAsync("Receive_Pickup", playerId, json);
            }

            return json;
        }

        public async Task<MoveResult> Send_Move(string group, int playerId, float xdelta, float ydelta, float zdelta, float pace)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_Move {playerId}");

            var map = GetSourceOfTruthMap(group);

            var player = map.GetPlayer(playerId);

            if (player == null) throw new Exception($"Failed to get player : {playerId}");

            // make copies
            var localxdelta = xdelta;
            var localydelta = ydelta;
            var localzdelta = zdelta;

            // apply to actual map
            var outcome = map.Move(player, ref localxdelta, ref localydelta, ref localzdelta, out Element touching, pace);
            var result = new MoveResult() { ElementId = touching == null ? -1 : touching.Id, Outcome = outcome };

            // broadcast this Pickup to all other clients
            if (outcome) await Clients.OthersInGroup(group).SendAsync("Receive_Move", playerId, xdelta, ydelta, zdelta, pace, result);

            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[out] Send_Move {playerId} {player.X} {player.Y} {player.Z}");

            return result;
        }

        public async Task<bool> Send_MoveAbsolute(string group, int playerId, float x, float y, float z)
        {
            if (DEBUG_TRACING) System.Diagnostics.Debug.WriteLine($"[in] Send_MoveAbsolute {playerId}");

            var map = GetSourceOfTruthMap(group);

            var player = map.GetPlayer(playerId);

            if (player == null) throw new Exception($"Failed to get player : {playerId}");

            // apply to actual map
            var result = map.MoveAbsolute(player, x, y, z);

            // broadcast this to other clients
            if (result) await Clients.OthersInGroup(group).SendAsync("Receive_MoveAbsolute", playerId, x, y, z, result);

            return result;
        }

        #region private
        class ConnectionDetails
        {
            public string Group;
            public IMap SourceOfTruthMap;
            public IUserInteraction World;
        }
        private static ReaderWriterLockSlim ConnectionsLock = new ReaderWriterLockSlim();
        private static Dictionary<string /*group*/, ConnectionDetails> Connections = new Dictionary<string, ConnectionDetails>();
        private const bool DEBUG_TRACING = true;

        private IMap GetSourceOfTruthMap(string group)
        {
            // get details
            try
            {
                ConnectionsLock.EnterReadLock();
                if (!Connections.TryGetValue(group, out ConnectionDetails details)) return null;
                return details.SourceOfTruthMap;
            }
            finally
            {
                ConnectionsLock.ExitReadLock();
            }
        }

        private string GenerateUniqueGroup()
        {
            var group = "";
            try
            {
                ConnectionsLock.EnterReadLock();
                do
                {
                    // todo make linux friendly
                    group = Guid.NewGuid().ToString();
                } while (Connections.ContainsKey(group));
            }
            finally
            {
                ConnectionsLock.ExitReadLock();
            }
            return group;
        }

        private bool AddConnection(string group, ConnectionDetails connection)
        {
            try
            {
                ConnectionsLock.EnterWriteLock();
                Connections.Add(group, connection);
            }
            finally
            {
                ConnectionsLock.ExitWriteLock();
            }

            return true;
        }
        #endregion
    }
}

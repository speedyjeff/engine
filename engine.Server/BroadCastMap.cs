using engine.Common;
using engine.Common.Entities;
using engine.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace engine.Server
{
    class BroadCastMap : IMap
    {
        public BroadCastMap(IMap map, RemoteMapHub hub)
        {
            Map = map;
            Server = hub;
        }

        public event Action<Element, Element> OnElementHit;
        public event Action<Element> OnElementDied;

        //
        // local operations
        //
        public int Width { get { return Map.Width; } }

        public int Height { get { return Map.Height; } }

        public int Depth { get { return Map.Depth; } }

        public bool IsPaused { get { return Map.IsPaused; } set { Map.IsPaused = value; } }

        public Background Background { get { return Map.Background; } }

        public List<EphemerialElement> GetEphemerials()
        {
            return Map.GetEphemerials();
        }

        public Player GetPlayer(int id)
        {
            return Map.GetPlayer(id);
        }

        public MapStats GetStats()
        {
            return Map.GetStats();
        }

        public bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            return Map.IsTouching(elem1, elem2, x1delta, y1delta, z1delta);
        }

        public List<Player> WhatPlayersAreTouching(Element elem)
        {
            return Map.WhatPlayersAreTouching(elem);
        }

        public bool WhatWouldPlayerTouch(Element elem, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace = 0)
        {
            return Map.WhatWouldPlayerTouch(elem, ref xdelta, ref ydelta, ref zdelta, out touching, pace);
        }

        public IEnumerable<Element> WithinWindow(float x, float y, float z, float width, float height, float depth)
        {
            return Map.WithinWindow(x, y, z, width, height, depth);
        }

        //
        // communicate to other clients
        //
        public bool AddItem(Element item)
        {
            return Task.Run(async () => { return await Server.Send_AddItem(item); }).Result;
        }

        public AttackStateEnum Attack(Player player)
        {
            return Task.Run(async () => { return await Server.Send_Attack(player); }).Result;
        }

        public Type Drop(Player player)
        {
            return Task.Run(async () => { return await Server.Send_Drop(player); }).Result;
        }

        public bool Move(Player player, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace)
        {
            var orgxdelta = xdelta;
            var orgydelta = ydelta;
            var orgzdelta = zdelta;
            var result = Task.Run(async () => { return await Server.Send_Move(player, orgxdelta, orgydelta, orgzdelta, pace);}).Result;

            touching = result.Item1;
            xdelta = result.Item2;
            ydelta = result.Item3;
            zdelta = result.Item4;

            return result.Item5;
        }

        public Type Pickup(Player player)
        {
            return Task.Run(async () => { return await Server.Send_Pickup(player); }).Result;
        }

        public bool RemoveItem(Element item)
        {
            return Task.Run(async () => { return await Server.Send_RemoveItem(item); }).Result;
        }

        #region private
        private IMap Map;
        private RemoteMapHub Server;
        #endregion
    }
}

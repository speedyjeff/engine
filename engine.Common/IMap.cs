using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public struct MapStats
    {
        public int PlayersAlive;
        public int PlayerCount;
    }

    public interface IMap
    {
        int Width { get; }
        int Height { get; }
        int Depth { get; }

        bool IsPaused { get; set; }

        Background Background { get; }

        event Action<Element, Element> OnElementHit;
        event Action<Element> OnElementDied;

        IEnumerable<Element> WithinWindow(float x, float y, float z, float width, float height, float depth);
        bool Move(Player player, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace);
        bool WhatWouldPlayerTouch(Element elem, ref float xdelta, ref float ydelta, ref float zdelta, out Element touching, float pace = 0);
        List<Player> WhatPlayersAreTouching(Element elem);
        Type Pickup(Player player);
        AttackStateEnum Attack(Player player);
        Type Drop(Player player);
        bool AddItem(Element item);
        bool RemoveItem(Element item);
        Player GetPlayer(int id);
        MapStats GetStats();
        List<EphemerialElement> GetEphemerials();
        bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0);
    }
}

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using engine.Common.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace engine.Common.Tests.Network
{
    [TestClass]
    public class Serialization
    {
        [TestMethod]
        public void SerializeElement()
        {
            var elem = new Element() { X = 50, Y = 100 };
            var json = JsonSerializer.Serialize<Element>(elem);
            var elem2 = JsonSerializer.Deserialize<Element>(json);
            Assert.IsTrue(elem.X == elem2.X && elem.Y == elem2.Y, $"Json ({json}) did not work properly");
        }

        [TestMethod]
        public void SerializePlayer()
        {
            var player = new Player() { X = 50, Y = 100 };
            var json = JsonSerializer.Serialize<Player>(player);
            var player2 = JsonSerializer.Deserialize<Element>(json);
            Assert.IsTrue(player.X == player2.X && player.Y == player2.Y, $"Json ({json}) did not work properly");
        }
    }
}

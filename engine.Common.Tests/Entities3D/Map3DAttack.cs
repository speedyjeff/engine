using System.Threading;
using engine.Common.Entities;
using engine.Common.Entities3D;

namespace engine.Common.Tests.Entities3D
{
    [TestClass]
    public class Map3DAttack
    {
        [TestMethod]
        public void AttackCreatesStyled3DTrajectory()
        {
            var player = new TestPlayer3D()
            {
                X = 100f,
                Y = 0f,
                Z = 100f,
                Width = 50f,
                Height = 60f,
                Depth = 50f,
                ShowDefaultDrawing = false,
                Body = new Humanoid3D(),
            };

            var weapon = new TestWeapon3D()
            {
                Damage = 9,
                Distance = 500,
                Delay = 0,
                ProjectileSize = 14f,
                ProjectileColor = new RGBA() { R = 12, G = 180, B = 240, A = 255 },
            };
            weapon.AddAmmo(1);
            Assert.IsTrue(weapon.Reload());
            Thread.Sleep(2);
            player.Equip(weapon);

            var map = new Map3D(
                width: 1000,
                height: 1000,
                depth: 1000,
                players: new Player[] { player },
                objects: Array.Empty<Element>(),
                background: new Background(1000, 1000) { GroundColor = new RGBA() { A = 255 } });
            map.IsPaused = false;

            ShotTrajectory3D? createdTrajectory = null;
            map.OnAddEphemerial += elem => createdTrajectory = elem as ShotTrajectory3D;

            var result = map.Attack(player);

            Assert.AreEqual(AttackStateEnum.Fired, result);
            Assert.IsNotNull(createdTrajectory);
            Assert.AreEqual(player.Id, createdTrajectory.SourcePlayerId);
            Assert.AreEqual(14f, createdTrajectory.Width, 0.001f);
            Assert.AreEqual(14f, createdTrajectory.Height, 0.001f);
            Assert.AreEqual(14f, createdTrajectory.Depth, 0.001f);
            Assert.IsNotNull(createdTrajectory.Body);
            Assert.AreEqual(14f, createdTrajectory.Body.Width, 0.001f);
            Assert.AreEqual(14f, createdTrajectory.Body.Height, 0.001f);
            Assert.AreEqual(14f, createdTrajectory.Body.Depth, 0.001f);
            Assert.AreEqual(12, createdTrajectory.Body.UniformColor.R);
            Assert.AreEqual(180, createdTrajectory.Body.UniformColor.G);
            Assert.AreEqual(240, createdTrajectory.Body.UniformColor.B);
            Assert.AreEqual(255, createdTrajectory.Body.UniformColor.A);
        }

        private sealed class TestPlayer3D : Player3D
        {
            public void Equip(Element item)
            {
                Primary = item;
            }
        }

        private sealed class TestWeapon3D : RangeWeapon3D
        {
            public TestWeapon3D()
            {
                ClipCapacity = 1;
            }
        }
    }
}

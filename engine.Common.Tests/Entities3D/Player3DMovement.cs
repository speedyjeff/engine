using engine.Common.Entities3D;

namespace engine.Common.Tests.Entities3D
{
    [TestClass]
    public class Player3DMovement
    {
        [TestMethod]
        public void BodySetterAlignsBodyToPlayerTransform()
        {
            var player = new Player3D()
            {
                X = 125f,
                Y = -40f,
                Z = 300f,
                Width = 42f,
                Height = 68f,
                Depth = 52f,
                ShowDefaultDrawing = false,
            };

            var body = new Humanoid3D();
            player.Body = body;

            Assert.AreEqual(player.X, body.X, 0.001f);
            Assert.AreEqual(player.Y, body.Y, 0.001f);
            Assert.AreEqual(player.Z, body.Z, 0.001f);
            Assert.AreEqual(player.Width, body.Width, 0.001f);
            Assert.AreEqual(player.Height, body.Height, 0.001f);
            Assert.AreEqual(player.Depth, body.Depth, 0.001f);
        }

        [TestMethod]
        public void MoveUpdatesPlayerAndBodyTogether()
        {
            var player = new Player3D()
            {
                X = 10f,
                Y = 20f,
                Z = 30f,
                Width = 50f,
                Height = 70f,
                Depth = 55f,
                ShowDefaultDrawing = false,
            };

            var body = new Humanoid3D();
            player.Body = body;

            player.Move(5f, -3f, 7f);

            Assert.AreEqual(15f, player.X, 0.001f);
            Assert.AreEqual(17f, player.Y, 0.001f);
            Assert.AreEqual(37f, player.Z, 0.001f);

            Assert.AreEqual(player.X, body.X, 0.001f);
            Assert.AreEqual(player.Y, body.Y, 0.001f);
            Assert.AreEqual(player.Z, body.Z, 0.001f);
            Assert.AreEqual(player.Width, body.Width, 0.001f);
            Assert.AreEqual(player.Height, body.Height, 0.001f);
            Assert.AreEqual(player.Depth, body.Depth, 0.001f);
            Assert.IsTrue(body.WalkPhase > 0f, "Humanoid walk animation should advance when moving.");
        }
    }
}

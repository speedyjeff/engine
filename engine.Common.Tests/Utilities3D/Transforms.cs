using engine.Common.Entities3D;

namespace engine.Common.Tests
{
    [TestClass]
    public class Transforms
    {
        [TestMethod]
        public void YawNinetyRotatesForwardVector()
        {
            var x = 0f;
            var y = 0f;
            var z = -10f;

            Utilities3D.Yaw(90, ref x, ref y, ref z);

            Assert.AreEqual(-10f, x, 0.001f);
            Assert.AreEqual(0f, y, 0.001f);
            Assert.AreEqual(0f, z, 0.001f);
        }

        [TestMethod]
        public void PitchNinetyRotatesForwardVectorUpward()
        {
            var x = 0f;
            var y = 0f;
            var z = -10f;

            Utilities3D.Pitch(90, ref x, ref y, ref z);

            Assert.AreEqual(0f, x, 0.001f);
            Assert.AreEqual(10f, y, 0.001f);
            Assert.AreEqual(0f, z, 0.001f);
        }

        [TestMethod]
        public void RollNinetyRotatesHorizontalVector()
        {
            var x = 10f;
            var y = 0f;
            var z = 0f;

            Utilities3D.Roll(90, ref x, ref y, ref z);

            Assert.AreEqual(0f, x, 0.001f);
            Assert.AreEqual(10f, y, 0.001f);
            Assert.AreEqual(0f, z, 0.001f);
        }

        [TestMethod]
        public void PerspectivePullsCoordinatesTowardCenter()
        {
            var x = 100f;
            var y = 50f;
            var z = -50f;

            var ratio = Utilities3D.Perspective(100f, ref x, ref y, ref z);

            Assert.AreEqual(0.5f, ratio, 0.001f);
            Assert.AreEqual(50f, x, 0.001f);
            Assert.AreEqual(25f, y, 0.001f);
            Assert.AreEqual(-50f, z, 0.001f);
        }
    }
}

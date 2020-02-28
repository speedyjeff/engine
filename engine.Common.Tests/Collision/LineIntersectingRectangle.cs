using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace engine.Common.Tests
{
    [TestClass]
    public class LineIntersectingRectangle
    {
        [TestMethod]
        public void Edges()
        {
            // rectangle
            float x, y;
            float width, height;
            x = y = 1;
            width = height = 50;

            var index = 0;
            foreach (var input in new Quad[]
            {
                // bottom
                new Quad()
                {
                    X1 = 10,
                    Y1 = -35,
                    X2 = 10,
                    Y2 = 10,
                    Value = 11f
                },
                // top
                new Quad()
                {
                    X1 = 10,
                    Y1 = 35,
                    X2 = 10,
                    Y2 = 10,
                    Value = 9f
                },
                // left
                new Quad()
                {
                    X1 = -35,
                    Y1 = 10,
                    X2 = 10,
                    Y2 = 10,
                    Value = 11f
                },
                // right
                new Quad()
                {
                    X1 = 35,
                    Y1 = 10,
                    X2 = 10,
                    Y2 = 10,
                    Value = 9f
                }
            })
            {
                var distance = Collision.LineIntersectingRectangle(
                    input.X1, input.Y1, input.X2, input.Y2,
                    x, y, width, height);

                Assert.AreEqual(input.Value, distance, string.Format("Test {0} : {1} != {2}", index, input.Value, distance));
                index++;
            }
        }

        [TestMethod]
        public void Corner()
        {
            // rectangle
            float x, y;
            float width, height;
            x = y = 25;
            width = height = 50;

            var index = 0;
            foreach (var input in new Quad[]
            {
                // corner
                new Quad()
                {
                    X1 = 40,
                    Y1 = -10,
                    X2 = 60,
                    Y2 = 10,
                    Value = 14.1421356f
                }
            })
            {
                var distance = Collision.LineIntersectingRectangle(
                    input.X1, input.Y1, input.X2, input.Y2,
                    x, y, width, height);

                Assert.AreEqual(input.Value, distance, string.Format("Test {0} : {1} != {2}", index, input.Value, distance));
                index++;
            }
        }
    }
}

using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Cube : Element3D
    {
        public Cube()
        {
            var multiplier = 20f;
            Width = 2f * multiplier;
            Height = 2f * multiplier;
            Depth = 2f * multiplier;
            UniformColor = new RGBA() { R = 33, G = 186, B = 75, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.5f } },
            };
        }
    }
}

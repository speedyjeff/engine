using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Hexagon : Element3D
    {
        public Hexagon()
        {
            Width = 58;
            Height = 50;
            Depth = 15;
            UniformColor = new RGBA() { R = 255, G = 126, B = 36, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = 0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.2413793f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.2413793f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f } },
                new Point[] { new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = 0.25862068f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f } },
                new Point[] { new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.2413793f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.25862068f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -3.061617E-17f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f } },
                new Point[] { new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.25862068f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.25862068f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.2413793f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.25862068f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f } },
                new Point[] { new Point() { X = -0.2413793f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -3.061617E-17f }, new Point() { X = 0.2413793f, Y = 0.5f, Z = -0.5f } },
            };
        }
    }
}

using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Wedge : Element3D
    {
        public Wedge()
        {
            Width = 40;
            Height = 40;
            Depth = 20;
            UniformColor = new RGBA() { R = 0, G = 160, B = 232, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.5f, Y = -0.5f, Z = 3.061617E-17f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = 0.5f } },
            };
        }
    }
}

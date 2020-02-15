using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Sheet : Element3D
    {
        public Sheet()
        {
            Width = 40;
            Height = 0;
            Depth = 40;
            UniformColor = new RGBA() { R = 33, G = 186, B = 75, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0f, Z = -0.5f } },
            };
        }
    }
}

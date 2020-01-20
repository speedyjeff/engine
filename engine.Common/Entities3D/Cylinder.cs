using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Cylinder : Element3D
    {
        public Cylinder()
        {
            Width = 40;
            Height = 40;
            Depth = 40;
            UniformColor = new RGBA() { R = 0, G = 160, B = 232, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = 0.075f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.125f, Y = 0.5f, Z = -0.475f } },
                new Point[] { new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f }, new Point() { X = 0.475f, Y = -0.5f, Z = 0.125f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f } },
                new Point[] { new Point() { X = -0.125f, Y = 0.5f, Z = -0.475f }, new Point() { X = -0.325f, Y = 0.5f, Z = -0.4f }, new Point() { X = 0.075f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.075f, Y = 0.5f, Z = -0.5f }, new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f }, new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f } },
                new Point[] { new Point() { X = -0.325f, Y = 0.5f, Z = -0.4f }, new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f }, new Point() { X = 0.075f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.075f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f } },
                new Point[] { new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f }, new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f }, new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f } },
                new Point[] { new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f } },
                new Point[] { new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.425f, Y = -0.5f, Z = -0.275f }, new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = -0.025f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.025f }, new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f } },
                new Point[] { new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.225f, Y = -0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = 0.425f, Y = -0.5f, Z = -0.275f }, new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f }, new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.025f }, new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.225f, Y = -0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = 0.025f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.225f, Y = 0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.025f }, new Point() { X = 0.275f, Y = 0.5f, Z = -0.425f } },
                new Point[] { new Point() { X = 0.025f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f }, new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f }, new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f }, new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f }, new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f }, new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f }, new Point() { X = -0.475f, Y = -0.5f, Z = 0.175f }, new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f } },
                new Point[] { new Point() { X = 0.225f, Y = -0.5f, Z = 0.45f }, new Point() { X = 0.275f, Y = -0.5f, Z = -0.425f }, new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.025f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.025f }, new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f } },
                new Point[] { new Point() { X = 0.425f, Y = -0.5f, Z = -0.275f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.075f }, new Point() { X = 0.475f, Y = -0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.025f }, new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f }, new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = -0.125f, Y = 0.5f, Z = -0.475f }, new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f }, new Point() { X = -0.325f, Y = 0.5f, Z = -0.4f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f }, new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f }, new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f }, new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f }, new Point() { X = -0.45f, Y = -0.5f, Z = -0.225f } },
                new Point[] { new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f }, new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f }, new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f } },
                new Point[] { new Point() { X = -0.45f, Y = -0.5f, Z = -0.225f }, new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f }, new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f } },
                new Point[] { new Point() { X = 0.4f, Y = 0.5f, Z = 0.325f }, new Point() { X = 0.025f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.225f, Y = 0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f }, new Point() { X = 0.4f, Y = 0.5f, Z = 0.325f }, new Point() { X = 0.225f, Y = 0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = 0.225f, Y = -0.5f, Z = 0.45f }, new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f }, new Point() { X = 0.225f, Y = 0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f }, new Point() { X = 0.425f, Y = -0.5f, Z = -0.275f }, new Point() { X = 0.475f, Y = -0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.025f }, new Point() { X = -0.475f, Y = -0.5f, Z = 0.175f } },
                new Point[] { new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f }, new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f }, new Point() { X = 0.475f, Y = -0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f }, new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f } },
                new Point[] { new Point() { X = -0.5f, Y = -0.5f, Z = -0.025f }, new Point() { X = -0.45f, Y = -0.5f, Z = -0.225f }, new Point() { X = -0.475f, Y = -0.5f, Z = 0.175f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f }, new Point() { X = 0.475f, Y = -0.5f, Z = 0.125f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.075f } },
                new Point[] { new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f }, new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f }, new Point() { X = 0.4f, Y = 0.5f, Z = 0.325f } },
                new Point[] { new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f }, new Point() { X = 0.025f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.4f, Y = 0.5f, Z = 0.325f } },
                new Point[] { new Point() { X = 0.4f, Y = 0.5f, Z = 0.325f }, new Point() { X = 0.4f, Y = -0.5f, Z = 0.325f }, new Point() { X = 0.475f, Y = 0.5f, Z = 0.125f } },
                new Point[] { new Point() { X = -0.325f, Y = 0.5f, Z = -0.4f }, new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f }, new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f } },
                new Point[] { new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f }, new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f }, new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f } },
                new Point[] { new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f }, new Point() { X = -0.475f, Y = 0.5f, Z = 0.175f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f } },
                new Point[] { new Point() { X = -0.45f, Y = -0.5f, Z = -0.225f }, new Point() { X = -0.5f, Y = -0.5f, Z = -0.025f }, new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.075f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.075f }, new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f }, new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.425f, Y = 0.5f, Z = -0.275f }, new Point() { X = 0.5f, Y = -0.5f, Z = -0.075f }, new Point() { X = 0.425f, Y = -0.5f, Z = -0.275f } },
                new Point[] { new Point() { X = -0.325f, Y = 0.5f, Z = -0.4f }, new Point() { X = -0.325f, Y = -0.5f, Z = -0.4f }, new Point() { X = -0.45f, Y = 0.5f, Z = -0.225f } },
                new Point[] { new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f }, new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f }, new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f } },
                new Point[] { new Point() { X = -0.125f, Y = 0.5f, Z = -0.475f }, new Point() { X = 0.075f, Y = -0.5f, Z = -0.5f }, new Point() { X = -0.125f, Y = -0.5f, Z = -0.475f } },
                new Point[] { new Point() { X = -0.175f, Y = 0.5f, Z = 0.475f }, new Point() { X = -0.175f, Y = -0.5f, Z = 0.475f }, new Point() { X = 0.025f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.475f, Y = -0.5f, Z = 0.175f }, new Point() { X = -0.45f, Y = -0.5f, Z = -0.225f }, new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f } },
                new Point[] { new Point() { X = 0.225f, Y = 0.5f, Z = 0.45f }, new Point() { X = 0.025f, Y = -0.5f, Z = 0.5f }, new Point() { X = 0.225f, Y = -0.5f, Z = 0.45f } },
                new Point[] { new Point() { X = -0.35f, Y = 0.5f, Z = 0.35f }, new Point() { X = -0.475f, Y = -0.5f, Z = 0.175f }, new Point() { X = -0.35f, Y = -0.5f, Z = 0.35f } },
            };
        }
    }
}

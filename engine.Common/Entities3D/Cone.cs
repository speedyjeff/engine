using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Cone : Element3D
    {
        public Cone()
        {
            Width = 39;
            Height = 39;
            Depth = 40;
            UniformColor = new RGBA() { R = 255, G = 126, B = 36, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = -0.37179488f, Y = 0.5f, Z = -0.32051283f }, new Point() { X = 0.37179488f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = -0.14102565f, Y = 0.5f, Z = -0.47435898f } },
                new Point[] { new Point() { X = -0.14102565f, Y = 0.5f, Z = -0.47435898f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.37179488f, Y = 0.5f, Z = -0.32051283f } },
                new Point[] { new Point() { X = 0.115384616f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.115384616f }, new Point() { X = 0.42307693f, Y = 0.5f, Z = -0.2948718f } },
                new Point[] { new Point() { X = 0.42307693f, Y = 0.5f, Z = -0.2948718f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.115384616f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.115384616f, Y = 0.5f, Z = -0.5f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.14102565f, Y = 0.5f, Z = -0.47435898f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.14102565f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.115384616f } },
                new Point[] { new Point() { X = 0.37179488f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.14102565f }, new Point() { X = -0.14102565f, Y = 0.5f, Z = -0.47435898f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.08974359f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.47435898f, Y = 0.5f, Z = 0.16666667f } },
                new Point[] { new Point() { X = -0.47435898f, Y = 0.5f, Z = 0.16666667f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.34615386f, Y = 0.5f, Z = 0.37179488f } },
                new Point[] { new Point() { X = 0.14102565f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.37179488f, Y = 0.5f, Z = 0.37179488f } },
                new Point[] { new Point() { X = -0.34615386f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.115384616f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.34615386f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = -0.115384616f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.47435898f, Y = 0.5f, Z = 0.16666667f } },
                new Point[] { new Point() { X = 0.14102565f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.37179488f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = -0.37179488f, Y = 0.5f, Z = -0.32051283f } },
                new Point[] { new Point() { X = -0.5f, Y = 0.5f, Z = -0.08974359f }, new Point() { X = 0.14102565f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.37179488f, Y = 0.5f, Z = -0.32051283f } },
                new Point[] { new Point() { X = -0.37179488f, Y = 0.5f, Z = -0.32051283f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.08974359f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = 0.14102565f }, new Point() { X = 0.5f, Y = 0.5f, Z = -0.115384616f }, new Point() { X = 0.115384616f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.37179488f, Y = 0.5f, Z = 0.37179488f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.14102565f } },
                new Point[] { new Point() { X = -0.14102565f, Y = 0.5f, Z = -0.47435898f }, new Point() { X = 0.5f, Y = 0.5f, Z = 0.14102565f }, new Point() { X = 0.115384616f, Y = 0.5f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.115384616f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.14102565f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.08974359f } },
                new Point[] { new Point() { X = -0.115384616f, Y = 0.5f, Z = 0.5f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.14102565f, Y = 0.5f, Z = 0.5f } },
                new Point[] { new Point() { X = 0.5f, Y = 0.5f, Z = -0.115384616f }, new Point() { X = 0.012820513f, Y = -0.5f, Z = 0.012820513f }, new Point() { X = 0.42307693f, Y = 0.5f, Z = -0.2948718f } },
                new Point[] { new Point() { X = -0.47435898f, Y = 0.5f, Z = 0.16666667f }, new Point() { X = -0.115384616f, Y = 0.5f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0.5f, Z = -0.08974359f } },
            };
        }
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace engine.Common
{
    public struct RGBA
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static RGBA Black = new RGBA() { R = 0, G = 0, B = 0, A = 255 };
        public static RGBA White = new RGBA() { R = 255, G = 255, B = 255, A = 255 };

        public override int GetHashCode()
        {
            return (int)A
                | ((int)B << 8)
                | ((int)G << 16)
                | ((int)R << 24);
        }
    }

    public struct Point
    {
        public float X;
        public float Y;
        public float Z;

        public Point(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Subtract(Point a, Point b)
        {
            return new Point()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y,
                Z = a.Z - b.Z
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Product(Point a, Point b)
        {
            // https://sciencing.com/plane-3-points-8123924.html
            return new Point()
            {
                X = ((a.Y * b.Z) - (a.Z * b.Y)),
                Y = ((a.Z * b.X) - (a.X * b.Z)),
                Z = ((a.X * b.Y) - (a.Y * b.X))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Point a, Point b)
        {
            return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        }

    }

    // 2D Graphics
    // (0,0) ----------------------- (+x, 0)
    //       |
    //       |
    //       |
    //       |
    //    (+y, 0)

    // 3D Graphics
    //                (-y, 0)
    //                   |   / (+z, 0)
    //                   |  /
    //                   | /
    //                   |/
    // (-x,0) ----------------------- (+x, 0)
    //                  /|
    //                 / |
    //                /  |
    //       (-z, 0) /   |
    //                (+y, 0)

    public enum TranslationOptions { None = 0, Translation = 1, Scaling = 2, RotaionYaw = 4, RotationPitch = 8, Default = 0xfff};

    public interface IGraphics
    {
        // drawing
        void Clear(RGBA color);
        void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5f);
        void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5f);
        void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5f); 
        void Text(RGBA color, float x, float y, string text, float fontsize = 16);
        void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness);
        void Image(string path, float x, float y, float width = 0, float height = 0);
        void Image(IImage img, float x, float y, float width = 0, float height = 0);
        void Image(string name, Stream stream, float x, float y, float width = 0, float height = 0);

        // support to project to screen coordinates
        void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon = 0f);
        void DisableTranslation(TranslationOptions options=0);
        void EnableTranslation();

        // 3D support
        void Polygon(RGBA color, Point[] points, bool fill = true, bool border = false, float thickness = 5f);

        // details
        int Height { get; }
        int Width { get; }

        // helper
        IImage CreateImage(int width, int height);
    }
}

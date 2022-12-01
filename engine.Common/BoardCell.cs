using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    // todo add round

    public class BoardCell
    {
        public BoardCell(Point[] points)
        {
            // init
            Points = points;

            // sort in clockwise order
            Collision.OrderPoints(Points, clockwise: true);

            // calculate stastics
            Top = Single.MaxValue;
            Bottom = Single.MinValue;
            Left = Single.MaxValue;
            Right = Single.MinValue;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i].Y < Top) Top = Points[i].Y;
                if (Points[i].X < Left) Left = Points[i].X;
                if (Points[i].Y > Bottom) Bottom = Points[i].Y;
                if (Points[i].X > Right) Right = Points[i].X;
            }
            Width = Right - Left;
            Height = Bottom - Top;

            // compute the normalized form of the points (origin based)
            NormalizedPoints = new Point[Points.Length];
            for(int i=0; i<Points.Length; i++)
            {
                NormalizedPoints[i] = new Point() { X = Points[i].X - Left, Y = Points[i].Y - Top, Z = Points[i].Z };
            }
        }

        public Point[] Points { get; private set; }

        public float Width { get; private set; }
        public float Height { get; private set; }

        public float Top { get; private set; }
        public float Left { get; private set; }
        public float Bottom { get; private set; }
        public float Right { get; private set; }

        public Point[] NormalizedPoints { get; private set; }
    }
}

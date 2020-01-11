using engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyover3D
{
    class Cube : Element
    {
        public override void Draw(IGraphics g)
        {
            // front points (left top corner clockwise)
            var f1 = new Point(X - (Width / 2), Y - (Height / 2), Z - (Depth / 2));
            var f2 = new Point(X + (Width / 2), Y - (Height / 2), Z - (Depth / 2));
            var f3 = new Point(X + (Width / 2), Y + (Height / 2), Z - (Depth / 2));
            var f4 = new Point(X - (Width / 2), Y + (Height / 2), Z - (Depth / 2));

            // back points (left top corner clockwise)
            var b1 = new Point(X - (Width / 2), Y - (Height / 2), Z + (Depth / 2));
            var b2 = new Point(X + (Width / 2), Y - (Height / 2), Z + (Depth / 2));
            var b3 = new Point(X + (Width / 2), Y + (Height / 2), Z + (Depth / 2));
            var b4 = new Point(X - (Width / 2), Y + (Height / 2), Z + (Depth / 2));

            // non-projected
            // front
            g.Polygon(new RGBA() { R = 255, A = 255 }, new Point[] { f1, f2, f3, f4 }, true);
            // top
            g.Polygon(new RGBA() { B = 255, A = 255 }, new Point[] { f1, b1, b2, f2 }, true);
            // back
            g.Polygon(RGBA.Black, new Point[] { b1, b2, b3, b4 }, false);
            // bottom
            g.Polygon(RGBA.Black, new Point[] { f4, b4, b3, f3 }, false);
            // left
            g.Polygon(RGBA.Black, new Point[] { f1, b1, b4, f4 }, false);
            // right
            g.Polygon(RGBA.Black, new Point[] { f2, b2, b3, f3 }, false);
        }
    }
}

using engine.Common;
using engine.Common.Entities;
using engine.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapes
{
    internal class CanvasBackground : Background
    {
        static CanvasBackground()
        {
            // load image
            var images = Resources.LoadImages(System.Reflection.Assembly.GetExecutingAssembly());
            if (images == null || images.Count == 0) throw new Exception("failed to load images");
            foreach (var img in images.Values) { Image = img; break; }
        }

        public override void Draw(IGraphics g)
        {
            // draw all the shapes for validation

            // split the canvas into 9 sections
            var cwidth = g.Width / 3;
            var cheight = g.Height / 3;

            // void Clear(RGBA color);
            g.Clear(RGBA.White);

            //
            // row 0
            //
            // void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5f);
            g.Ellipse(Red,
                x: 0,
                y: 0,
                cwidth,
                cheight);

            // void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5f);
            g.Rectangle(Green,
                x: cwidth,
                y: 0,
                cwidth,
                cheight);

            // void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5f);
            g.Triangle(Blue,
                x1: 0 + (cwidth * 2),
                y1: 0,
                x2: 0 + (cwidth * 2),
                y2: cheight,
                x3: 0 + (cwidth * 3),
                y3: cheight);

            //
            // row 1
            //
            // void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial");
            g.Text(RGBA.Black,
                x: 0,
                y: cheight + (cheight/2),
                "shapes!",
                fontsize: 8);

            // void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness);
            g.Line(Purple,
                x1: cwidth,
                y1: cheight,
                x2: cwidth + cwidth,
                y2: cheight + cheight,
                thickness: 5f);

            // void Polygon(RGBA color, Point[] points, bool fill = true, bool border = false, float thickness = 5f);
            var diamond = new Point[]
            {
                // top
                new Point(x: (cwidth*2) + (cwidth/2),y: cheight + 0,z: 0),

                // left
                new Point(x: (cwidth*2) +0,y: cheight + (cheight/2),z: 0),

                // bottom
                new Point(x: (cwidth*2) +(cwidth/2),y: cheight + cheight,z: 0),

                // right
                new Point(x: (cwidth*2) +cwidth,y: cheight + (cheight/2),z: 0),
            };
            g.Polygon(Yellow,
                diamond);

            // 
            // row 2
            //
            // void Image(IImage img, float x, float y, float width = 0, float height = 0);
            g.Image(Image,
                x: 0,
                y: (cheight * 2),
                cwidth,
                cheight);

            // void Image(IImage img, Point[] points);
            var skew = new Point[]
            {
                // top left
                new Point(x: cwidth + (cwidth/8), y: (cheight*2), z:0),
                // bottom left
                new Point(x: cwidth, y: (cheight*3), z:0),
                // bottom right
                new Point(x: cwidth + (7 * cwidth/8), y: (cheight*3), z:0),
                // top right
                new Point(x: cwidth + cwidth, y: (cheight*2), z:0)
            };
            //g.Image(Image,
            //    skew);
        }

        #region private
        private static RGBA Red = new RGBA() { R = 255, A = 255 };
        private static RGBA Green = new RGBA() { G = 255, A = 255 };
        private static RGBA Blue = new RGBA() { B = 255, A = 255 };
        private static RGBA Purple = new RGBA() { R = 255, B = 255, A = 255 };
        private static RGBA Yellow = new RGBA() { R = 255, G = 255, A = 255 };

        private static IImage Image;
        #endregion
    }
}

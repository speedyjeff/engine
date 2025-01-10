using engine.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace engine.XPlatform
{
    // todo borders

    public class ImageSharpGraphics : IGraphics
    {
        public ImageSharpGraphics(Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img)
        {
            Surface = img;
            Height = Surface.Height;
            Width = Surface.Width;

            FontCache = new Dictionary<string, Dictionary<float, Font>>();

            // create common point sizes
            Points = new PointF[4][];
            for (int i = 1; i < Points.Length; i++) Points[i] = new PointF[i];
        }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public float LevelOfDetail { get; } // 0 is high details, 1 is low detail
        public bool HasChanged { get; internal set; }

        public void Clear(RGBA color)
        {
            HasChanged = true;

            // set rectangle
            Rect.X = 0;
            Rect.Y = 0;
            Rect.Width = Width;
            Rect.Height = Height;

            // clear
            Surface.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgba(color.R, color.G, color.B, color.A), Rect);
            });
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            HasChanged = true;

            // http://www.williammalone.com/briefs/how-to-draw-ellipse-html5-canvas/
            var centerx = x + (width / 2);
            var centery = y + (height / 2);
            Surface.Mutate(ctx =>
            {
                // todo https://github.com/SixLabors/ImageSharp.Drawing/blob/28f3616055d232877b8972922e7a0875ac2f744b/tests/ImageSharp.Drawing.Tests/Drawing/SolidBezierTests.cs

                // first half
                Points[3][0].X = centerx + (width / 2);
                Points[3][0].Y = centery - (height / 2);
                Points[3][1].X = centerx + (width / 2);
                Points[3][1].Y = centery + (height / 2);
                Points[3][2].X = centerx;
                Points[3][2].Y = centery + (height / 2);

                if (!fill) ctx.DrawBeziers(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, Points[3]);
                else throw new NotImplementedException("Ellipse fill:true is not implemented");

                // seconf half
                Points[3][0].X = centerx - (width / 2);
                Points[3][0].Y = centery + (height / 2);
                Points[3][1].X = centerx - (width / 2);
                Points[3][1].Y = centery + (height / 2);
                Points[3][2].X = centerx;
                Points[3][2].Y = centery - (height / 2);

                if (!fill) ctx.DrawBeziers(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, Points[3]);
                else throw new NotImplementedException("Ellipse fill:true is not implemented");
            });
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            HasChanged = true;

            // populate points
            Points[2][0].X = x1;
            Points[2][0].Y = y1;
            Points[2][1].X = x2;
            Points[2][1].Y = y2;

            // draw line
            Surface.Mutate(ctx =>
            {
                ctx.DrawLine(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, Points[2]);
            });
        }

        public void Polygon(RGBA color, Common.Point[] points, bool fill = true, bool border = false, float thickness = 5)
        {
            HasChanged = true;

            // transform points
            var pointfs = Points[3];
            if (points.Length != 3) pointfs = new PointF[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                pointfs[i].X = points[i].X;
                pointfs[i].Y = points[i].Y;
            }

            // draw polygons
            if (fill)
            {
                Surface.Mutate(ctx =>
                {
                    ctx.FillPolygon(Color.FromRgba(color.R, color.G, color.B, color.A), pointfs);
                });
            }
            else
            {
                Surface.Mutate(ctx =>
                {
                    ctx.DrawPolygon(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, pointfs);
                });
            }
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            HasChanged = true;

            Rect.X = x;
            Rect.Y = y;
            Rect.Width = width;
            Rect.Height = height;

            // draw rectangle
            if (fill)
            {
                Surface.Mutate(ctx =>
                {
                    ctx.Fill(Color.FromRgba(color.R, color.G, color.B, color.A), Rect);
                });
            }
            else
            {
                Surface.Mutate(ctx =>
                {
                    ctx.Draw(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, Rect);
                });
            }
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial")
        {
            HasChanged = true;

            // set points
            Points[1][0].X = x;
            Points[1][0].Y = y;

            // draw text
            Surface.Mutate(ctx =>
            {
                ctx.DrawText(text, GetCachedFont(fontname, fontsize), Color.FromRgba(color.R, color.G, color.B, color.A), Points[1][0]);
            });
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5)
        {
            HasChanged = true;

            // transform points
            Points[3][0].X = x1;
            Points[3][0].Y = y1;
            Points[3][1].X = x2;
            Points[3][1].Y = y2;
            Points[3][2].X = x3;
            Points[3][2].Y = y3;

            // draw polygons
            if (fill)
            {
                Surface.Mutate(ctx =>
                {
                    ctx.FillPolygon(Color.FromRgba(color.R, color.G, color.B, color.A), Points[3]);
                });
            }
            else
            {
                Surface.Mutate(ctx =>
                {
                    ctx.DrawPolygon(Color.FromRgba(color.R, color.G, color.B, color.A), thickness, Points[3]);
                });
            }
        }

        //
        // Images
        //
        public Common.IImage CreateImage(int width, int height)
        {
            return new ImageSharpImage(width, height);
        }

        public Common.IImage CreateImage(Stream stream)
        {
            // read the bytes from stream
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return new ImageSharpImage(bytes);
        }

        public Common.IImage CreateImage(string path)
        {
            // load bytes from path
            var bytes = File.ReadAllBytes(path);
            return new ImageSharpImage( bytes);
        }

        public void Image(Common.IImage img, float x, float y, float width = 0, float height = 0)
        {
            HasChanged = true;

            Point.X = (int)x;
            Point.Y = (int)y;

            Surface.Mutate(ctx =>
            {
                ctx.DrawImage((img as ImageSharpImage).UnderlyingImage, Point, opacity: 1f);
            });
        }

        public void Image(Common.IImage img, Common.Point[] points)
        {
            throw new NotImplementedException();
        }

        //
        // translation (no-op)
        //

        public void DisableTranslation(TranslationOptions options = TranslationOptions.None) { }
        public void EnableTranslation() { }
        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon = 0f, float lod = 0f) { }

        #region private
        private Image<SixLabors.ImageSharp.PixelFormats.Rgba32> Surface;
        private PointF[][] Points;
        private RectangleF Rect;
        private SixLabors.ImageSharp.Point Point;
        private Dictionary<string, Dictionary<float, Font>> FontCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Font GetCachedFont(string name, float size)
        {
            Dictionary<float, Font> fonts = null;
            if (!FontCache.TryGetValue(name, out fonts))
            {
                fonts = new Dictionary<float, Font>();
                FontCache.Add(name, fonts);
            }

            var key = (float)Math.Round(size, 2);
            Font font = null;
            if (!fonts.TryGetValue(key, out font))
            {
                // find the font
                FontFamily family;
                try
                {
                    family = SystemFonts.Get(name);
                }
                catch (FontFamilyNotFoundException)
                { 
                    family = Fonts.All.Get(name);
                }

                font = new Font(family, key);
                fonts.Add(key, font);
            }
            return font;
        }
        #endregion
    }
}

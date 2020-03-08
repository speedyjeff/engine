using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using engine.Common;
using engine.Common.Entities3D;

namespace engine.Winforms
{
    public class WritableGraphics : IGraphics
    {
        public WritableGraphics(BufferedGraphicsContext context, Graphics g, int height, int width)
        {
            // init
            Context = context;
            Graphics = g;
            Width = width;
            Height = height;
            ImageCache = new Dictionary<string, Image>();
            SolidBrushCache = new Dictionary<int, SolidBrush>();
            PenCache = new Dictionary<long, Pen>();
            ArialFontCache = new Dictionary<float, Font>();

            // get graphics ready
            if (Context != null) RawResize(g, height, width);
        }

        // access to the Graphics implementation
        public Graphics RawGraphics => Graphics;
        public void RawRender(Graphics g) { Surface.Render(g); }
        public void RawResize(Graphics g, int height, int width)
        {
            if (Context == null) return;

            // initialize the double buffer
            Width = width;
            Height = height;
            Context.MaximumBuffer = new Size(width + 1, height + 1);
            if (Surface != null)
            {
                Surface.Dispose();
            }
            Surface = Context.Allocate(g,
                new Rectangle(0, 0, width, height));
            Graphics = Surface.Graphics;
        }

        // high level access to drawing
        public void Clear(RGBA color)
        {
            Graphics.FillRectangle(GetCachedSolidBrush(color), 0, 0, Width, Height);
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill, bool border, float thickness)
        {
            if (fill)
            {
                Graphics.FillEllipse(GetCachedSolidBrush(color), x, y, width, height);
                if (border) Graphics.DrawEllipse(GetCachedPen(RGBA.Black, thickness), x, y, width, height);
            }
            else
            {
                Graphics.DrawEllipse(GetCachedPen(color, thickness), x, y, width, height);
            }
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill, bool border, float thickness)
        {
            if (fill)
            {
                Graphics.FillRectangle(GetCachedSolidBrush(color), x, y, width, height);
                if (border) Graphics.DrawRectangle(GetCachedPen(RGBA.Black, thickness), x, y, width, height);
            }
            else
            {
                Graphics.DrawRectangle(GetCachedPen(color, thickness), x, y, width, height);
            }
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill, bool border, float thickness)
        {
            // use screen coordinates
            var edges = new PointF[]
            {
                new PointF(x1, y1),
                new PointF(x2, y2),
                new PointF(x3, y3)
            };
            if (fill)
            {
                Graphics.FillPolygon(GetCachedSolidBrush(color), edges);
                if (border) Graphics.DrawPolygon(GetCachedPen(RGBA.Black, thickness), edges);
            }
            else
            {
                Graphics.DrawPolygon(GetCachedPen(RGBA.Black, thickness), edges);
            }
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16)
        {
            Graphics.DrawString(text, GetCachedArialFont(fontsize), GetCachedSolidBrush(color), x, y);
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            Graphics.DrawLine(GetCachedPen(color, thickness), x1, y1, x2, y2);
        }

        public void Image(string path, float x, float y, float width = 0, float height = 0)
        {
            Image img = GetCachedImage(path);
            Graphics.DrawImage(img, x, y, width, height);
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            var bitmap = img as BitmapImage;
            if (bitmap == null
                || bitmap.UnderlyingImage == null) throw new Exception("Image(IImage) must be used with a BitmapImage");
            Graphics.DrawImage(bitmap.UnderlyingImage, x, y, width, height);
        }

        public void Image(string name, Stream stream, float x, float y, float width = 0, float height = 0)
        {
            Image img = GetCachedImage(name, stream);

            Graphics.DrawImage(img, x, y, width, height);
        }

        // no-op
        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon) { }
        public void EnableTranslation() { }
        public void DisableTranslation(TranslationOptions options) {}

        public void Polygon(RGBA color, Common.Point[] points, bool fill = false, bool border = false, float thickness = 5f)
        {
            if (points == null || points.Length <= 1) throw new Exception("Must provide a valid number of points");

            // convert points into PointF
            var edges = new System.Drawing.PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                edges[i] = new System.Drawing.PointF(points[i].X, points[i].Y);
            }

            if (fill)
            {
                Graphics.FillPolygon(GetCachedSolidBrush(color), edges);
                if (border) Graphics.DrawPolygon(GetCachedPen(RGBA.Black, thickness), edges);
            }
            else
            {
                Graphics.DrawPolygon(GetCachedPen(color, thickness), edges);
            }
        }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public IImage CreateImage(int width, int height)
        {
            return new BitmapImage(width, height);
        }

        #region private
        private Graphics Graphics;
        private BufferedGraphics Surface;
        private BufferedGraphicsContext Context;

        // caches
        private Dictionary<string, Image> ImageCache;
        private Dictionary<int, SolidBrush> SolidBrushCache;
        private Dictionary<long, Pen> PenCache;
        private Dictionary<float, Font> ArialFontCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Image GetCachedImage(string path, Stream stream = null)
        {
            System.Drawing.Image img = null;
            if (!ImageCache.TryGetValue(path, out img))
            {
                if (stream != null) img = System.Drawing.Image.FromStream(stream);
                else img = System.Drawing.Image.FromFile(path);
                var bitmap = new Bitmap(img);
                bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
                ImageCache.Add(path, bitmap);
            }
            return img;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SolidBrush GetCachedSolidBrush(RGBA color)
        {
            var key = color.GetHashCode();
            SolidBrush brush = null;
            if (!SolidBrushCache.TryGetValue(key, out brush))
            {
                brush = new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                SolidBrushCache.Add(key, brush);
            }
            return brush;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Pen GetCachedPen(RGBA color, float thickness)
        {
            var key = (long)color.GetHashCode() | ((long)thickness << 32);
            Pen pen = null;
            if (!PenCache.TryGetValue(key, out pen))
            {
                pen = new Pen(Color.FromArgb(color.A, color.R, color.G, color.B), thickness);
                PenCache.Add(key, pen);
            }
            return pen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Font GetCachedArialFont(float size)
        {
            var key = (float)Math.Round(size, 2);
            Font font = null;
            if (!ArialFontCache.TryGetValue(key, out font))
            {
                font = new Font("Arial", key);
                ArialFontCache.Add(key, font);
            }
            return font;
        }
        #endregion
    }
}

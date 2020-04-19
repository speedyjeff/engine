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
            SolidBrushCache = new Dictionary<int, SolidBrush>();
            PenCache = new Dictionary<long, Pen>();
            FontCache = new Dictionary<string, Dictionary<float, Font>>();
            // 3 pointf's is the most common type, so use one as a cache (to avoid the allocation)
            TriPoints = new PointF[3];

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

            // cleanup
            if (Surface != null) Surface.Dispose();

            // recreate
            Surface = Context.Allocate(g, new Rectangle(0, 0, width, height));
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
            TriPoints[0].X = x1; TriPoints[0].Y = y1;
            TriPoints[1].X = x2; TriPoints[1].Y = y2;
            TriPoints[2].X = x3; TriPoints[2].Y = y3;
            if (fill)
            {
                Graphics.FillPolygon(GetCachedSolidBrush(color), TriPoints);
                if (border) Graphics.DrawPolygon(GetCachedPen(RGBA.Black, thickness), TriPoints);
            }
            else
            {
                Graphics.DrawPolygon(GetCachedPen(RGBA.Black, thickness), TriPoints);
            }
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial")
        {
            Graphics.DrawString(text, GetCachedFont(fontname, fontsize), GetCachedSolidBrush(color), x, y);
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            Graphics.DrawLine(GetCachedPen(color, thickness), x1, y1, x2, y2);
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            var bitmap = img as BitmapImage;
            if (bitmap == null
                || bitmap.UnderlyingImage == null) throw new Exception("Image(IImage) must be used with a BitmapImage");
            Graphics.DrawImage(bitmap.UnderlyingImage, x, y, width, height);
        }

        // no-op
        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon) { }
        public void EnableTranslation() { }
        public void DisableTranslation(TranslationOptions options) {}

        public void Polygon(RGBA color, Common.Point[] points, bool fill = false, bool border = false, float thickness = 5f)
        {
            if (points == null || points.Length <= 1) throw new Exception("Must provide a valid number of points");

            // convert points into PointF
            PointF[] edges = null;
            if (points.Length == 3)
            {
                TriPoints[0].X = points[0].X; TriPoints[0].Y = points[0].Y;
                TriPoints[1].X = points[1].X; TriPoints[1].Y = points[1].Y;
                TriPoints[2].X = points[2].X; TriPoints[2].Y = points[2].Y;

                edges = TriPoints;
            }
            else
            {
                edges = new System.Drawing.PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    edges[i] = new System.Drawing.PointF(points[i].X, points[i].Y);
                }
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

        public void Image(IImage img, Common.Point[] points)
        {
            var bitmap = img as BitmapImage;
            if (bitmap == null
                || bitmap.UnderlyingImage == null) throw new Exception("Image(IImage) must be used with a BitmapImage");

            // convert points into PointF
            PointF[] edges = null;
            if (points.Length == 3)
            {
                TriPoints[0].X = points[0].X; TriPoints[0].Y = points[0].Y;
                TriPoints[1].X = points[1].X; TriPoints[1].Y = points[1].Y;
                TriPoints[2].X = points[2].X; TriPoints[2].Y = points[2].Y;

                edges = TriPoints;
            }
            else
            {
                edges = new System.Drawing.PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    edges[i] = new System.Drawing.PointF(points[i].X, points[i].Y);
                }
            }

            // draw
            try
            {
                Graphics.DrawImage(bitmap.UnderlyingImage, edges);
            }
            catch (OutOfMemoryException)
            {
                // these errors happen from time to time and are ignorable
                System.Diagnostics.Debug.WriteLine("OOM in Image");
            }
        }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public IImage CreateImage(int width, int height)
        {
            return new BitmapImage(width, height);
        }

        public IImage CreateImage(Stream stream)
        {
            var img = LoadImage(path: "", stream);
            return new BitmapImage(img);
        }

        public IImage CreateImage(string path)
        {
            var img = LoadImage(path, stream: null);
            return new BitmapImage(img);
        }

        #region private
        private Graphics Graphics;
        private BufferedGraphics Surface;
        private BufferedGraphicsContext Context;
        private PointF[] TriPoints;

        internal void Release()
        {
            if (Graphics != null) Graphics.Dispose();
            if (Surface != null) Surface.Dispose();
            if (Context != null) Context.Dispose();
            Graphics = null;
            Surface = null;
            Context = null;
        }

        // caches
        private Dictionary<int, SolidBrush> SolidBrushCache;
        private Dictionary<long, Pen> PenCache;
        private Dictionary<string, Dictionary<float, Font>> FontCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private System.Drawing.Bitmap LoadImage(string path, Stream stream = null)
        {
            System.Drawing.Image img = null;
            if (stream != null) img = System.Drawing.Image.FromStream(stream);
            else img = System.Drawing.Image.FromFile(path);
            var bitmap = new Bitmap(img);
            bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
            return bitmap;
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
                font = new Font(name, key);
                fonts.Add(key, font);
            }
            return font;
        }
        #endregion
    }
}

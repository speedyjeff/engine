﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using engine.Common;

namespace engine.Winforms
{
    public class WritableGraphics : IGraphics
    {
        public WritableGraphics(BufferedGraphicsContext context, Graphics g, int height, int width)
        {
            // init
            DoTranslation = true;
            ImageCache = new Dictionary<string, Image>();
            Context = context;
            Graphics = g;
            Width = width;
            Height = height;

            // get graphics ready
            if (Context != null)
            {
                RawResize(g, height, width);
            }
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
            Graphics.FillRectangle(new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B)), 0, 0, Width, Height);
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill, bool border)
        {
            float thickness = 5f;
            float sx = x;
            float sy = y;
            float swidth = width;
            float sheight = height;
            float sthickness = thickness;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, width, height, thickness, out sx, out sy, out swidth, out sheight, out sthickness)) return;

            // safe guard accidental usage
            x = y = width = height = thickness = 0;

            // use screen coordinates
            if (fill)
            {
                Graphics.FillEllipse(new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B)), sx, sy, swidth, sheight);
                if (border) Graphics.DrawEllipse(new Pen(Color.Black, sthickness), sx, sy, swidth, sheight);
            }
            else
            {
                Graphics.DrawEllipse(new Pen(Color.FromArgb(color.A, color.R, color.G, color.B), sthickness), sx, sy, swidth, sheight);
            }
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill, bool border)
        {
            float thickness = 5f;
            float sx = x;
            float sy = y;
            float swidth = width;
            float sheight = height;
            float sthickness = thickness;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, width, height, thickness, out sx, out sy, out swidth, out sheight, out sthickness)) return;

            // safe guard accidental usage
            x = y = width = height = thickness = 0;

            // use screen coordinates
            if (fill)
            {
                Graphics.FillRectangle(new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B)), sx, sy, swidth, sheight);
                if (border) Graphics.DrawRectangle(new Pen(Color.Black, sthickness), sx, sy, swidth, sheight);
            }
            else
            {
                Graphics.DrawRectangle(new Pen(Color.FromArgb(color.A, color.R, color.G, color.B), sthickness), sx, sy, swidth, sheight);
            }
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill, bool border)
        {
            float thickness = 5f;
            float sthickness = 0f;
            float sx1 = x1;
            float sy1 = y1;
            float sx2 = x2;
            float sy2 = y2;
            float sx3 = x3;
            float sy3 = y3;
            float width = 0;
            float height = 0;
            float other = 0;
            float swidth = 0;
            float sheight = 0;
            float sother = 0;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x1, y1, width, height, thickness, out sx1, out sy1, out swidth, out sheight, out sthickness)) return;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x2, y2, width, height, other, out sx2, out sy2, out swidth, out sheight, out sother)) return;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x3, y3, width, height, other, out sx3, out sy3, out swidth, out sheight, out sother)) return;

            // safe guard accidental usage
            x1 = y1 = x2 = y2 = x3 = y3 = thickness = 0;

            // use screen coordinates
            var edges = new PointF[]
            {
                new PointF(sx1, sy1),
                new PointF(sx2, sy2),
                new PointF(sx3, sy3)
            };
            if (fill)
            {
                Graphics.FillPolygon(new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B)), edges);
                if (border) Graphics.DrawPolygon(new Pen(Color.Black, sthickness), edges);
            }
            else
            {
                Graphics.DrawPolygon(new Pen(Color.Black, sthickness), edges);
            }
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16)
        {
            float sx = x;
            float sy = y;
            float swidth = 0;
            float sheight = 0;
            float sfontsize = fontsize;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, 0, 0, fontsize, out sx, out sy, out swidth, out sheight, out sfontsize)) return;

            // safe guard accidental usage
            x = y = fontsize = 0;

            // use screen coordinates
            Graphics.DrawString(text, new Font("Arial", sfontsize), new SolidBrush(Color.FromArgb(color.A, color.R, color.G, color.B)), sx, sy);
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            float sx1 = x1;
            float sy1 = y1;
            float width = Math.Abs(x1 - x2);
            float height = Math.Abs(y1 - y2);
            float swidth = width;
            float sheight = height;
            float sthickness = thickness;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x1, y1, width, height, thickness, out sx1, out sy1, out swidth, out sheight, out sthickness)) return;

            // safe guard accidental usage
            x1 = y1 = thickness = 0;

            float sx2 = x2;
            float sy2 = y2;
            float sother = 0;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x2, y2, width, height, 0, out sx2, out sy2, out swidth, out sheight, out sother)) return;

            // safe guard accidental usage
            x2 = y2 = 0;

            Graphics.DrawLine(new Pen(Color.FromArgb(color.A, color.R, color.G, color.B), sthickness), sx1, sy1, sx2, sy2);
        }

        public void Image(string path, float x, float y, float width = 0, float height = 0)
        {
            System.Drawing.Image img = null;
            if (!ImageCache.TryGetValue(path, out img))
            {
                img = System.Drawing.Image.FromFile(path);
                var bitmap = new Bitmap(img);
                bitmap.MakeTransparent(bitmap.GetPixel(0,0));
                ImageCache.Add(path, bitmap);
            }

            float sx = x;
            float sy = y;
            float swidth = width == 0 ? img.Width : width;
            float sheight = height == 0 ? img.Height : height;
            float sother = 0;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, swidth, sheight, 0, out sx, out sy, out swidth, out sheight, out sother)) return;

            // safe guard accidental usage
            x = y = 0;

            // use screen coordinates
            Graphics.DrawImage(img, sx, sy, swidth, sheight);
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            var bitmap = img as BitmapImage;
            if (bitmap == null
                || bitmap.UnderlyingImage == null) throw new Exception("Image(IImage) must be used with a BitmapImage");

            float sx = x;
            float sy = y;
            float swidth = width == 0 ? bitmap.Width : width;
            float sheight = height == 0 ? bitmap.Height : height;
            float sother = 0;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, swidth, sheight, 0, out sx, out sy, out swidth, out sheight, out sother)) return;

            // safe guard accidental usage
            x = y = 0;

            // use screen coordinates
            Graphics.DrawImage(bitmap.UnderlyingImage, sx, sy, swidth, sheight);
        }

        public void Image(string name, Stream stream, float x, float y, float width = 0, float height = 0)
        {
            System.Drawing.Image img = null;
            if (!ImageCache.TryGetValue(name, out img))
            {
                img = System.Drawing.Image.FromStream(stream);
                var bitmap = new Bitmap(img);
                bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
                ImageCache.Add(name, bitmap);
            }

            float sx = x;
            float sy = y;
            float swidth = width == 0 ? img.Width : width;
            float sheight = height == 0 ? img.Height : height;
            float sother = 0;
            if (Translate != null && DoTranslation && !Translate(DoScaling, x, y, swidth, sheight, 0, out sx, out sy, out swidth, out sheight, out sother)) return;

            // safe guard accidental usage
            x = y = 0;

            // use screen coordinates
            Graphics.DrawImage(img, sx, sy, swidth, sheight);
        }

        public void RotateTransform(float angle)
        {
            Graphics.TranslateTransform(1 * Width / 2, 1 * Height / 2);
            Graphics.RotateTransform(angle);
            Graphics.TranslateTransform(-1 * Width / 2, -1 * Height / 2);
        }

        public void EnableTranslation()
        {
            DoScaling = true;
            DoTranslation = true;
        }

        public void DisableTranslation(bool nonScaledTranslation)
        {
            DoTranslation = false;
            DoScaling = false;
            if (nonScaledTranslation) DoTranslation = true;
        }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public void SetTranslateCoordinates(TranslateCoordinatesDelegate callback)
        {
            Translate = callback;
        }

        public IImage CreateImage(int width, int height)
        {
            // todo Should this image inherit the TranslateCoordinates from its parent
            return new BitmapImage(width, height);
        }

        #region private
        private Graphics Graphics;
        private BufferedGraphics Surface;
        private BufferedGraphicsContext Context;
        private TranslateCoordinatesDelegate Translate;
        private Dictionary<string, Image> ImageCache;
        private bool DoTranslation;
        private bool DoScaling;
        // TODO! color cache
        #endregion
    }
}

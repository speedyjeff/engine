using engine.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace engine.Maui
{
    public class MauiGraphics : IGraphics
    {
        public MauiGraphics(SKCanvas canvas, int width, int height)
        {
            Canvas = canvas;
            Height = height;
            Width = width;

            // setup caches
            SKColors = new Dictionary<int, SKColor>();
            SKPaints = new Dictionary<long, SKPaint>();
            SKFonts = new Dictionary<string, Dictionary<float, SKFont>>();

            // the most common point array (reused)
            TriPoints = new SKPoint[3];
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public void RawResize(SKCanvas canvas, int width, int height)
        {
            // initialize the double buffer
            Width = width;
            Height = height;

            // cleanup
            if (Canvas != null && Canvas != canvas) Canvas.Dispose();

            // recreate
            Canvas = canvas;
        }

        public void Clear(RGBA color)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");
            Canvas.Clear(GetCachedColor(color));
        }

        public engine.Common.IImage CreateImage(int width, int height)
        {
            return new MauiImage(width, height);
        }

        public engine.Common.IImage CreateImage(Stream stream)
        {
            return new MauiImage(stream);
        }

        public engine.Common.IImage CreateImage(string path)
        {
            return new MauiImage(path);
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");

            if (fill)
            {
                Canvas.DrawOval(
                    cx: x + (width / 2),
                    cy: y + (width / 2),
                    rx: (width / 2f),
                    ry: (height / 2f),
                    GetCachedPaint(color, fill, border: false, thickness: 0));
                if (border) Canvas.DrawOval(
                    cx: x + (width / 2),
                    cy: y + (width / 2),
                    rx: (width / 2f),
                    ry: (height / 2f),
                    GetCachedPaint(RGBA.Black, fill: false, border: true, thickness));
            }
            else
            {
                Canvas.DrawOval(
                    cx: x + (width / 2),
                    cy: y + (width / 2),
                    rx: (width / 2f),
                    ry: (height / 2f),
                    GetCachedPaint(color, fill: false, border: true, thickness));
            }
        }

        public void Image(engine.Common.IImage img, float x, float y, float width = 0, float height = 0)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");
            if (img is not MauiImage) throw new Exception("only compatible with MauiImage");
            var bitmap = (img as MauiImage).UnderlyingBitmap;
            if (width <= 0) width = bitmap.Width;
            if (height <= 0) height = bitmap.Height;
            Canvas.DrawBitmap(
                bitmap,
                new SKRect(
                    left: x,
                    top: y,
                    right: x+ width,
                    bottom: y + height),
                WithTransparency);
        }

        public void Image(engine.Common.IImage img, engine.Common.Point[] points)
        {
            // perhaps Canvas.DrawBitmapLattice
            throw new NotImplementedException();
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");
            Canvas.DrawLine(
                x1,
                y1,
                x2,
                y2,
                GetCachedPaint(color, fill: false, border: false, thickness));
        }

        public void Polygon(RGBA color, engine.Common.Point[] points, bool fill = true, bool border = false, float thickness = 5)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");

            // todo how to cache the SKPath?

            // build the apprpriate path
            var path = new SKPath() { FillType = SKPathFillType.EvenOdd };
            for(int i=0; i<points.Length; i++)
            {
                if (i == 0) path.MoveTo(points[i].X, points[i].Y);
                else path.LineTo(points[i].X, points[i].Y);
            }
            path.Close();

            Canvas.DrawPath(path,
                GetCachedPaint(color, fill, border, thickness));
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");

            Canvas.DrawRect(
                x,
                y,
                width,
                height,
                GetCachedPaint(color, fill, border, thickness));

            if (fill)
            {
                Canvas.DrawRect(
                    x,
                    y,
                    width,
                    height,
                    GetCachedPaint(color, fill, border: false, thickness: 0));
                if (border) Canvas.DrawRect(
                    x,
                    y,
                    width,
                    height,
                    GetCachedPaint(RGBA.Black, fill: false, border: true, thickness));
            }
            else
            {
                Canvas.DrawRect(
                    x,
                    y,
                    width,
                    height,
                    GetCachedPaint(RGBA.Black, fill: false, border: true, thickness));
            }
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial")
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");

            Canvas.DrawText(
                text,
                x,
                y,
                GetCachedFont(fontname, fontsize),
                GetCachedPaint(color, fill: false, border: false, thickness: 0));
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5)
        {
            if (Canvas == null) throw new Exception("must have a valid canvas");

            // todo cache the SKPath
            var path = new SKPath() { FillType = SKPathFillType.EvenOdd };
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            path.LineTo(x3, y3);
            path.Close();

            Canvas.DrawPath(
                path,
                GetCachedPaint(color, fill, border, thickness));
        }

        // no-op
        public void DisableTranslation(TranslationOptions options = TranslationOptions.None) { }
        public void EnableTranslation() { }
        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon = 0) { }

        #region private
        private SKCanvas Canvas;
        private SKPoint[] TriPoints;

        private static SKPaint WithTransparency = new SKPaint()
        {
            Color = new SKColor(red: 0, green: 0, blue: 0, alpha: 250)
        };

        // caches
        private Dictionary<int, SKColor> SKColors;
        private Dictionary<long, SKPaint> SKPaints;
        private Dictionary<string, Dictionary<float, SKFont>> SKFonts;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SKColor GetCachedColor(RGBA color)
        {
            var key = color.GetHashCode();
            if (!SKColors.TryGetValue(key, out SKColor skcolor))
            {
                skcolor = new SKColor(red: color.R, green: color.G, blue: color.B, alpha: color.A);
                SKColors.Add(key, skcolor);
            }
            return skcolor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SKPaint GetCachedPaint(RGBA color, bool fill, bool border, float thickness)
        {
            var key = (long)color.GetHashCode() | ((long)thickness << 32) | (fill ? 1L << 60 : 0) | (border ? 1L << 61 : 0);
            if (!SKPaints.TryGetValue(key, out SKPaint skpaint))
            {
                skpaint = new SKPaint()
                {
                    Style = (fill & border) ? SKPaintStyle.StrokeAndFill :
                            (fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke),
                    Color = GetCachedColor(color),
                    StrokeWidth = thickness
                };
                SKPaints.Add(key, skpaint);
            }
            return skpaint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SKFont GetCachedFont(string name, float size)
        {
            if (!SKFonts.TryGetValue(name, out Dictionary<float, SKFont> fonts))
            {
                fonts = new Dictionary<float, SKFont>();
                SKFonts.Add(name, fonts);
            }

            var key = (float)Math.Round(size, 2);
            if (!fonts.TryGetValue(key, out SKFont font))
            {
                font = new SKFont(
                    SKTypeface.FromFamilyName(name),
                    size);
                fonts.Add(key, font);
            }
            return font;
        }
        #endregion
    }
}

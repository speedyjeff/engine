using Blazor.Extensions.Canvas.Canvas2D;
using engine.Common;
using engine.XPlatform;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace engine.Blazor
{
    // TODO borders are not working

    public class Canvas2DGraphics : IGraphics
    {
        public Canvas2DGraphics(Canvas2DContext surface, Action<IImage, float, float, float, float> drawImageCallback, long width, long height)
        {
            Surface = surface;
            DrawImageCallback = drawImageCallback;
            Width = (int)width;
            Height = (int)height;
            Colors = new Dictionary<int, string>();
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public async void Clear(RGBA color)
        {
            await Surface.BeginBatchAsync();
            {
                await Surface.SetFillStyleAsync(ColorCache(color));
                await Surface.ClearRectAsync(x: 0, y: 0, Width, Height);
            }
            await Surface.EndBatchAsync();
        }

        public async void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            await Surface.BeginBatchAsync();
            {
                // http://www.williammalone.com/briefs/how-to-draw-ellipse-html5-canvas/
                var hex = ColorCache(color);
                var centerx = x + (width / 2);
                var centery = y + (height / 2);
                if (border)
                {
                    await Surface.SetLineWidthAsync(thickness);
                    await Surface.SetStrokeStyleAsync(ColorCache(RGBA.Black));
                }
                else
                {
                    await Surface.SetStrokeStyleAsync(hex);
                }
                await Surface.BeginPathAsync();
                {
                    await Surface.MoveToAsync(centerx, centery - (height / 2));
                    await Surface.BezierCurveToAsync(
                      centerx + (width / 2), centery - (height / 2), // C1
                      centerx + (width / 2), centery + (height / 2), // C2
                      centerx, centery + (height / 2)); // A2

                    await Surface.BezierCurveToAsync(
                      centerx - (width / 2), centery + (height / 2), // C3
                      centerx - (width / 2), centery - (height / 2), // C4
                      centerx, centery - (height / 2)); // A1
                    if (fill)
                    {
                        await Surface.SetFillStyleAsync(hex);
                        await Surface.FillAsync();
                    }
                    else
                    {
                        await Surface.StrokeAsync();
                    }
                }
                await Surface.ClosePathAsync();
            }
            await Surface.EndBatchAsync();
        }

        public async void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            await Surface.BeginBatchAsync();
            {
                await Surface.SetStrokeStyleAsync(ColorCache(color));
                await Surface.SetLineWidthAsync(thickness);
                await Surface.BeginPathAsync();
                {
                    await Surface.MoveToAsync(x1, y1);
                    await Surface.LineToAsync(x2, y2);
                    await Surface.StrokeAsync();
                }
                await Surface.ClosePathAsync();
            }
            await Surface.EndBatchAsync();
        }

        public async void Polygon(RGBA color, Common.Point[] points, bool fill = true, bool border = false, float thickness = 5)
        {
            await Surface.BeginBatchAsync();
            {
                var hex = ColorCache(color);
                if (border)
                {
                    await Surface.SetLineWidthAsync(thickness);
                    await Surface.SetStrokeStyleAsync(ColorCache(RGBA.Black));
                }
                else
                {
                    await Surface.SetStrokeStyleAsync(hex);
                }
                await Surface.BeginPathAsync();
                {
                    await Surface.MoveToAsync(points[0].X, points[0].Y);
                    for (int i = 1; i < points.Length; i++) await Surface.LineToAsync(points[i].X, points[i].Y);
                    await Surface.LineToAsync(points[0].Y, points[0].Y);
                    if (fill)
                    {
                        await Surface.SetFillStyleAsync(hex);
                        await Surface.FillAsync();
                    }
                    else
                    {
                        await Surface.StrokeAsync();
                    }
                }
                await Surface.ClosePathAsync();
            }
            await Surface.EndBatchAsync();
        }

        public async void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            await Surface.BeginBatchAsync();
            {
                var hex = ColorCache(color);
                if (border)
                {
                    await Surface.SetLineWidthAsync(thickness);
                    await Surface.SetStrokeStyleAsync(ColorCache(RGBA.Black));
                }
                else
                {
                    await Surface.SetStrokeStyleAsync(hex);
                }
                if (fill)
                {
                    await Surface.SetFillStyleAsync(hex);
                    await Surface.FillRectAsync(x, y, width, height);
                }
                else
                {
                    await Surface.StrokeRectAsync(x, y, width, height);
                }
            }
            await Surface.EndBatchAsync();
        }

        public async void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial")
        {
            await Surface.BeginBatchAsync();
            {
                await Surface.SetFillStyleAsync(ColorCache(color));
                await Surface.SetFontAsync(string.Format($"{fontsize}px {fontname}"));
                await Surface.FillTextAsync(text, x, y + fontsize);
            }
            await Surface.EndBatchAsync();
        }

        public async void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5)
        {
            await Surface.BeginBatchAsync();
            {
                var hex = ColorCache(color);
                if (border)
                {
                    await Surface.SetLineWidthAsync(thickness);
                    await Surface.SetStrokeStyleAsync(ColorCache(RGBA.Black));
                }
                else
                {
                    await Surface.SetStrokeStyleAsync(hex);
                }
                await Surface.BeginPathAsync();
                {
                    await Surface.MoveToAsync(x1, y1);
                    await Surface.LineToAsync(x2, y2);
                    await Surface.LineToAsync(x3, y3);
                    await Surface.LineToAsync(x1, y1);
                    if (fill)
                    {
                        await Surface.SetFillStyleAsync(hex);
                        await Surface.FillAsync();
                    }
                    else
                    {
                        await Surface.StrokeAsync();
                    }
                }
                await Surface.ClosePathAsync();
            }
            await Surface.EndBatchAsync();
        }

        //
        // Images
        //
        public IImage CreateImage(int width, int height)
        {
            return new ImageSharpImage(width, height);
        }

        public IImage CreateImage(Stream stream)
        {
            // read the bytes from stream
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return new ImageSharpImage(bytes);
        }

        public IImage CreateImage(string path)
        {
            // load bytes from path
            var bytes = File.ReadAllBytes(path);
            return new ImageSharpImage(bytes);
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            DrawImageCallback(img, x, y, width, height);
        }

        public void Image(IImage img, Common.Point[] points)
        {
            throw new NotImplementedException("Image points");
        }

        //
        // Translation (no-op)
        //
        public void DisableTranslation(TranslationOptions options = TranslationOptions.None) { }
        public void EnableTranslation() { }
        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon = 0) { } 

        #region private
        private Canvas2DContext Surface;
        private Action<IImage, float, float, float, float> DrawImageCallback;
        private Dictionary<int, string> Colors;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ColorCache(RGBA color)
        {
            var hash = color.GetHashCode();
            if (!Colors.TryGetValue(hash, out string hex))
            {
                hex = string.Format($"rgba({color.R},{color.G},{color.B},{(float)color.A / 256f:f1})");
                Colors.Add(hash, hex);
            }
            return hex;
        }
        #endregion
    }
}

using engine.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui
{
    public class MauiImage : engine.Common.IImage
    {
        public MauiImage(int width, int height)
        {
            UnderlyingBitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(UnderlyingBitmap);

            Height = UnderlyingBitmap.Height;
            Width = UnderlyingBitmap.Width;
            Graphics = new MauiGraphics(canvas, Width, Height);
        }

        public MauiImage(Stream stream)
        {
            UnderlyingBitmap = SKBitmap.Decode(stream);
            var canvas = new SKCanvas(UnderlyingBitmap);

            Height = UnderlyingBitmap.Height;
            Width = UnderlyingBitmap.Width;
            Graphics = new MauiGraphics(canvas, Width, Height);
        }

        public MauiImage(string path)
        {
            UnderlyingBitmap = SKBitmap.Decode(path);
            var canvas = new SKCanvas(UnderlyingBitmap);

            Height = UnderlyingBitmap.Height;
            Width = UnderlyingBitmap.Width;
            Graphics = new MauiGraphics(canvas, Width, Height);
        }

        public IGraphics Graphics { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public void MakeTransparent(RGBA color)
        {
            var count = Width * Height;
            var pixels = new SKColor[count];
            var span = new Span<SKColor>(pixels);

            // copy the current set of pixels
            UnderlyingBitmap.Pixels.CopyTo(span);

            var changed = 0L;
            for (int n = 0; n < count; n++)
            {
                if (pixels[n].Red == color.R &&
                    pixels[n].Green == color.G &&
                    pixels[n].Blue == color.B)
                {
                    // mark as transparent
                    pixels[n] = Transparent;
                    changed++;
                }
            }

            // if there was an update, update the pixels
            if (changed > 0)
            {
                UnderlyingBitmap.Pixels = pixels;
            }
        }

        public void Close()
        {
            UnderlyingBitmap.Dispose();
            UnderlyingBitmap = null;
            Graphics = null;
        }

        public void Save(string path)
        {
            throw new NotImplementedException();
        }

        #region internal
        internal SKBitmap UnderlyingBitmap;
        #endregion

        #region private
        private static SKColor Transparent = new SKColor(red: 0, green: 0, blue: 0, alpha: 0);
        #endregion
    }
}

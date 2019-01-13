using engine.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Winforms
{
    public class BitmapImage : IImage
    {
        public BitmapImage(int width, int height)
        {
            Width = width;
            Height = height;

            // create a bitmap and return the WritableGraphics handle
            UnderlyingImage = new Bitmap(width, height);
            var g = System.Drawing.Graphics.FromImage(UnderlyingImage);
            Graphics = new WritableGraphics(null, g, height, width);
        }

        public IGraphics Graphics { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public void MakeTransparent(RGBA color)
        {
            UnderlyingImage.MakeTransparent(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        #region internal
        internal Bitmap UnderlyingImage;
        #endregion
    }
}

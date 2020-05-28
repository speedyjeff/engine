using engine.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace engine.XPlatform
{
    public class ImageSharpImage : Common.IImage
    {
        public ImageSharpImage(int width, int height)
        {
            UnderlyingImage = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
            Width = UnderlyingImage.Width;
            Height = UnderlyingImage.Height;
            Graphics = new ImageSharpGraphics(UnderlyingImage);
        }

        public ImageSharpImage(byte[] bytes)
        {
            UnderlyingImage = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(bytes);
            Width = UnderlyingImage.Width;
            Height = UnderlyingImage.Height;
            Graphics = new ImageSharpGraphics(UnderlyingImage);

            // determine the format of the input
            var mimetype = "image/png";
            foreach (var kvp in Detectors)
            {
                if (kvp.Key.DetectFormat(bytes) != null)
                {
                    mimetype = kvp.Value;
                    break;
                }
            }

            // compute the base64string
            Base64 = $"data:{mimetype};base64,{Convert.ToBase64String(bytes)}";
        }

        public IGraphics Graphics { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public string Base64 { get; private set; }

        public void ComputeBase64()
        {
            // todo - this is slow
            // https://stackoverflow.com/questions/50025908/convert-imagergba32-to-byte-using-imagesharp
            using (MemoryStream ms = new MemoryStream())
            {
                UnderlyingImage.SaveAsPng(ms);
                ms.TryGetBuffer(out ArraySegment<byte> buffer);
                Base64 = string.Format($"data:image/png;base64,{Convert.ToBase64String(buffer.Array, 0, (int)ms.Length)}");
            }
        }
        public string DomId { get; set; }
        public bool HasChanged
        {
            get
            {
                return (Graphics as ImageSharpGraphics).HasChanged;
            }
            set
            {
                (Graphics as ImageSharpGraphics).HasChanged = value;
            }
        }

        public void MakeTransparent(RGBA color)
        {
            throw new NotImplementedException("MakeTransparent not implemented");
        }

        public void Save(string path)
        {
            using (var stream = File.Create(path))
            {
                UnderlyingImage.SaveAsPng(stream);
            }
        }

        #region private
        internal Image<SixLabors.ImageSharp.PixelFormats.Rgba32> UnderlyingImage;
        private static Dictionary<IImageFormatDetector, string> Detectors;

        static ImageSharpImage()
        {
            Detectors = new Dictionary<IImageFormatDetector, string>()
            {
                {new PngImageFormatDetector(), "image/png" },
                {new JpegImageFormatDetector(), "image/jpeg" },
                {new BmpImageFormatDetector(), "image/bmp" },
                {new GifImageFormatDetector(), "image/gif" },
            };
        }
        #endregion
    }
}

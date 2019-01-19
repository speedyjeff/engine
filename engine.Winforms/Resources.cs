using engine.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace engine.Winforms
{
    public static class Resources
    {
        public static Dictionary<string, IImage> LoadImages(Assembly assembly, IGraphics g)
        {
            // load resources
            var resources = engine.Common.Embedded.LoadResource<Bitmap>(assembly);

            // convert to images
            var images = new Dictionary<string, IImage>();
            foreach (var kvp in resources)
            {
                var bitmap = kvp.Value;
                var name = kvp.Key;

                using (var mem = new MemoryStream())
                {
                    // save to stream
                    bitmap.Save(mem, System.Drawing.Imaging.ImageFormat.Bmp);

                    // return to the begining
                    mem.Seek(0, SeekOrigin.Begin);

                    // create an IImage
                    var img = g.CreateImage(bitmap.Width, bitmap.Height);
                    img.Graphics.Image(name, mem, 0, 0, bitmap.Width, bitmap.Height);

                    // add to collection
                    images.Add(name, img);
                } // using
            }

            return images;
        }
    }
}

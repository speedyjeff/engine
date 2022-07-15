using engine.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui
{
    public static class Resources
    {
        public static Dictionary<string, IImage> LoadImages(Assembly assembly)
        {
            // load resources
            var images = new Dictionary<string, IImage>();
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    // load images from stream
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    var img = new MauiImage(stream);
                    // return the name of the asset "folder1.folder2.name.extension"
                    var parts = resourceName.Split('.');
                    var name = parts.Length == 0 ? resourceName :
                        (parts.Length == 1 ? parts[0] : parts[parts.Length - 2]);
                    images.Add(name, img);
                }

            } // foreach

            return images;
        }
    }
}

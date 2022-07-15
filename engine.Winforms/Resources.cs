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
        public static Dictionary<string, IImage> LoadImages(Assembly assembly)
        {
            // load resources
            var images = new Dictionary<string, IImage>();
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                // load from stream
                var stream = assembly.GetManifestResourceStream(resourceName);
                var bitmap = new Bitmap(stream);
                var img = new BitmapImage(bitmap);
                // return the name of the asset "folder1.folder2.name.extension"
                var parts = resourceName.Split('.');
                var name = parts.Length == 0 ? resourceName :
                    (parts.Length == 1 ? parts[0] : parts[parts.Length - 2]);
                images.Add(name, img);

            } // foreach

            return images;
        }

        public static Dictionary<string, string> LoadText(Assembly assembly)
        {
            // load resources
            var resources = engine.Common.Embedded.LoadResource<byte[]>(assembly);

            // convert to images
            var files = new Dictionary<string, string>();
            foreach (var kvp in resources)
            {
                var bytes = kvp.Value;
                var name = kvp.Key;

                files.Add(name, System.Text.UTF8Encoding.UTF8.GetString(bytes));
            }

            return files;
        }
    }
}

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
            var resources = engine.Common.Embedded.LoadResource(assembly);

            // convert to images
            var images = new Dictionary<string, IImage>();
            foreach (var kvp in resources)
            {
                if (!kvp.Key.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !kvp.Key.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) continue;

                // shorten name
                var parts = kvp.Key.Split('.');
                var name = parts.Length == 0 ? kvp.Key :
                    (parts.Length == 1 ? parts[0] : parts[parts.Length - 2]);

                // load images
                var bitmap = new Bitmap(kvp.Value);
                var img = new BitmapImage(bitmap);

                // store
                images.Add(name, img);
            }

            return images;
        }

        public static Dictionary<string, string> LoadText(Assembly assembly)
        {
            // load resources
            var resources = engine.Common.Embedded.LoadResource(assembly);

            // grab text
            var files = new Dictionary<string, string>();
            foreach (var kvp in resources)
            {
                if (!kvp.Key.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) continue;

                // shorten name
                var parts = kvp.Key.Split('.');
                var name = parts.Length == 0 ? kvp.Key :
                    (parts.Length == 1 ? parts[0] : parts[parts.Length - 2]);

                // read bytes
                var bytes = new byte[kvp.Value.Length];
                kvp.Value.Read(bytes, offset: 0, count: bytes.Length);

                // store
                files.Add(name, System.Text.UTF8Encoding.UTF8.GetString(bytes));
            }

            return files;
        }
    }
}

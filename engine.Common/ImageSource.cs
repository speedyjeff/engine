using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public class ImageSource
    {
        public ImageSource(string path)
        {
            Name = path;

            // add this content for later processing
            if (File.Exists(path))
            {
                lock (Images)
                {
                    if (!Content.ContainsKey(Name) && !Images.ContainsKey(Name))
                    {
                        Content.Add(Name, File.ReadAllBytes(path));
                    }
                }
            }
        }

        public ImageSource(string name, byte[] bytes)
        {
            Name = name;
            // add this content for later processing
            lock (Images)
            {
                if (!Content.ContainsKey(Name) && !Images.ContainsKey(Name))
                {
                    Content.Add(Name, bytes);
                }
            }
        }

        public ImageSource(string name, IImage image)
        {
            Name = name;
            _Image = image;

            lock(Images)
            {
                if (!Images.ContainsKey(name)) Images.Add(name, image);
            }
        }

        public string Name { get; private set; }

        public IImage Image
        {
            get
            {
                if (_Image != null) return _Image;
                if (Graphics == null) return null;

                lock (Images)
                {
                    if (!Images.TryGetValue(Name, out _Image))
                    {
                        if (!Content.TryGetValue(Name, out byte[] content)) throw new Exception("No content to load for image : " + Name);

                        // load from bytes
                        using (var mem = new MemoryStream(content, 0, content.Length))
                        {
                            _Image = Graphics.CreateImage(mem);
                        }

                        // store for later
                        Content.Remove(Name);
                        Images.Add(Name, _Image);
                    }
                    return _Image;
                }
            }
        }

        #region private
        private static Dictionary<string, IImage> Images = new Dictionary<string, IImage>();
        private static Dictionary<string, byte[]> Content = new Dictionary<string, byte[]>();

        private static IGraphics Graphics;
        private IImage _Image;

        internal static void SetGraphics(IGraphics g)
        {
            Graphics = g;
        }
        #endregion
    }
}

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
            Path = path;
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
                        _Image = Graphics.CreateImage(Path);
                        Images.Add(Name, _Image);
                    }
                    return _Image;
                }
            }
        }

        #region private
        private static Dictionary<string, IImage> Images = new Dictionary<string, IImage>();

        private static IGraphics Graphics;
        private readonly string Path;
        private IImage _Image;

        internal static void SetGraphics(IGraphics g)
        {
            Graphics = g;
        }
        #endregion
    }
}

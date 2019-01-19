using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public interface IImage
    {
        IGraphics Graphics { get;  }
        int Height { get; }
        int Width { get; }

        void MakeTransparent(RGBA color);
        void Save(string path);
    }
}

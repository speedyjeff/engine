using System;
using System.Collections.Generic;
using System.IO;

namespace engine.Common
{
    static class Media
    {
        static Media()
        {
            Sounds = Embedded.LoadResource<Stream>(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static Dictionary<string, Stream> Sounds { get; private set; }
    }
}

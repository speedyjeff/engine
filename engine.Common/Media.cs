using System;
using System.Collections.Generic;
using System.IO;

namespace engine.Common
{
    static class Media
    {
        static Media()
        {
            var streams = Embedded.LoadResource(System.Reflection.Assembly.GetExecutingAssembly());
            
            // convert the names into the short form
            Sounds = new Dictionary<string, Stream>();
            foreach(var kvp in streams)
            {
                if (!kvp.Key.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) continue;
                var parts = kvp.Key.Split('.');
                var name = parts.Length == 0 ? kvp.Key :
                    (parts.Length == 1 ? parts[0] : parts[parts.Length - 2]);
                Sounds.Add(name, kvp.Value);
            }
        }

        public static Dictionary<string, Stream> Sounds { get; private set; }
    }
}

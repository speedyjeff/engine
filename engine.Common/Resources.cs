using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace engine.Common
{
    public static class Embedded
    {
        public static Dictionary<string, Stream> LoadResource(Assembly assembly)
        {
            var output = new Dictionary<string, Stream>();

            // load all the images out of the embedded resources
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                // load from stream
                var stream = assembly.GetManifestResourceStream(resourceName);
                // return the name of the asset "folder1.folder2.name.extension"
                output.Add(resourceName, stream);
            } // foreach

            return output;
        }
    }
}

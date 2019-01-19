using System;
using System.Collections.Generic;
using System.Reflection;

namespace engine.Common
{
    public static class Embedded
    {
        public static Dictionary<string, T> LoadResource<T>(Assembly assembly)
        {
            var output = new Dictionary<string, T>();

            // load all the images out of the embedded resources
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                var stream = assembly.GetManifestResourceStream(resourceName);
                var resources = new System.Resources.ResourceSet(stream);

                var id = resources.GetEnumerator();
                while (id.MoveNext())
                {
                    if (id.Value is T) output.Add(id.Key as String, (T)id.Value);
                } // while
            } // foreach

            return output;
        }
    }
}

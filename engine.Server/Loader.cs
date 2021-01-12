using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace engine.Server
{
    public static class Loader
    {
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"In AssemblyResolve {args.Name}");
            var name = args.Name.Split(',')[0];
            return LoadLocalAssembly(name);
        }

        public static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            // attempt to parse the namespace and glean the assembly name
            System.Diagnostics.Debug.WriteLine($"In TypeResolve {args.Name}");
            var index = args.Name.LastIndexOf('.');
            var assemblyName = args.Name;
            while (index > 0)
            {
                assemblyName = assemblyName.Substring(0, index);
                var assembly = LoadLocalAssembly(assemblyName);
                if (assembly != null) return assembly;
                // try again
                index = assemblyName.LastIndexOf('.');
            }

            return null;
        }

        #region private
        private static Assembly LoadLocalAssembly(string assemblyName)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "games", assemblyName + ".dll");
            System.Diagnostics.Debug.WriteLine($"Loading {path}");
            if (File.Exists(path)) return Assembly.LoadFrom(path);
            return null;
        }
        #endregion
    }
}

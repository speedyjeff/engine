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
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var name = args.Name.Split(',')[0];
            path = Path.Combine(path, "games", name + ".dll");
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }
            return null;
        }

        public static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"In TypeResolve {args.Name}");
            return null;
        }
    }
}

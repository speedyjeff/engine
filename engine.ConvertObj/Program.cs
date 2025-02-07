using System;
using System.IO;

namespace engine.ConvertObj
{
    class Program
    {
        static int Main(string[] args)
        {
            var namespaceName = "engine.Common.Entities3D";
            var xangle = 0f;
            var yangle = 0f;
            var zangle = 0f;
            var path = "";

            // parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-n", StringComparison.OrdinalIgnoreCase) && (i + 1) < args.Length) namespaceName = args[++i];
                else if (args[i].StartsWith("-x", StringComparison.OrdinalIgnoreCase) && (i + 1) < args.Length) xangle = Convert.ToSingle(args[++i]);
                else if (args[i].StartsWith("-y", StringComparison.OrdinalIgnoreCase) && (i + 1) < args.Length) yangle = Convert.ToSingle(args[++i]);
                else if (args[i].StartsWith("-z", StringComparison.OrdinalIgnoreCase) && (i + 1) < args.Length) zangle = Convert.ToSingle(args[++i]);
                else path = args[i];
            }

            // validate input
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("./engine.ConvertObj <path to .obj file>");
                Console.WriteLine("   -n <namespace>");
                Console.WriteLine("   -x <angle to rotate around the x-axis (pitch)>");
                Console.WriteLine("   -y <angle to rotate around the y-axis (yaw)>");
                Console.WriteLine("   -z <angle to rotate around the z-axis (roll)>");
                Console.WriteLine("Use Blender to create a scene and save in Wavefront obj format (select TriangulatedMesh option)");
                return -1;
            }

            var baseDirectory = Path.GetDirectoryName(path);
            var filename = Path.GetFileNameWithoutExtension(path);

            Console.WriteLine($"Parsing {path} with angles({xangle},{yangle},{zangle})...");

            // parse and write the code to disk
            var count = 0;
            foreach (var obj in Obj.Parse(path))
            {
                var output = Path.Combine(baseDirectory, filename + "_" + (count++) + ".cs");
                Console.WriteLine($"... saving {output}");
                if (obj.Triangles.Count > 1000) Console.WriteLine("*** WARNING *** having too many faces may lead to long or failing initialization of the object (consider reducing the faces)");
                var code = obj.GenerateCode(namespaceName, xangle: xangle, yangle: yangle, zangle: zangle);
                File.WriteAllText(output, code.ToString());
            }

            return 0;
        }
    }
}

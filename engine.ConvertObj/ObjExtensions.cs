using engine.Common;
using engine.Common.Entities3D;
using System;
using System.Collections.Generic;
using System.Text;

namespace engine.ConvertObj
{
    static class ObjExtensions
    {
        private static void Rotate(float xangle, float yangle, float zangle, ref Point point)
        {
            // nomralize
            while (xangle >= 360) xangle -= 360;
            while (yangle >= 360) yangle -= 360;
            while (zangle >= 360) zangle -= 360;
            while (xangle < 0) xangle += 360;
            while (yangle < 0) yangle += 360;
            while (zangle < 0) zangle += 360;

            // iterate through all points and apply 
            // yaw then pitch then roll
            if (yangle != 0) Utilities3D.Yaw(yangle, ref point.X, ref point.Y, ref point.Z);
            if (xangle != 0) Utilities3D.Pitch(xangle, ref point.X, ref point.Y, ref point.Z);
            if (zangle != 0) Utilities3D.Roll(zangle, ref point.X, ref point.Y, ref point.Z);
        }

        public static StringBuilder GenerateCode(this Obj obj, string namespaceName, float scale = 1f, float xangle = 0f, float yangle = 0f, float zangle = 0f)
        {
            // produce source to draw this obj
            var code = new StringBuilder();

            code.AppendLine("using engine.Common;");
            code.AppendLine("using engine.Common.Entities3D;");
            code.AppendFormat($"namespace {namespaceName}");
            code.AppendLine();
            code.AppendLine("{");
            code.AppendFormat($"    public class {obj.Name} : Element3D");
            code.AppendLine();
            code.AppendLine("    {");

            code.AppendFormat($"        public {obj.Name}()");
            code.AppendLine();
            code.AppendLine("        {");
            code.AppendLine("               var multiplier = 1f;");
            code.AppendFormat($"            Width = {obj.Width * scale}f * multiplier;");
            code.AppendLine();
            code.AppendFormat($"            Height = {obj.Height * scale}f * multiplier;");
            code.AppendLine();
            code.AppendFormat($"            Depth = {obj.Depth * scale}f * multiplier;");
            code.AppendLine();

            // check if the colors are all the same
            var uniformColor = obj.Colors[0];
            var theSame = true;
            for(int i=1; i<obj.Colors.Count && theSame; i++)
            {
                if (obj.Colors[i].R != uniformColor.R ||
                    obj.Colors[i].G != uniformColor.G ||
                    obj.Colors[i].B != uniformColor.B ||
                    obj.Colors[i].A != uniformColor.A) theSame = false;
            }

            if (!theSame)
            {
                // set individual colors
                code.AppendLine("            Colors = new RGBA[]");
                code.AppendLine("            {");
                foreach (var color in obj.Colors)
                {
                    code.AppendFormat("                new RGBA() {{ R = {0}, G = {1}, B = {2}, A = {3} }},",
                            color.R,
                            color.G,
                            color.B,
                            color.A);
                    code.AppendLine();
                }
                code.AppendLine("            };");
                code.AppendLine();
            }
            else
            {
                // else just set a uniform color
                code.AppendFormat("            UniformColor = new RGBA() {{ R = {0}, G = {1}, B = {2}, A = {3} }};",
                            uniformColor.R,
                            uniformColor.G,
                            uniformColor.B,
                            uniformColor.A);
                code.AppendLine();
            }
            code.AppendLine("            Polygons = new Point[][]");
            code.AppendLine("            {");
            foreach (var triangle in obj.Triangles)
            {
                var p1 = triangle.P1;
                var p2 = triangle.P2;
                var p3 = triangle.P3;

                Rotate(xangle, yangle, zangle, ref p1);
                Rotate(xangle, yangle, zangle, ref p2);
                Rotate(xangle, yangle, zangle, ref p3);

                code.AppendFormat("                new Point[] {{ new Point() {{ X = {0}f, Y = {1}f, Z = {2}f }}, new Point() {{ X = {3}f, Y = {4}f, Z = {5}f }}, new Point() {{ X = {6}f, Y = {7}f, Z = {8}f }} }},",
                    p1.X * scale,
                    p1.Y * scale,
                    p1.Z * scale,

                    p2.X * scale,
                    p2.Y * scale,
                    p2.Z * scale,

                    p3.X * scale,
                    p3.Y * scale,
                    p3.Z * scale
                    );
                code.AppendLine();
            }
            code.AppendLine("            };");
            code.AppendLine("        }");

            code.AppendLine("    }");
            code.AppendLine("}");

            return code;
        }
    }
}

using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace engine.ConvertObj
{
    class Obj
    {
        public string Name { get; private set; }
        public List<RGBA> Colors { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public float Width { get;  private set; }
        public float Height { get; private set; }
        public float Depth { get; private set; }

        public Obj()
        {
            Triangles = new List<Triangle>();
            Colors = new List<RGBA>();
        }

        public static List<Obj> Parse(string path, bool treatAsScene = false)
        {
            var objects = new List<Obj>();
            Mtl mtl = null;
            var baseDirectory = Path.GetDirectoryName(path);

            // parse
            Dictionary<int, Point> vertices = new Dictionary<int, Point>();
            int index = 1;
            Point min = default(Point);
            Point max = default(Point);
            Obj obj = null;
            RGBA color = default(RGBA);
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                var parts = trimmed.Split(' ');

                if (parts.Length > 1)
                {
                    switch (parts[0])
                    {
                        case "#":
                            // comment
                            break;
                        case "mtllib":
                            var mtlname = parts[1];
                            if (parts.Length > 2)
                            {
                                for (int i = 2; i < parts.Length; i++) mtlname += " " + parts[i];
                            }
                            mtl = Mtl.Parse(Path.Combine(baseDirectory, mtlname));
                            break;
                        case "o":
                            // store the current object
                            if (obj != null) objects.Add(obj);

                            // start of a new object
                            min = new Point() { X = Single.MaxValue, Y = Single.MaxValue, Z = Single.MaxValue };
                            max = new Point() { X = Single.MinValue, Y = Single.MinValue, Z = Single.MinValue };
                            obj = new Obj();

                            // check if there is a name
                            if (parts.Length >= 2) obj.Name = parts[1];
                            break;
                        case "v":
                            if (parts.Length != 4) throw new Exception("Invalid v : " + trimmed);
                            var point = new Point()
                            {
                                X = (float)Convert.ToSingle(parts[1]),
                                Y = (float)Convert.ToSingle(parts[2]),
                                Z = (float)Convert.ToSingle(parts[3])
                            };

                            // set min and max
                            max.X = Math.Max(point.X, max.X);
                            max.Y = Math.Max(point.Y, max.Y);
                            max.Z = Math.Max(point.Z, max.Z);
                            min.X = Math.Min(point.X, min.X);
                            min.Y = Math.Min(point.Y, min.Y);
                            min.Z = Math.Min(point.Z, min.Z);

                            // set width, height, depth
                            obj.Width = (max.X - min.X);
                            obj.Height = (max.Y - min.Y);
                            obj.Depth = (max.Z - min.Z);

                            // save
                            vertices.Add(index++, point);
                            break;
                        case "usemtl":
                            // get the color and set the name
                            if (parts.Length != 2) throw new Exception("Invalid usemtl : " + trimmed);
                            if (mtl == null || !mtl.Sections.TryGetValue(parts[1], out color)) throw new Exception("Missing mtl : " + trimmed);
                            break;
                        case "f":
                            // f 1/1/1 5/2/1 7/3/1
                            // f 1 5 7
                            if (parts.Length != 4) throw new Exception("Invalid v : " + trimmed);

                            // check the further format
                            if (parts[1].Contains("/"))
                            {
                                // f 1/1/1 5/2/1 7/3/1
                                // f 1 5 7
                                parts[1] = parts[1].Split('/')[0];
                                parts[2] = parts[2].Split('/')[0];
                                parts[3] = parts[3].Split('/')[0];
                            }

                            // parse out the vertices index
                            var v1 = Convert.ToInt32(parts[1]);
                            var v2 = Convert.ToInt32(parts[2]);
                            var v3 = Convert.ToInt32(parts[3]);

                            if (!vertices.TryGetValue(v1, out Point p1)) throw new Exception("Invalid vertex index : " + v1);
                            if (!vertices.TryGetValue(v2, out Point p2)) throw new Exception("Invalid vertex index : " + v2);
                            if (!vertices.TryGetValue(v3, out Point p3)) throw new Exception("Invalid vertex index : " + v3);

                            // save
                            obj.Triangles.Add(
                                CreateTriangle(
                                    p1: p1,
                                    p2: p2,
                                    p3: p3,
                                    min: min,
                                    max: max,
                                    width: obj.Width,
                                    height: obj.Height,
                                    depth: obj.Depth,
                                    normalize: (!treatAsScene))
                                );
                            obj.Colors.Add(color);
                            break;
                        case "vn":
                            // ignore normals
                            break;
                        case "vt":
                            // ignore texture coordinates
                            break;
                        case "s":
                            // ignore smoothing
                            break;
                        default: throw new Exception("Unknown file content : " + trimmed);
                    }
                }
            }

            // todo - treatAsScene - only produce 1 object, and do the normalization at the end

            // save the last
            if (obj != null) objects.Add(obj);

            return objects;
        }

        #region private
        private static Triangle CreateTriangle(Point p1, Point p2, Point p3, Point min, Point max, float width, float height, float depth, bool normalize)
        {
            var triangle = new Triangle()
            {
                P1 = p1,
                P2 = p2,
                P3 = p3
            };

            // shift towards the origin
            if (normalize)
            {
                var delta = new Point()
                {
                    X = (0 - max.X) + (width / 2f),
                    Y = (0 - max.Y) + (height / 2f),
                    Z = (0 - max.Z) + (depth / 2f)
                };

                // shift to origin
                triangle.P1.X += delta.X;
                triangle.P2.X += delta.X;
                triangle.P3.X += delta.X;

                triangle.P1.Y += delta.Y;
                triangle.P2.Y += delta.Y;
                triangle.P3.Y += delta.Y;

                triangle.P1.Z += delta.Z;
                triangle.P2.Z += delta.Z;
                triangle.P3.Z += delta.Z;

                // normalize to 0.5...-0.5
                triangle.P1.X /= width;
                triangle.P2.X /= width;
                triangle.P3.X /= width;

                triangle.P1.Y /= height;
                triangle.P2.Y /= height;
                triangle.P3.Y /= height;

                triangle.P1.Z /= depth;
                triangle.P2.Z /= depth;
                triangle.P3.Z /= depth;
            }

            return triangle;
        }
        #endregion
    }
}

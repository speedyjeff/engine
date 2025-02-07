using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace engine.ConvertObj
{
    class Mtl
    {
        public Dictionary<string, RGBA> Sections;

        public Mtl()
        {
            Sections = new Dictionary<string, RGBA>();
        }

        public static Mtl Parse(string path)
        {
            var mtl = new Mtl();

            var name = "";
            foreach(var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                var parts = trimmed.Split(' ');

                if (parts.Length > 1)
                {
                    switch(parts[0])
                    {
                        case "#": 
                            // comment
                            break;
                        case "newmtl":
                            if (parts.Length != 2) throw new Exception("Invalid newmtl : " + trimmed);
                            name = parts[1];
                            break;
                        case "Kd":
                            if (parts.Length != 4) throw new Exception("Invalid Kd : " + trimmed);
                            var rgba = new RGBA()
                                    {
                                        R = (byte)(255 * Convert.ToSingle(parts[1])),
                                        G = (byte)(255 * Convert.ToSingle(parts[2])),
                                        B = (byte)(255 * Convert.ToSingle(parts[3])),
                                        A = 255
                            };

                            // check if we have one to store
                            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Missing name");
                            mtl.Sections.Add(name, rgba);
                            name = "";
                            break;
                        case "illum":
                            // ignore the illumination model
                            break;
                        case "Ks":
                            // ignore the specular color
                            break;
                        case "Ka":
                            // ignore the reflective color
                            break;
                        case "Ns":
                            // ignore the specular exponent
                            break;
                        case "Ke":
                            // ignore the emissive color
                            break;
                        case "d":
                            // ignore the dissolve factor
                            break;
                        case "Ni":
                            // ignore the optical density
                            break;
                        default: throw new Exception("Unknown file content : " + trimmed);
                    }
                }
            }

            return mtl;
        }
    }
}

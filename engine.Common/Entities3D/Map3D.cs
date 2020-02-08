using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    class Map3D : Map
    {
        public Map3D(int width, int height, int depth, Element[] objects, Background background)
        {
            Initialize(width: width, height: height, depth: depth, objects: objects, background: background);
        }

        public override bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            // bounding box test first
            if (base.IsTouching(elem1, elem2, x1delta, y1delta, z1delta))
            {
                // get the 3D object's polygons
                var e1_3D = (elem1 is Element3D) ? elem1 as Element3D : 
                    ((elem1 is Player3D) ? (elem1 as Player3D).Body : null);
                var e2_3D = (elem2 is Element3D) ? elem2 as Element3D :
                    ((elem2 is Player3D) ? (elem2 as Player3D).Body : null);

                // test if both have polygons
                if (e1_3D != null && e2_3D != null)
                {
                    // TODO! - ugh, n^2 algorithm (perhaps sorting?)

                    var p1 = new Point();
                    var q1 = new Point();
                    var r1 = new Point();
                    var p2 = new Point();
                    var q2 = new Point();
                    var r2 = new Point();

                    // ensure to scale and position via X,Y,Z and Width,Height,Depth

                    // check if the polygons in elem1 are touching any of the polygons in elem2
                    for (int i=0; i<e1_3D.Polygons.Length; i++)
                    {
                        if (e1_3D.Polygons[i].Length >= 3)
                        {
                            for (int j = 0; j < e2_3D.Polygons.Length; j++)
                            {
                                if (e2_3D.Polygons[j].Length >= 3)
                                {
                                    p1.X = (e1_3D.Polygons[i][0].X * e1_3D.Width) + e1_3D.X + x1delta; p1.Y = (e1_3D.Polygons[i][0].Y * e1_3D.Height) + e1_3D.Y + y1delta; p1.Z = (e1_3D.Polygons[i][0].Z * e1_3D.Depth) + e1_3D.Z + z1delta;
                                    q1.X = (e1_3D.Polygons[i][1].X * e1_3D.Width) + e1_3D.X + x1delta; q1.Y = (e1_3D.Polygons[i][1].Y * e1_3D.Height) + e1_3D.Y + y1delta; q1.Z = (e1_3D.Polygons[i][1].Z * e1_3D.Depth) + e1_3D.Z + z1delta;
                                    r1.X = (e1_3D.Polygons[i][2].X * e1_3D.Width) + e1_3D.X + x1delta; r1.Y = (e1_3D.Polygons[i][2].Y * e1_3D.Height) + e1_3D.Y + y1delta; r1.Z = (e1_3D.Polygons[i][2].Z * e1_3D.Depth) + e1_3D.Z + z1delta;

                                    p2.X = (e2_3D.Polygons[j][0].X * e2_3D.Width) + e2_3D.X; p2.Y = (e2_3D.Polygons[j][0].Y * e2_3D.Height) + e2_3D.Y; p2.Z = (e2_3D.Polygons[j][0].Z * e2_3D.Depth) + e2_3D.Z;
                                    q2.X = (e2_3D.Polygons[j][1].X * e2_3D.Width) + e2_3D.X; q2.Y = (e2_3D.Polygons[j][1].Y * e2_3D.Height) + e2_3D.Y; q2.Z = (e2_3D.Polygons[j][1].Z * e2_3D.Depth) + e2_3D.Z;
                                    r2.X = (e2_3D.Polygons[j][2].X * e2_3D.Width) + e2_3D.X; r2.Y = (e2_3D.Polygons[j][2].Y * e2_3D.Height) + e2_3D.Y; r2.Z = (e2_3D.Polygons[j][2].Z * e2_3D.Depth) + e2_3D.Z;

                                    // if true, exit early
                                    if (Utilities3D.IntersectingTriangles(p1, q1, r1, p2, q2, r2))
                                        return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                // go with the bounding box decision
                return true;
            }

            // not touching
            return false;
        }
    }
}

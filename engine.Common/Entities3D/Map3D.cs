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
                Point[][] polygons1 = (elem1 is Element3D) ? (elem1 as Element3D).Polygons : 
                    ((elem1 is Player3D) ? (elem1 as Player3D).Body.Polygons : null);
                Point[][] polygons2 = (elem2 is Element3D) ? (elem2 as Element3D).Polygons :
                    ((elem2 is Player3D) ? (elem2 as Player3D).Body.Polygons : null);

                // test if both have polygons
                if (polygons1 != null && polygons2 != null)
                {
                    // TODO! - ugh, n^2 algorithm (perhaps sorting?)

                    // check if the polygons in elem1 are touching any of the polygons in elem2
                    for (int i=0; i<polygons1.Length; i++)
                    {
                        if (polygons1[i].Length >= 3)
                        {
                            for (int j = 0; j < polygons2.Length; j++)
                            {
                                if (polygons2[j].Length >= 3)
                                {
                                    // if true, exit early
                                    if (Utilities3D.IntersectingTriangles(
                                        polygons1[i][0], polygons1[i][1], polygons1[i][2], // triangle elem1
                                        polygons2[j][0], polygons2[j][1], polygons2[j][2]  // triangle elem2
                                        )) 
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

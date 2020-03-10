using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Sheet : Element3D
    {
        // Orientation of Images so they show up in the right orientation:
        //     
        // Points[0] - the Top-Left triangle spanning the top and left edges
        //    Images[0] - 1) generate png which represents the same portion of a square
        //                2) mark the first pixel (width:0,height:0) with a color that should be transparent
        //                3) color the negative space with this color (to avoid incorrect artifacts from showing up)
        //                4) save
        // Points[1] - the Bottom-Right triangle spanning the bottom and right edges
        //    Images[1] - 1) generate a png which represents the same portion of a square
        //                2) mark the first pixel (width:0,height:0) with a color that should be transparent
        //                3) color the negative space with this color (to avoid incorrect artifacts from showing up)
        //                4) before saving do a 'horizontal-flip' and a 90 degree clockwise rotation
        //                5) save

        public Sheet()
        {
            Width = 40;
            Height = 0;
            Depth = 40;
            UniformColor = new RGBA() { R = 33, G = 186, B = 75, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0f, Z = -0.5f }, new Point() { X = -0.5f, Y = 0f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = 0.5f }, new Point() { X = 0.5f, Y = 0f, Z = 0.5f }, new Point() { X = -0.5f, Y = 0f, Z = -0.5f },  },
            };
        }
    }
}

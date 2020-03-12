using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public class Element3D : Element
    {
        // array of Images for each polygon
        public ImageSource[] ImageSources { get; set; }
        // array of colors for each polygon
        public RGBA[] Colors { get; set; }
        // list of polygons
        public Point[][] Polygons { get; set; }
        // do not fill in the faces
        public bool Wireframe { get; set; }
        // shape is a uniform color
        public RGBA UniformColor { get; set; }
        // turn on color shading
        public bool DisableShading { get; set; }

        public Element3D()
        {
            IsSolid = true;
        }

        public override void Draw(IGraphics g)
        {
            // check if shaders should be applied
            if (!DisableShading && !Wireframe && OnShader != null && ShaderLevel != GlobalShaderLevel)
            {
                if (ShadedColors == null) ShadedColors = new RGBA[Polygons.Length];

                // apply shaders to all colors
                for (int i = 0; i < Polygons.Length; i++)
                {
                    var color = IndexToColor(i, applyShaders: false);
                    ShadedColors[i] = OnShader(this, Polygons[i], color);
                }

                // mark as updated
                ShaderLevel = GlobalShaderLevel;
            }

            // display the polygons
            for (int i = 0; i < Polygons.Length; i++)
            {
                // transform
                var color = IndexToColor(i);
                Point[] points = null;
                if (Polygons[i].Length == 3)
                {
                    TriPoints[0].X = (Polygons[i][0].X * Width) + X; TriPoints[0].Y = (Polygons[i][0].Y * Height) + Y; TriPoints[0].Z = (Polygons[i][0].Z * Depth) + Z;
                    TriPoints[1].X = (Polygons[i][1].X * Width) + X; TriPoints[1].Y = (Polygons[i][1].Y * Height) + Y; TriPoints[1].Z = (Polygons[i][1].Z * Depth) + Z;
                    TriPoints[2].X = (Polygons[i][2].X * Width) + X; TriPoints[2].Y = (Polygons[i][2].Y * Height) + Y; TriPoints[2].Z = (Polygons[i][2].Z * Depth) + Z;

                    points = TriPoints;
                }
                else
                {
                    points = new Point[Polygons[i].Length];
                    for (int j = 0; j < Polygons[i].Length; j++) points[j] = new Point() { X = (Polygons[i][j].X * Width) + X, Y = (Polygons[i][j].Y * Height) + Y, Z = (Polygons[i][j].Z * Depth) + Z };
                }

                // draw
                if (ImageSources != null && ImageSources[i] != null) g.Image(ImageSources[i].Image, points);
                else g.Polygon(color, points, fill: !Wireframe, border: false, thickness: 1f);
            }

            base.Draw(g);
        }

        // shaders should be reapplied
        public void ReapplyShaders()
        {
            System.Threading.Interlocked.Increment(ref GlobalShaderLevel);
        }

        // callback to apply appropriate shading
        public static void SetShader(Func<Element3D, Point[], RGBA, RGBA> shader)
        {
            OnShader = shader;
        }

        public void Rotate(float yaw, float pitch, float roll)
        {
            // iterate through all the points and apply the angle
            for(int i=0; i<Polygons.Length; i++)
            {
                for(int j=0; j<Polygons[i].Length; j++)
                {
                    if (yaw != 0) Utilities3D.Yaw(yaw, ref Polygons[i][j].X, ref Polygons[i][j].Y, ref Polygons[i][j].Z);
                    if (pitch != 0) Utilities3D.Pitch(pitch, ref Polygons[i][j].X, ref Polygons[i][j].Y, ref Polygons[i][j].Z);
                    if (roll != 0) Utilities3D.Roll(roll, ref Polygons[i][j].X, ref Polygons[i][j].Y, ref Polygons[i][j].Z);
                }
            }
        }

        #region private
        // global shader support
        private static Func<Element3D, Point[], RGBA, RGBA> OnShader;
        private static volatile int GlobalShaderLevel = 1;
        private static Point[] TriPoints = new Point[3];

        // per Element3D shading
        private volatile int ShaderLevel = 0;
        private RGBA[] ShadedColors;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RGBA IndexToColor(int index, bool applyShaders = true)
        {
            RGBA[] colors = null;

            // If not applying a shader, use one of the two original color sources
            if (!applyShaders || DisableShading)
            {
                if ((UniformColor.R + UniformColor.G + UniformColor.B + UniformColor.A) != 0) return UniformColor;
                colors = Colors;
            }
            else
            {
                colors = ShadedColors;
            }
            // choose an appropriate color
            return (colors == null || colors.Length == 0) ? RGBA.Black : ((index < colors.Length) ? colors[index] : colors[colors.Length - 1]);
        }
        #endregion
    }
}

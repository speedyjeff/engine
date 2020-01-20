using engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public class Element3D : Element
    {
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
                var color = IndexToColor(i);
                var points = new Point[Polygons[i].Length];
                for (int j = 0; j < Polygons[i].Length; j++) points[j] = new Point() { X = (Polygons[i][j].X * Width) + X, Y = (Polygons[i][j].Y *Height) + Y, Z = (Polygons[i][j].Z * Depth) + Z };
                g.Polygon(color, points, fill: !Wireframe, border: false, thickness: 1f);
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

        // todo - rotate the element 

        #region private
        // global shader support
        private static Func<Element3D, Point[], RGBA, RGBA> OnShader;
        private static volatile int GlobalShaderLevel = 1;

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

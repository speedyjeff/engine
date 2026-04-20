using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
        // turn off drawing the polygons
        public bool DisableDrawing { get; set; }

        public Element3D()
        {
            IsSolid = true;
        }

        public override void Draw(IGraphics g)
        {
            // check if we should skip drawing
            if (DisableDrawing || Polygons == null || Polygons.Length == 0)
            {
                base.Draw(g);
                return;
            }

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
                var polygon = Polygons[i];
                var points = GetRenderPoints(i, polygon.Length);
                for (int j = 0; j < polygon.Length; j++)
                {
                    points[j].X = (polygon[j].X * Width) + X;
                    points[j].Y = (polygon[j].Y * Height) + Y;
                    points[j].Z = (polygon[j].Z * Depth) + Z;
                }

                // For textured faces, draw only the image. Rendering the fallback face
                // underneath makes the masked triangle appear vertically offset.
                if (ImageSources != null && i < ImageSources.Length && ImageSources[i] != null && !Wireframe)
                    g.Image(ImageSources[i].Image, points);
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

        // per Element3D shading
        private volatile int ShaderLevel = 0;
        private RGBA[] ShadedColors;
        private Point[][] RenderPointsCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point[] GetRenderPoints(int polygonIndex, int pointCount)
        {
            if (RenderPointsCache == null || RenderPointsCache.Length != Polygons.Length)
            {
                RenderPointsCache = new Point[Polygons.Length][];
            }

            var points = RenderPointsCache[polygonIndex];
            if (points == null || points.Length != pointCount)
            {
                points = new Point[pointCount];
                RenderPointsCache[polygonIndex] = points;
            }

            return points;
        }

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    class BoardTranslationGraphics : IGraphics
    {
        public BoardTranslationGraphics(IGraphics graphics)
        {
            Graphics = graphics;
        }

        public int Height { get; private set; }

        public int Width { get; private set; }
        public float LevelOfDetail { get { return Graphics.LevelOfDetail; } }

        public void Clear(RGBA color)
        {
            // apply a rectangle of this color
            Rectangle(color, StartX, StartY, Width, Height, fill: true, border: false);
        }

        public IImage CreateImage(int width, int height)
        {
            return Graphics.CreateImage(width, height);
        }

        public IImage CreateImage(Stream stream)
        {
            return Graphics.CreateImage(stream);
        }

        public IImage CreateImage(string path)
        {
            return Graphics.CreateImage(path);
        }

        public void DisableTranslation(TranslationOptions options = TranslationOptions.None)
        {
            Graphics.DisableTranslation(options);
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            // translate as if the start is not 0,0
            Graphics.Ellipse(color, x + StartX, y + StartY, width, height, fill, border, thickness);
        }

        public void EnableTranslation()
        {
            Graphics.EnableTranslation();
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            // translate as if the start is not 0,0
            Graphics.Image(img, x + StartX, y + StartY, width, height);
        }

        public void Image(IImage img, Point[] points)
        {
            // translate as if the start is not 0,0
            var tpoints = new Point[points.Length];
            for(int i=0; i<points.Length; i++)
            {
                tpoints[i].X = points[i].X + StartX;
                tpoints[i].Y = points[i].Y + StartY;
                tpoints[i].Z = points[i].Z;
            }
            Graphics.Image(img, tpoints);
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            // translate as if the start is not 0,0
            Graphics.Line(color, x1 + StartX, y1 + StartY, x2 + StartX, y2 + StartY, thickness);
        }

        public void Polygon(RGBA color, Point[] points, bool fill = true, bool border = false, float thickness = 5)
        {
            // translate as if the start is not 0,0
            var tpoints = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                tpoints[i].X = points[i].X + StartX;
                tpoints[i].Y = points[i].Y + StartY;
                tpoints[i].Z = points[i].Z;
            }
            Graphics.Polygon(color, tpoints, fill, border, thickness);
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill = true, bool border = true, float thickness = 5)
        {
            // translate as if the start is not 0,0
            Graphics.Rectangle(color, x + StartX, y + StartY, width, height, fill, border, thickness);
        }

        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon = 0f, float lod = 0f)
        {
            Graphics.SetPerspective(is3D, centerX, centerY, centerZ, yaw, pitch, roll, cameraX, cameraY, cameraZ, horizon, lod);
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16, string fontname = "Arial")
        {
            // translate as if the start is not 0,0
            Graphics.Text(color, x + StartX, y + StartY, text, fontsize, fontname);
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill = true, bool border = false, float thickness = 5)
        {
            // translate as if the start is not 0,0
            Graphics.Triangle(color, x1 + StartX, y1 + StartY, x2 + StartX, y2 + StartY, x3 + StartX, y3 + StartY, fill, border, thickness);
        }

        #region private
        private IGraphics Graphics;
        private float StartX;
        private float StartY;

        internal void SetScoping(float x, float y, int width, int height)
        {
            // scope the graphics to look like it is a subset of the overall screen
            StartX = x;
            StartY = y;
            Width = width;
            Height = height;
        }
        #endregion
    }
}

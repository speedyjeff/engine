﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using engine.Common;
using engine.Common.Entities3D;

namespace engine.Common
{
    class WorldTranslationGraphics : IGraphics
    {
        public WorldTranslationGraphics(IGraphics graphics)
        {
            Graphics = graphics;
        }

        public void Clear(RGBA color)
        {
            Graphics.Clear(color);
        }

        public void Ellipse(RGBA color, float x, float y, float width, float height, bool fill, bool border, float thickness)
        {
            float z = Constants.Ground;
            if (DoTranslation) TranslateCoordinates(Options, ref x, ref y, ref z, ref width, ref height, ref thickness);
            Graphics.Ellipse(color, x, y, width, height, fill, border, thickness);
        }

        public void Rectangle(RGBA color, float x, float y, float width, float height, bool fill, bool border, float thickness)
        {
            float z = Constants.Ground;
            if (DoTranslation) TranslateCoordinates(Options, ref x, ref y, ref z, ref width, ref height, ref thickness);
            Graphics.Rectangle(color, x, y, width, height, fill, border, thickness);
        }

        public void Triangle(RGBA color, float x1, float y1, float x2, float y2, float x3, float y3, bool fill, bool border, float thickness)
        {
            if (DoTranslation)
            {
                // only translate thickness once
                float z = Constants.Ground;
                float o1 = 0f, o2 = 0f, o3 = 0f;
                TranslateCoordinates(Options, ref x1, ref y1, ref z, ref o1, ref o2, ref o3);
                TranslateCoordinates(Options, ref x2, ref y2, ref z, ref o1, ref o2, ref o3);
                TranslateCoordinates(Options, ref x3, ref y3, ref z, ref o1, ref o2, ref thickness);
            }

            Graphics.Triangle(color, x1, y1, x2, y2, x3, y3, fill, border, thickness);
        }

        public void Text(RGBA color, float x, float y, string text, float fontsize = 16)
        {
            float z = Constants.Ground;
            float width = 0f, height = 0f;
            if (DoTranslation) TranslateCoordinates(Options, ref x, ref y, ref z, ref width, ref height, ref fontsize);
            Graphics.Text(color, x, y, text, fontsize);
        }

        public void Line(RGBA color, float x1, float y1, float x2, float y2, float thickness)
        {
            float z = Constants.Ground;
            float width = 0f, height = 0f, o3 = 0f;
            if (DoTranslation)
            {
                // only translate thickness once
                TranslateCoordinates(Options, ref x1, ref y1, ref z, ref width, ref height, ref o3);
                TranslateCoordinates(Options, ref x2, ref y2, ref z, ref width, ref height, ref thickness);
            }
            Graphics.Line(color, x1, y1, x2, y2, thickness);
        }

        public void Image(IImage img, float x, float y, float width = 0, float height = 0)
        {
            float z = Constants.Ground;
            float o3 = 0f;
            if (DoTranslation) TranslateCoordinates(Options, ref x, ref y, ref z, ref width, ref height, ref o3);
            Graphics.Image(img, x, y, width, height);
        }

        public void SetPerspective(bool is3D, float centerX, float centerY, float centerZ, float yaw, float pitch, float roll, float cameraX, float cameraY, float cameraZ, float horizon)
        {
            Is3D = is3D;
            CenterX = centerX;
            CenterY = centerY;
            CenterZ = centerZ;
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            CameraX = cameraX;
            CameraY = cameraY;
            CameraZ = cameraZ;
            Horizon = horizon;
        }

        public void EnableTranslation()
        {
            Options = TranslationOptions.Default;
            DoTranslation = true;
        }

        public void DisableTranslation(TranslationOptions options = 0)
        {
            DoTranslation = (options & TranslationOptions.Translation) != 0;
            Options = options;
        }

        // 3d support

        public void Polygon(RGBA color, Common.Point[] points, bool fill = false, bool border = false, float thickness = 5f)
        {
            if (points == null || points.Length <= 1) throw new Exception("Must provide a valid number of points");

            // translate the points
            float minz = Single.MaxValue;
            float miny = Single.MaxValue;
            float translatedThickness = 0f;
            if (DoTranslation)
            {
                float o1 = 0f, o2 = 0f;
                for (int i = 0; i < points.Length; i++)
                {
                    TranslateCoordinates(Options, ref points[i].X, ref points[i].Y, ref points[i].Z, ref o1, ref o2, ref thickness);

                    // only translate thickness once
                    if (translatedThickness == 0f) translatedThickness = thickness;

                    // track the furthestZ for sorting
                    if (points[i].Z < minz) minz = points[i].Z;
                    if (points[i].Y < miny) miny = points[i].Y;
                }
            }

            // defer rending for later
            if (_CapturePolygons)
            {
                Poloygons.Add(new PolygonDetails()
                {
                    Color = color,
                    Points = points,
                    Fill = fill,
                    Border = border,
                    Thickness = translatedThickness,
                    MinZ = minz,
                    MinY = miny
                });
            }
            // draw the polygon
            else Graphics.Polygon(color, points, fill, border, thickness);
        }

        public void Image(IImage img, Common.Point[] points)
        {
            if (points == null || points.Length <= 1) throw new Exception("Must provide a valid number of points");

            // translate the points
            float minz = Single.MaxValue;
            float miny = Single.MaxValue;
            float other = 0f;
            if (DoTranslation)
            {
                float o1 = 0f, o2 = 0f;
                for (int i = 0; i < points.Length; i++)
                {
                    TranslateCoordinates(Options, ref points[i].X, ref points[i].Y, ref points[i].Z, ref o1, ref o2, ref other);

                    // track the furthestZ for sorting
                    if (points[i].Z < minz) minz = points[i].Z;
                    if (points[i].Y < miny) miny = points[i].Y;
                }
            }

            // defer rending for later
            if (_CapturePolygons)
            {
                Poloygons.Add(new PolygonDetails()
                {
                    Points = points,
                    MinZ = minz,
                    MinY = miny,
                    Image = img
                });
            }
            // draw the polygon
            else Graphics.Image(img, points);
        }

        public void CapturePolygons()
        {
            if (Poloygons == null) Poloygons = new List<PolygonDetails>();
            else Poloygons.Clear();

            // start capturing
            _CapturePolygons = true;
        }

        public void RenderPolygons()
        {
            // stop capturing
            _CapturePolygons = false;

            // sort the Z's from back to front
            Poloygons.Sort(SortPolygonsByZ);

            // draw them directly
            foreach (var polygon in Poloygons)
            {
                // draw the image or polygon
                if (polygon.Image != null) Graphics.Image(polygon.Image, polygon.Points);
                else Graphics.Polygon(polygon.Color, polygon.Points, polygon.Fill, polygon.Border, polygon.Thickness);
            }

            // clear
            Poloygons.Clear();
        }

        public int Height 
        { 
            get { return Graphics.Height;  }
        }
        public int Width 
        { 
            get { return Graphics.Width; }
        }

        public IImage CreateImage(int width, int height)
        {
            return Graphics.CreateImage(width, height);
        }

        public IImage CreateImage(string path)
        {
            return Graphics.CreateImage(path);
        }

        public IImage CreateImage(Stream stream)
        {
            return Graphics.CreateImage(stream);
        }

        #region private
        private IGraphics Graphics;
        private bool DoTranslation;
        private TranslationOptions Options;

        // perspective tracking
        private bool Is3D;
        private float CenterX;
        private float CenterY;
        private float CenterZ;
        private float CameraX;
        private float CameraY;
        private float CameraZ;
        private float Yaw;
        private float Pitch;
        private float Roll;
        private float Horizon;

        // 3D support
        class PolygonDetails
        {
            public RGBA Color;
            public bool Fill;
            public bool Border;
            public Common.Point[] Points;
            public float Thickness;
            public float MinZ;
            public float MinY;
            public IImage Image;
        }
        private bool _CapturePolygons;
        private List<PolygonDetails> Poloygons;
        private static PolygonComparer SortPolygonsByZ = new PolygonComparer();

        class PolygonComparer : IComparer<PolygonDetails>
        {
            public int Compare(PolygonDetails x, PolygonDetails y)
            {
                // sort the z's from back to front
                if (x.MinZ < y.MinZ) return -1;
                else if (y.MinZ < x.MinZ) return 1;
                // when z's are equal, sort by y's
                else if (x.MinY < y.MinY) return -1;
                else if (y.MinY < x.MinY) return 1;
                // equal
                else return 0;
            }
        }

        // translation
        private void TranslateCoordinates(TranslationOptions options, ref float x, ref float y, ref float z, ref float width, ref float height, ref float other)
        {
            if (!Is3D)
            {
                // get pov point
                var pov = CenterZ + CameraZ;

                // determine scaling factor
                var zoom = ((options & TranslationOptions.Scaling) != 0) && pov > 0.001f ? 1f / pov : 1f;

                // scale
                width *= zoom;
                height *= zoom;
                other *= zoom;

                // Surface.Width & Surface.Height are the current windows width & height
                float windowHWidth = Graphics.Width / 2.0f;
                float windowHHeight = Graphics.Height / 2.0f;

                // now translate to the window
                x = ((x - CenterX) * zoom) + windowHWidth;
                y = ((y - CenterY) * zoom) + windowHHeight;
            }

            // if (Is3D)
            else
            {
                // translate to 0,0,0 (origin)
                x -= CenterX;
                y -= CenterY;
                z -= CenterZ;

                // turn first (yaw)
                if ((options & TranslationOptions.RotaionYaw) != 0 && Yaw != 0) Utilities3D.Yaw(Yaw, ref x, ref y, ref z);

                // tilt head (pitch)
                if ((options & TranslationOptions.RotationPitch) != 0 && Pitch != 0) Utilities3D.Pitch(Pitch, ref x, ref y, ref z);

                // todo - rotate (roll)
                // if ((options & TranslationOptions.RotationRoll) != 0 && Roll != 0) Utilities3D.Roll(Roll, ref x, ref y, ref z);

                // apply camera
                z += CameraZ;
                y += CameraY;
                x += CameraX;

                // scale
                var zoom = ((options & TranslationOptions.Scaling) != 0) ? Utilities3D.Perspective(Horizon, ref x, ref y, ref z) : 1;
                width -= (width * zoom);
                height -= (height * zoom);
                other -= (other * zoom);

                // Surface.Width & Surface.Height are the current windows width & height
                float windowHWidth = Graphics.Width / 2.0f;
                float windowHHeight = Graphics.Height / 2.0f;

                // now translate to the window
                x += windowHWidth;
                y += windowHHeight;
            }
        }
        #endregion
    }
}

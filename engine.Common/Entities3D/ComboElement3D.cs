using System;
using System.Collections.Generic;

namespace engine.Common.Entities3D
{
    public class ComboElement3D : Element3D
    {
        public ComboElement3D() : base()
        {
            Elements = new List<Element3D>();
            // set that we want to skip drawing
            DisableDrawing = true;
        }

        public void AddInner(Element3D innerElement)
        {
            // retain the inner elements for drawing
            Elements.Add(innerElement);

            // combine the list of elements by combining all the polygons into a single list
            var polygons = new List<Point[]>();

            // add any of the previously set polygons
            if (Polygons != null)
            {
                foreach (var poly in Polygons)
                {
                    polygons.Add(poly);
                }
            }

            // add the polygons from each of the inner elements
            foreach (var poly in innerElement.Polygons)
            {
                polygons.Add(poly);
            }

            // set the polygons (used for collision detection)
            Polygons = polygons.ToArray();
        }

        public override void Draw(IGraphics g)
        {
            // draw each of the elements individually
            foreach (var elem in Elements)
            {
                elem.Draw(g);
            }
            // call the base draw
            base.Draw(g);
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            // move each of the elements individually
            foreach (var elem in Elements)
            {
                elem.Move(xDelta, yDelta, zDelta);
            }
            // move the combo element
            base.Move(xDelta, yDelta, zDelta);
        }

        #region private
        private List<Element3D> Elements;
        #endregion
    }
}

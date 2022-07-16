using System.Collections.Generic;
using System.Windows.Forms;
using System;
using engine.Common;
using engine.Winforms;
using engine.Common.Entities;

namespace engine.Samples.Winforms
{
    public partial class Shapes : UserControl
    {
        public Shapes(int width, int height)
        {
            Height = height;
            Width = width;
            DoubleBuffered = true;

            // transparent user
            var me = new Player() { ShowDefaultDrawing = false};

            // all the drawing happens in the background as a canvas
            World = new World(
                new WorldConfiguration()
                {
                    Width = width,
                    Height = height,
                    EnableZoom = false,
                    ShowCoordinates = false
                },
                players: new Player[] {me},
                objects: null,
                background: new CanvasBackground()
                );

            UI = new UIHookup(this, World);
        }

        public World World { get; private set; }

        #region private
        private UIHookup UI;
        #endregion
    }
}
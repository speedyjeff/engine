using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using engine.Common;
using engine.Winforms;
using engine.Common.Entities;

namespace engine.Samples.Winforms
{
    public partial class TopDownPlatformer : UserControl
    {
        public TopDownPlatformer()
        {
            InitializeComponent();
            InitializeSurface(Width, Height);
        }

        public TopDownPlatformer(int width, int height)
        {
            InitializeComponent();
            InitializeSurface(width, height);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public World World { get; private set; }

        #region private
        private UIHookup UI;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // pass along to the UI layer
            UI.ProcessCmdKey(keyData);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeSurface(int width, int height)
        {
            Height = height;
            Width = width;
            DoubleBuffered = true;

            var players = new Player[]
            {
                new Player() { Name = "Me", Z = 1 }
            };
            var obstacles = new List<Element>()
            {
                // top
                new Platform()
                {
                    X = 0,
                    Y = -300,
                    Z = float.MaxValue, // to ensure a player can not escape
                    Width = 600,
                    Height = 20,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // left
                new Platform()
                {
                    X = -300,
                    Y = 0,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 20,
                    Height = 600,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // bottom
                new Platform()
                {
                    X = 0,
                    Y = 300,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 600,
                    Height = 20,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // right
                new Platform()
                {
                    X = 300,
                    Y = 0,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 20,
                    Height = 600,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                }
            };
            var background = new Background(width, height)
            {
                GroundColor = new RGBA() { R = 0, G = 255, B = 100, A = 255 }
            };

            World = new World(
                new WorldConfiguration()
                {
                    Width = width,
                    Height = height,
                    EnableZoom = true,
                    ShowCoordinates = false
                },
                players,
                obstacles.ToArray(),
                background
                );

            UI = new UIHookup(this, World);
        }
        #endregion
    }
}

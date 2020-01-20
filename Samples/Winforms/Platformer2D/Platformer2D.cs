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
using engine.Common.Entities;
using engine.Winforms;

namespace engine.Samples.Winforms
{
    public partial class Platformer2D: UserControl
    {
        public Platformer2D()
        {
            InitializeComponent();
            InitializeSurface(Width, Height);
        }

        public Platformer2D(int width, int height)
        {
            InitializeComponent();
            InitializeSurface(width, height);
        }

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
                new Person() { Name = "Me", X = 0, Y = -150 }
            };
            var obstacles = new List<Element>()
            {
                new Platform()
                {
                    X = 0, 
                    Y = 120,
                    Width = 10000,
                    Height = 200,
                    Color = new RGBA() {R = 175, G = 175, B = 175, A = 255}
                }
            };
            var background = new Background(width, height)
            {
                GroundColor = new RGBA() { R = 150, G = 150, B = 255, A = 255 }
            };

            // add some obstacles to the left
            for(int x=-1000; x<=1000; x+=100)
            {
                if (x == 0) continue;

                obstacles.Add(new Platform()
                {
                    X = x,
                    Y = -60,
                    Width = 20,
                    Height = 20,
                    Color = new RGBA() { R = 175, G = 175, B = 175, A = 255 }
                });
            }

            World = new World(
                new WorldConfiguration()
                {
                    Width = width,
                    Height = height,
                    EnableZoom = true,
                    ShowCoordinates = true,
                    ForcesApplied = (int)(Forces.Y)
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

using engine.Common;
using engine.Common.Entities;
using engine.Common.Entities3D;
using engine.Winforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flyover3D
{
    public partial class Flyover3D : Form
    {
        public Flyover3D()
        {
            InitializeComponent();

            this.Name = "Flyover3D";
            this.Text = "Flyover3D";
            this.Width = 1024;
            this.Height = 800;
            // setting a double buffer eliminates the flicker
            this.DoubleBuffered = true;

            // basic green background
            var width = 10000;
            var height = 800;
            var background = new Background(width, height) { GroundColor = new RGBA { R = 255, G = 255, B = 255, A = 255 } };
            // put the player in the middle
            var players = new Player[]
              {
                new Player3D() { Name = "YoBro", X = 0, Y = 0, Z = 1000 }
              };
            // any objects to interact with
            Element[] objects = new Element[100];
            var rand = new Random();
            for (int i = 0; i < objects.Length; i++)
            {

                // x = [-500 ... 600] (100 gap)
                // z = [0 ... 600] (100 gap)

                var x = ((i % 10) * 200) - 700;
                var z = (i / 10) * 200;

                switch (rand.Next() % 9)
                {
                    case 0: objects[i] = new Cube() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 1: objects[i] = new Cone() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 2: objects[i] = new Cylinder() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 3: objects[i] = new Hexagon() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 4: objects[i] = new Pyramid() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 5: objects[i] = new Sphere() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 6: objects[i] = new Torus() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 7: objects[i] = new Tree() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                    case 8: objects[i] = new Wedge() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                }
            }
            var world = new World(
              new WorldConfiguration()
              {
                  Width = width,
                  Height = height,
                  ShowCoordinates = true,
                  HorizonX = 1000,
                  HorizonY = 1000,
                  HorizonZ = 1000,
                  Is3D = true
              },
              players,
              objects,
              background
            );
            // start the UI painting
            UI = new UIHookup(this, world);

            // hide the cursor
            System.Windows.Forms.Cursor.Hide();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            UI.ProcessCmdKey(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        } // ProcessCmdKey

        #region private
        private UIHookup UI;
        #endregion
    }
}

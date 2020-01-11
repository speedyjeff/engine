using engine.Common;
using engine.Common.Entities;
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
                new FirstPerson() { Name = "YoBro", X = 0, Y = 0, Z = 1000 }
              };
            // any objects to interact with
            Element[] objects = new Element[100];
            for (int i = 0; i < objects.Length; i++)
            {

                // x = [-500 ... 600] (100 gap)
                // z = [0 ... 600] (100 gap)

                var x = ((i % 10) * 200) - 700;
                var z = (i / 10) * 200;

                objects[i] = new Cube() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100 };
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

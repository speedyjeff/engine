using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace engine.Samples.Winforms
{
    public partial class Samples : Form
    {
        public Samples()
        {
            InitializeComponent();

            Height = 800;
            Width = 450;

            var table = new TableLayoutPanel();
            table.RowCount = 7;
            table.ColumnCount = 2;
            table.Width = Width;
            table.Height = Height;
            Controls.Add(table);

            var checkersTitle = new Label() { Text = "Checkers" };
            table.Controls.Add(checkersTitle, column: 0, row: 0);
            var checkers = new Checkers(200, 200);
            table.Controls.Add(checkers, column: 0, row: 1);

            var catanTitle = new Label() { Text = "Catan" };
            table.Controls.Add(catanTitle, column: 1, row: 0);
            var catan = new Catan(200, 200);
            table.Controls.Add(catan, column: 1, row: 1);

            var platTitle = new Label() { Text = "2D Platformer" };
            table.Controls.Add(platTitle, column: 0, row: 2);
            var platform2d = new Platformer2D(200, 200);
            table.Controls.Add(platform2d, column: 0, row: 3);

            var tdTitle = new Label() { Text = "Top Down Platformer" };
            table.Controls.Add(tdTitle, column: 1, row: 2);
            var tdplatform = new TopDownPlatformer(200, 200);
            table.Controls.Add(tdplatform, column: 1, row: 3);

            var f3Title = new Label() { Text = "3D Flyover" };
            table.Controls.Add(f3Title, column: 0, row: 4);
            var f3platform = new Flyover3D(200, 200);
            table.Controls.Add(f3platform, column: 0, row: 5);

            var sTitle = new Label() { Text = "Shapes" };
            table.Controls.Add(sTitle, column: 1, row: 4);
            var splatform = new Shapes(200, 200);
            table.Controls.Add(splatform, column: 1, row: 5);
        }
    }
}

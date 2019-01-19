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

            Height = 520;
            Width = 420;

            var table = new TableLayoutPanel();
            table.RowCount = 4;
            table.ColumnCount = 2;
            table.Width = 400;
            table.Height = 500;
            Controls.Add(table);

            var checkersTitle = new Label() { Text = "Checkers" };
            table.Controls.Add(checkersTitle, 0, 0);
            var checkers = new Checkers(200, 200);
            table.Controls.Add(checkers,0,1);

            var catanTitle = new Label() { Text = "Catan" };
            table.Controls.Add(catanTitle, 1, 0);
            var catan = new Catan(200, 200);
            table.Controls.Add(catan, 1, 1);

            var platTitle = new Label() { Text = "2D Platformer" };
            table.Controls.Add(platTitle, 0, 2);
            var platform2d = new Platformer2D(200, 200);
            table.Controls.Add(platform2d, 0, 3);

            var tdTitle = new Label() { Text = "Top Down Platformer" };
            table.Controls.Add(tdTitle, 1, 2);
            var tdplatform = new TopDownPlatformer(200, 200);
            table.Controls.Add(tdplatform, 1, 3);
        }
    }
}

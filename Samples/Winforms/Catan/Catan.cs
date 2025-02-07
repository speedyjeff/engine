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

namespace engine.Samples.Winforms
{
    public partial class Catan: UserControl
    {
        public Catan()
        {
            InitializeComponent();
            InitalizeBoard(Width, Height);
        }

        public Catan(int width, int height)
        {
            InitializeComponent();
            InitalizeBoard(width, height);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Board Board { get; private set; }

        #region private
        class Coord
        {
            public int Row;
            public int Col;
        }

        enum Resources { Grain, Wool, Rock, Gold, Barren, Nothing };
        private UIHookup UI;
        private Resources[][] Cells;
        private Dictionary<string, IImage> Images;
        private Coord Previous;

        private void Board_OnResize()
        {
            // set initial board pieces
            for (int row = 0; row < Board.Rows; row++)
            {
                for (int col = 0; col < Board.Columns; col++)
                {
                    if (Cells[row][col] == Resources.Nothing) continue;

                    Board.UpdateCell(row, col, (img) =>
                    {
                        DrawHexagon(row, col, img);
                    });
                }
            }
        }

        private void InitalizeBoard(int width, int height)
        {
            // initialize user control
            Height = height;
            Width = width;
            DoubleBuffered = true;

            // initialize board
            Board = new Board(
                new BoardConfiguration()
                {
                    Height = height,
                    Width = width,
                    Rows = 5,
                    Columns = 5,
                    EdgeAngle = 30, // hexagon
                    Background = RGBA.Black
                }
                );
            Board.OnCellClicked += Board_OnCellClicked;
            Board.OnCellOver += Board_OnCellOver;
            Board.OnResize += Board_OnResize;

            // initialize UI handlers
            UI = new UIHookup(this, Board);

            // configure the default cells
            var rand = new Random();
            Cells = new Resources[5][];
            for (int row = 0; row < Cells.Length; row++)
            {
                Cells[row] = new Resources[5];
                for (int col = 0; col < Cells[row].Length; col++)
                {
                    Cells[row][col] = (Resources)(rand.Next() % (int)Resources.Nothing);
                }
            }
            Cells[0][0] = Cells[0][4] = Resources.Nothing;
            Cells[1][4] = Resources.Nothing;
            Cells[3][4] = Resources.Nothing;
            Cells[4][0] = Cells[4][4] = Resources.Nothing;

            // load embedded resources
            Images = engine.Winforms.Resources.LoadImages(System.Reflection.Assembly.GetExecutingAssembly());

            // set initial board pieces
            Board_OnResize();
        }

        private void Board_OnCellOver(int row, int col, float x, float y)
        {
            if (Cells[row][col] == Resources.Nothing) return;

            // check if we have already highlighted a cell
            if (Previous != null)
            {
                // check if it is the same cell
                if (Previous.Row == row && Previous.Col == col) return;

                // else unhighlight the other cell
                Board.UpdateCell(Previous.Row, Previous.Col, (img) =>
                {
                    DrawHexagon(Previous.Row, Previous.Col, img);
                });

                Previous = null;
            }

            // highlight this cell
            Board.UpdateCell(row, col, (img) =>
            {
                img.Graphics.Rectangle(new RGBA() { R = 255, G = 255, B = 0, A = 200 }, 0, 0, img.Width, img.Height, true);
            });

            // set this as the previou
            Previous = new Coord() { Row = row, Col = col };
        }

        private void Board_OnCellClicked(int row, int col, float x, float y)
        {
            // add game logic
        }

        private void DrawHexagon(int row, int col, IImage img)
        {
            switch (Cells[row][col])
            {
                case Resources.Barren:
                    //img.Graphics.Clear(RGBA.Black);
                    img.Graphics.Image(Images["barren"], 0, 0, img.Width, img.Height);
                    break;
                case Resources.Gold:
                    //img.Graphics.Clear(RGBA.White);
                    img.Graphics.Image(Images["gold"], 0, 0, img.Width, img.Height);
                    break;
                case Resources.Grain:
                    //img.Graphics.Clear(new RGBA() { R = 255, G = 255, A = 255 });
                    img.Graphics.Image(Images["wheat"], 0, 0, img.Width, img.Height);
                    break;
                case Resources.Rock:
                    //img.Graphics.Clear(new RGBA() { R = 175, G = 175, B = 175, A = 255 });
                    img.Graphics.Image(Images["rock"], 0, 0, img.Width, img.Height);
                    break;
                case Resources.Wool:
                    //img.Graphics.Clear(new RGBA() { G = 255, A = 255 });
                    img.Graphics.Image(Images["wool"], 0, 0, img.Width, img.Height);
                    break;
                default:
                    throw new Exception("Unknown Resource type : " + Cells[row][col]);
            }
        }
        #endregion
    }
}

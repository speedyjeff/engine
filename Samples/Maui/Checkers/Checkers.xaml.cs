using engine.Common;
using engine.Maui;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace Checkers;

public partial class Checkers : ContentView
{
    public Checkers()
    {
        InitializeComponent();
    }

    public Board Board { get; private set; }


    public void InitalizeBoard(int width, int height)
    {
        // initialize board
        Board = new Board(
            new BoardConfiguration()
            {
                Height = height,
                Width = width,
                Rows = 8,
                Columns = 8,
                EdgeAngle = 0, // rectangles
                Background = RGBA.Black
            }
            );
        Board.OnCellClicked += Board_OnCellClicked;
        Board.OnCellOver += Board_OnCellOver;
        Board.OnResize += Board_OnResize;

        // initialize UI handlers
        UI = new UIHookup(this, Board);

        // load embedded resources
        Images = engine.Maui.Resources.LoadImages(System.Reflection.Assembly.GetExecutingAssembly());

        // make the background transparent
        Images["black"].MakeTransparent(RGBA.White);
        Images["red"].MakeTransparent(RGBA.White);

        // set initial board pieces
        Board_OnResize();
    }

    #region private
    class Coord
    {
        public int Row;
        public int Col;
    }

    private UIHookup UI;
    private Coord Previous;
    private Dictionary<string, IImage> Images;

    private void Board_OnResize()
    {
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Columns; col++)
            {
                Board.UpdateCell(row, col, (img) =>
                {
                    DrawCell(row, col, img);
                });
            }
        }
    }

    private void DrawCell(int row, int col, IImage img)
    {
        bool isActive = false;
        if ((row % 2 == 0 && col % 2 == 0)
            ||
            (row % 2 != 0 && col % 2 != 0))
        {
            isActive = true;
        }

        if (isActive)
        {
            var number = (row * (Board.Columns / 2)) + (col / 2);

            // add background
            img.Graphics.Image(Images["active"], 0, 0, img.Width, img.Height);

            // add a checker
            if (row <= 2) img.Graphics.Image(Images["black"], 0, 0, img.Width, img.Height);
            else if (row >= 5) img.Graphics.Image(Images["red"], 0, 0, img.Width, img.Height);

            // add the numbering
            img.Graphics.Text(RGBA.Black, 2, 2, number.ToString(), 8);
        }
        else
        {
            // add background
            img.Graphics.Image(Images["inactive"], 0, 0, img.Width, img.Height);
        }
    }


    private void Board_OnCellOver(int row, int col, float x, float y)
    {
        // check if we have already highlighted a cell
        if (Previous != null)
        {
            // check if it is the same cell
            if (Previous.Row == row && Previous.Col == col) return;

            // else unhighlight the other cell
            Board.UpdateCell(Previous.Row, Previous.Col, (img) =>
            {
                DrawCell(Previous.Row, Previous.Col, img);
            });

            Previous = null;
        }

        // highlight this cell
        Board.UpdateCell(row, col, (img) =>
        {
            img.Graphics.Rectangle(new RGBA() { R = 255, G = 255, B = 0, A = 100 }, 0, 0, img.Width, img.Height, true);
        });

        // set this as the previou
        Previous = new Coord() { Row = row, Col = col };
    }

    private void Board_OnCellClicked(int row, int col, float x, float y)
    {
        // insert game logic
    }
    #endregion
}
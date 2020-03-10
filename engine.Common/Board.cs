using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public struct BoardConfiguration
    {
        public int Width;
        public int Height;
        public RGBA Background;
        public string BackgroundImage;
        public int Rows;
        public int Columns;
        public int EdgeAngle; // 0 = rectangle, 1..89 = hexagon
    }

    public delegate void CellDelegate(int row, int col, float x, float y);
    public delegate void UpdateImageDelegate(IImage img);

    public class Board : IUserInteraction
    {
        public Board(BoardConfiguration config)
        {
            // config validation
            if (config.Width < 0 || config.Height < 0
                || config.Rows == 0 || config.Columns == 0
                || config.Width < config.Columns || config.Height < config.Rows) throw new Exception("Invalid Board dimensions");

            // ensure the angle is not too steep
            if (config.EdgeAngle < 0 || config.EdgeAngle > 89) throw new Exception("Invalid Edge Angle - must be between 0 and 89");

            // setup background image
            if (!string.IsNullOrWhiteSpace(config.BackgroundImage))
            {
                BackgroundImage = new ImageSource(path: config.BackgroundImage);
            }

            // init
            Config = config;
            Overlay = new CellDetails() { IsDirty = false, Image = null };
            IsDirty = false;
            if (Config.EdgeAngle == 0)
            {
                // rectangle
                CellWidth = config.Width / config.Columns;
                CellHeight = config.Height / config.Rows;
            }
            else
            {
                // hexagon

                // the angle starts at the mid point of the width
                // calculate how far down the height the edge will go
                // tan(angle) = opposite / adjacent
                EdgeWidth = ((Config.Width / Config.Columns) / 2);
                EdgeHeight = (int)Math.Floor(Math.Tan((Math.PI / 180) * Config.EdgeAngle) * EdgeWidth);

                // add later
                if ((2 * EdgeHeight * Config.Rows) > Config.Height) throw new Exception("The EdgeAngle is too steep");

                // need to adjust to an additional EdgeWidth and EdgeHeight (given the shift)
                CellWidth = ((config.Width - EdgeWidth) / config.Columns);
                CellHeight = ((config.Height - EdgeHeight + (Config.Rows * EdgeHeight) ) / config.Rows);

                // update EdgeWidth
                EdgeWidth = CellWidth / 2;
            }

            // create the cells
            Cells = new CellDetails[config.Height][];
            for(int row = 0; row < Cells.Length; row++)
            {
                Cells[row] = new CellDetails[config.Width];
                for(int col = 0; col < Cells[row].Length; col++)
                {
                    Cells[row][col] = new CellDetails()
                    {
                        IsDirty = false,
                        Image = null
                    };
                }
            }
        }

        public int Width { get { return Config.Width; } }
        public int Height { get { return Config.Height; } }

        public int CellWidth { get; private set; }
        public int CellHeight { get; private set; }

        public int Rows { get { return Config.Rows; } }
        public int Columns { get { return Config.Columns; } }

        public RGBA BackgroundColor { get { return Config.Background; } }
        public ImageSource BackgroundImage { get; private set; }

        public event CellDelegate OnCellClicked;
        public event CellDelegate OnCellOver;

        public void InitializeGraphics(IGraphics surface, ISounds sounds)
        {
            Surface = surface;
            Sounds = sounds;

            // set the background color
            Clear();
        }

        // this function will save an image of the cell
        // to enable a template for images
        public void SaveCellTemplate(string path)
        {
            var img = Surface.CreateImage(CellWidth, CellHeight);
            img.Graphics.Clear(RGBA.Black);
            MarkEdgesTransparent(img);
            img.Save(path);
        }

        // 
        // User input
        //
        public void KeyPress(char key)
        {
        }

        public void Mousemove(float x, float y, float angle)
        {
            // provide details of what and were was clicked
            if (OnCellOver != null)
            {
                // translate the x,y to row,col 
                Translate(x, y, out int row, out int col);
                // then get local x,y
                Translate(x, y, row, col, out float lx, out float ly);

                OnCellOver(row, col, lx, ly);
            }
        }

        public void Mousewheel(float delta)
        {
        }

        public void Mousedown(MouseButton btn, float x, float y)
        {
            // provide details of what and were was clicked
            if (OnCellClicked != null)
            {
                // translate the x,y to row,col 
                Translate(x, y, out int row, out int col);
                // then get local x,y
                Translate(x, y, row, col, out float lx, out float ly);

                OnCellClicked(row, col, lx, ly);
            }
        }

        public void Mouseup(MouseButton btn, float x, float y)
        {
            // ignore
        }

        //
        // Infastructure
        // 
        public void Paint()
        {
            // check if the input is dirty
            if (IsDirty)
            {
                IsDirty = false;

                lock (Cells)
                {
                    // iterate through and update the Surface for the dirty cells
                    for (int row = 0; row < Cells.Length; row++)
                    {
                        for (int col = 0; col < Cells[row].Length; col++)
                        {
                            if (Cells[row][col].Image == null) continue;

                            if (Cells[row][col].IsDirty || Overlay.IsDirty)
                            {
                                Cells[row][col].IsDirty = false;

                                // translate to x,y
                                Translate(row, col, out float x, out float y);

                                Surface.Image(Cells[row][col].Image, x, y, CellWidth, CellHeight);
                            } // if (Cells.IsDirty)
                        } // for
                    } // for
                } // lock(Cells)

                // mark the overlay as no longer dirty
                Overlay.IsDirty = false;

                // update the overlay (everytime)
                if (Overlay.Image != null)
                {
                    Surface.Image(Overlay.Image, 0, 0, Surface.Width, Surface.Height);
                }
            } // if (IsDirty)
        }

        public void Resize()
        {
            // invalidate everything so it is painted
            lock (Cells)
            {
                IsDirty = true;
                Clear();
                for (int row = 0; row < Cells.Length; row++)
                {
                    for (int col = 0; col < Cells[row].Length; col++)
                    {
                        if (Cells[row][col].Image != null) Cells[row][col].IsDirty = true;
                    }
                }
                if (Overlay.Image != null) Overlay.IsDirty = true;
            }
        }

        // 
        // surface area
        //

        public void UpdateCell(int row, int col, UpdateImageDelegate update)
        {
            if (row < 0 || row > Rows
                || col < 0 || col > Columns) throw new Exception("Invalid row x col : " + row + "," + col);
            if (update == null) throw new Exception("Must pass in a valid delegate to use for updates");

            lock (Cells)
            {
                if (Cells[row][col].Image == null)
                {
                    // initialize
                    Cells[row][col].Image = Surface.CreateImage(CellWidth, CellHeight);
                }

                // pass to the user for an update
                update(Cells[row][col].Image);

                // make necessary translations
                if (Config.EdgeAngle > 0)
                {
                    MarkEdgesTransparent(Cells[row][col].Image);
                }

                // mark as dirty
                Cells[row][col].IsDirty = true;
                IsDirty = true;
            }
        }

        public void UpdateOverlay(UpdateImageDelegate update)
        {
            if (Overlay.Image == null)
            {
                // initialize
                Overlay.Image = Surface.CreateImage(Surface.Width, Surface.Height);
            }

            // pass to the user to update
            update(Overlay.Image);

            // mark as dirty
            Overlay.IsDirty = true;
            IsDirty = true;
        }

        #region private
        private IGraphics Surface;
        private ISounds Sounds;
        private BoardConfiguration Config;
        private CellDetails Overlay;

        // used for non-rectangluar shapes
        private int EdgeWidth;
        private int EdgeHeight;

        private bool IsDirty = true;
        private struct CellDetails
        {
            public IImage Image;
            public bool IsDirty;
        }
        private CellDetails[][] Cells;

        private void Clear()
        {
            Surface.Clear(Config.Background);
            if (BackgroundImage != null)
            {
                Surface.Image(BackgroundImage.Image, 0, 0, Surface.Width, Surface.Height);
            }
        }

        private void MarkEdgesTransparent(IImage img)
        {
            // make edges transparent
            var clear = new RGBA() { R = 0x12, G = 0x34, B = 0x56, A = 255 };

            // upper left
            img.Graphics.Triangle(clear, 0, 0, EdgeWidth, 0, 0, EdgeHeight);
            // upper right
            img.Graphics.Triangle(clear, EdgeWidth, 0, CellWidth, 0, CellWidth, EdgeHeight);
            // lower left
            img.Graphics.Triangle(clear, 0, CellHeight - EdgeHeight, 0, CellHeight, EdgeWidth, CellHeight);
            // lower right
            img.Graphics.Triangle(clear, EdgeWidth, CellHeight, CellWidth, CellHeight, CellWidth, CellHeight - EdgeHeight);
            // make the edges transparent
            img.MakeTransparent(clear);
        }

        // row,col to screen x,y
        private void Translate(int row, int col, out float x, out float y)
        {
            if (col < 0) col = 0;
            if (col >= Columns) col = Columns - 1;
            if (row < 0) row = 0;
            if (row >= Rows) row = Rows - 1;

            x = col * CellWidth;
            y = row * CellHeight;

            if (Config.EdgeAngle != 0)
            {
                if (row % 2 != 0)
                {
                    // odd
                    x += EdgeWidth;
                }
                y -= (EdgeHeight * row);
            }

        }

        // screen x,y into row,col
        private void Translate(float x, float y, out int row, out int col)
        {
            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            if (y < 0) y = 0;
            if (y >= Height) y = Height - 1;

            if (Config.EdgeAngle == 0)
            {
                col = (int)Math.Floor(x / (float)CellWidth);
                row = (int)Math.Floor(y / (float)CellHeight);
            }
            else
            {
                // hexagon

                // get row (could be either row or row+1)
                if (y < EdgeHeight) row = 0;
                else
                {
                    row = (int)Math.Floor(y - EdgeHeight) / (CellHeight - EdgeHeight);
                }

                // get column (could be either col or col+1)
                if (x < EdgeWidth) col = 0;
                else if (row % 2 == 0)
                {
                    col = (int)Math.Floor(x / (float)CellWidth);
                }
                else
                {
                    col = (int)Math.Floor((x - EdgeWidth) / (float)CellWidth);
                }

                // get the local coordinates 
                Translate(x, y, row, col, out float lx, out float ly);

                // we are in the lower hexagon portion of the shape
                // compare this point with the two hypotenuse' to see
                //  where in the hexagon this point is
                //  
                //   | 0  |
                //   \    /
                //   1\  /2
                //     \/
                // Case 0: row, col
                // Case 1: row++, col-- IFF (row % 2 == 0)
                // Case 2: row++, col++ IFF (row % 2 != 0)

                if (!Collision.IntersectingLine(
                    0, CellHeight - EdgeHeight, EdgeWidth, CellHeight,  // left hypotenuse
                    lx, ly,
                    0, CellHeight // left lower corner
                    ))
                {
                    // case 1
                    if (row % 2 == 0) col--;
                    row++;
                }
                else if (!Collision.IntersectingLine(
                    CellWidth, CellHeight - EdgeHeight, EdgeWidth, CellHeight,  // right hypotenuse
                    lx, ly,
                    CellWidth, CellHeight // left lower corner
                    ))
                {
                    // case 2
                    if (row % 2 != 0) col++;
                    row++;
                }

                if (row < 0) row = 0;
                if (row >= Config.Rows) row = Config.Rows - 1;
                if (col < 0) col = 0;
                if (col >= Config.Columns) col = Config.Columns - 1;
            } // if (rectangle)
        }

        // screen x,y to cell local x,y
        private void Translate(float x, float y, int row, int col, out float lx, out float ly)
        {
            // get the starting point of this cell
            Translate(row, col, out float cx, out float cy);

            // substract the difference to get the local x,y
            lx = x - cx;
            ly = y - cy;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        public BoardCell[][] Cells; // all cells are unique shapes (EdgeAngle does not apply)
    }

    public delegate void CellDelegate(int row, int col, float x, float y);
    public delegate void UpdateImageDelegate(IImage img);
    public delegate bool CellPaintDelegate(IGraphics g, int row, int col);
    public delegate bool OverlayPaintDelegate(IGraphics g);

    public class Board : IUserInteraction
    {
        public Board(BoardConfiguration config)
        {
            // config validation
            if (config.Width <= 0 || config.Height <= 0) throw new Exception("Invalid Board dimensions");

            // ensure the angle is not too steep
            if (config.EdgeAngle < 0 || config.EdgeAngle > 89) throw new Exception("Invalid Edge Angle - must be between 0 and 89");

            // setup background image
            if (!string.IsNullOrWhiteSpace(config.BackgroundImage))
            {
                BackgroundImage = new ImageSource(path: config.BackgroundImage);
            }

            // init
            Height = config.Height;
            Width = config.Width;
            OverlayLock = new object();
            Config = config;
            Overlay = new CellDetails() { IsDirty = false, Image = null };

            // create cells

            // unique cell shapes
            if (Config.Cells != null && Config.Cells.Length > 0)
            {
                // init
                CellWidth = CellHeight = 0;

                // calculate the rows and columns
                Config.Rows = Config.Cells.Length;
                Config.Columns = 0;
                for(int row=0; row<Config.Cells.Length; row++)
                {
                    if (Config.Cells[row].Length > Config.Columns) Config.Columns = Config.Cells[row].Length;
                }
                // moving from index to length
                Config.Columns++;

                // add all the points
                Cells = new CellDetails[Config.Rows][];
                for (int row = 0; row < Config.Cells.Length; row++)
                {
                    Cells[row] = new CellDetails[Config.Cells[row].Length];

                    for (int col = 0; col < Config.Cells[row].Length; col++)
                    {
                        // create cell
                        Cells[row][col] = new CellDetails()
                        {
                            IsDirty = false,
                            Image = null,
                            Width = (int)Math.Ceiling(Config.Cells[row][col].Width),
                            Height = (int)Math.Ceiling(Config.Cells[row][col].Height),
                            Cells = Config.Cells[row][col].Points,
                            Top = Config.Cells[row][col].Top,
                            Left = Config.Cells[row][col].Left
                        };
                    }
                }
            }

            // regular shaped
            else
            {
                if (Config.Rows == 0 || Config.Columns == 0 || Config.Width < Config.Columns || Config.Height < Config.Rows) throw new Exception("Invalid Board dimensions");

                // sets CellWidth/CellHeight and EdgeWidth/EdgeHeight
                InitializeCellBoundaries(Width, Height);
            } // if regular

            // setup the background update timer
            TickTimer = new Timer(TickUpdate, null, 0, Constants.GlobalClock);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int CellWidth { get; private set; }
        public int CellHeight { get; private set; }

        public int Rows { get { return Config.Rows; } }
        public int Columns { get { return Config.Columns; } }

        public RGBA BackgroundColor { get { return Config.Background; } }
        public ImageSource BackgroundImage { get; private set; }

        public event CellDelegate OnCellClicked;
        public event CellDelegate OnCellOver;
        public event Action<char> OnKeyPressed;
        public event Action OnTick;
        public event Action OnResize;
        public event CellPaintDelegate OnCellPaint;
        public event OverlayPaintDelegate OnOverlayPaint;

        public void InitializeGraphics(IGraphics surface, ISounds sounds)
        {
            Surface = surface;
            Sounds = sounds;
            Height = Surface.Height;
            Width = Surface.Width;

            // init imagesource
            ImageSource.SetGraphics(Surface);

            // create the translation graphics
            SubsetOfBoardGraphics = new BoardTranslationGraphics(Surface);

            // set the background color
            Clear();
        }

        // this function will save an image of the cell
        // to enable a template for images
        public void SaveCellTemplate(string path)
        {
            if (CellWidth <= 0 || CellHeight <= 0) return;
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
            // fixup some of the special keys
            if (key == Constants.Space) key = ' ';
            if (OnKeyPressed != null) OnKeyPressed(key);
        }

        public void Mousemove(float x, float y, float angle)
        {
            // provide details of what and were was clicked
            if (OnCellOver != null)
            {
                // translate the x,y to row,col 
                if (!Translate(x, y, out int row, out int col)) return;
                // then get local x,y
                if (!Translate(x, y, row, col, out float lx, out float ly)) throw new Exception("failed to get local x,y");

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
                if (!Translate(x, y, out int row, out int col)) return;
                // then get local x,y
                if (!Translate(x, y, row, col, out float lx, out float ly)) throw new Exception("failed to get local x,y");

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
            lock (Cells)
            {
                // iterate through and update the Surface for the dirty cells
                for (int row = 0; row < Cells.Length; row++)
                {
                    if (Cells[row] == null) continue;

                    for (int col = 0; col < Cells[row].Length; col++)
                    {
                        // provide the ability to directly paint into this cell
                        if (OnCellPaint != null)
                        {
                            SubsetOfBoardGraphics.SetScoping(x: col * CellWidth, y: row * CellHeight, CellWidth, CellHeight);
                            if (OnCellPaint(SubsetOfBoardGraphics, row, col)) continue;
                        }

                        if (Cells[row][col].Image == null) continue;

                        if (Cells[row][col].IsDirty || Overlay.IsDirty)
                        {
                            Cells[row][col].IsDirty = false;
                            Surface.Image(Cells[row][col].Image, Cells[row][col].Left, Cells[row][col].Top, Cells[row][col].Width, Cells[row][col].Height);
                        } // if (Cells.IsDirty)
                    } // for
                } // for
            } // lock(Cells)

            // mark the overlay as no longer dirty
            Overlay.IsDirty = false;

            lock (OverlayLock)
            {
                var useOverlayImage = true;
                if (OnOverlayPaint != null)
                {
                    SubsetOfBoardGraphics.SetScoping(x: 0, y: 0, Surface.Width, Surface.Height);
                    if (OnOverlayPaint(Surface)) useOverlayImage = false;
                }

                // update the overlay (everytime)
                if (useOverlayImage && Overlay.Image != null)
                {
                    Surface.Image(Overlay.Image, 0, 0, Overlay.Image.Width, Overlay.Image.Height);
                }
            }
        }

        public void Resize()
        {
            // invalidate everything so it is painted
            lock (Cells)
            {
                // reset the dimensions
                Height = Surface.Height;
                Width = Surface.Width;

                // update cellheight/width/edges
                InitializeCellBoundaries(Width, Height);

                Clear();

                if (Overlay.Image != null) Overlay.IsDirty = true;
            }

            if (OnResize != null) OnResize();
        }

        // 
        // surface area
        //

        public void UpdateCell(int row, int col, UpdateImageDelegate update)
        {
            if (row < 0 || row >= Rows
                || col < 0
                || Cells[row] == null
                || col >= Cells[row].Length) throw new Exception("Invalid row x col : " + row + "," + col);
            if (update == null) throw new Exception("Must pass in a valid delegate to use for updates");

            lock (Cells)
            {
                if (Cells[row][col].Image == null)
                {
                    // initialize
                    Cells[row][col].Image = Surface.CreateImage(Cells[row][col].Width, Cells[row][col].Height);
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
            }
        }

        public void UpdateOverlay(UpdateImageDelegate update)
        {
            if (Surface == null) return;

            lock (OverlayLock)
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
            }
        }

        #region private
        private IGraphics Surface;
        private ISounds Sounds;
        private BoardConfiguration Config;
        private CellDetails Overlay;
        private object OverlayLock;
        private BoardTranslationGraphics SubsetOfBoardGraphics;

        // tick
        private Timer TickTimer;
        private int TickLock;

        // used for non-rectangluar shapes
        private int EdgeWidth;
        private int EdgeHeight;

        private struct CellDetails
        {
            public IImage Image;
            public bool IsDirty;
            public int Width;
            public int Height;
            public float Top;
            public float Left;

            // used for unique shape layout
            public Point[] Cells;
        }
        private CellDetails[][] Cells;

        private void InitializeCellBoundaries(int width, int height)
        {
            // only for rectangular and hexagon cells
            if (Config.Cells != null && Config.Cells.Length > 0) return;

            // rectangle
            if (Config.EdgeAngle == 0)
            {
                EdgeWidth = 0;
                EdgeHeight = 0;
                CellWidth = width / Config.Columns;
                CellHeight = height / Config.Rows;
            }

            // hexagon
            else
            {
                // the angle starts at the mid point of the width
                // calculate how far down the height the edge will go
                // tan(angle) = opposite / adjacent
                EdgeWidth = ((width / Config.Columns) / 2);
                EdgeHeight = (int)Math.Floor(Math.Tan((Math.PI / 180) * Config.EdgeAngle) * EdgeWidth);

                // add later
                if ((2 * EdgeHeight * Config.Rows) > height) throw new Exception("The EdgeAngle is too steep");

                // need to adjust to an additional EdgeWidth and EdgeHeight (given the shift)
                CellWidth = ((width - EdgeWidth) / Config.Columns);
                CellHeight = ((height - EdgeHeight + (Config.Rows * EdgeHeight)) / Config.Rows);

                // update EdgeWidth
                EdgeWidth = CellWidth / 2;
            }

            // create/update the cells
            if (Cells == null) Cells = new CellDetails[Config.Rows][];
            for (int row = 0; row < Cells.Length; row++)
            {
                if (Cells[row] == null) Cells[row] = new CellDetails[Config.Columns];
                for (int col = 0; col < Cells[row].Length; col++)
                {
                    // rectangle
                    var left = col * CellWidth;
                    var top = row * CellHeight;

                    if (Config.EdgeAngle != 0)
                    {
                        // adjust for hexagon shape
                        if (row % 2 != 0)
                        {
                            // odd
                            left += EdgeWidth;
                        }
                        top -= (EdgeHeight * row);
                    }

                    // init/update
                    Cells[row][col].IsDirty = true;
                    Cells[row][col].Image = null;
                    Cells[row][col].Width = CellWidth;
                    Cells[row][col].Height = CellHeight;
                    Cells[row][col].Left = left;
                    Cells[row][col].Top = top;
                } // for col
            } // for row
        }

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
            if (CellWidth == 0 || CellHeight == 0 || EdgeHeight == 0 || EdgeWidth == 0) return;

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

        // screen x,y into row,col
        private bool Translate(float x, float y, out int row, out int col)
        {
            // clean up edges
            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            if (y < 0) y = 0;
            if (y >= Height) y = Height - 1;

            // unique cell shapes
            if (Config.Cells != null && Config.Cells.Length > 0)
            {
                // iterate through the shapes looking for a match
                for(row=0; row<Cells.Length; row++)
                {
                    if (Cells[row] == null) continue;
                    for(col=0; col<Cells[row].Length; col++)
                    {
                        // check if this point resides within this region
                        if (Collision.PointWithinPolygon(x,y, Cells[row][col].Cells)) return true;
                    }
                }

                // no match found
                row = col = -1;
                return false;
            }

            // rectangle
            else if (Config.EdgeAngle == 0)
            {
                col = (int)Math.Floor(x / (float)CellWidth);
                row = (int)Math.Floor(y / (float)CellHeight);

                // mind the edges
                if (row < 0) row = 0;
                if (row >= Config.Rows) row = Config.Rows - 1;
                if (col < 0) col = 0;
                if (col >= Config.Columns) col = Config.Columns - 1;

                return true;
            }

            // hexagon
            else if (Config.EdgeAngle != 0)
            {
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

                // mind the edges
                if (row < 0) row = 0;
                if (row >= Config.Rows) row = Config.Rows - 1;
                if (col < 0) col = 0;
                if (col >= Config.Columns) col = Config.Columns - 1;

                // get the local coordinates 
                if (!Translate(x, y, row, col, out float lx, out float ly)) throw new Exception("failed to get local x,y");

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

                // mind the edges
                if (row < 0) row = 0;
                if (row >= Config.Rows) row = Config.Rows - 1;
                if (col < 0) col = 0;
                if (col >= Config.Columns) col = Config.Columns - 1;

                return true;
            } // else if (hexagon)

            else
            {
                throw new Exception("Unknow cell configuration");
            }
        }

        // screen x,y to cell local x,y
        private bool Translate(float x, float y, int row, int col, out float lx, out float ly)
        {
            // init
            lx = ly = 0f;

            // substract the difference to get the local x,y
            lx = x - Cells[row][col].Left;
            ly = y - Cells[row][col].Top;

            return true;
        }

        private void TickUpdate(object state)
        {
            if (OnTick == null) return;
            if (Surface == null) return;

            // the timer is reentrant, so only allow one instance to run
            if (System.Threading.Interlocked.CompareExchange(ref TickLock, 1, 0) != 0) return;

            var timer = new Stopwatch();
            timer.Start();
            {
                // make callback
                OnTick();
            }
            timer.Stop();

            // set state back to not running
            System.Threading.Volatile.Write(ref TickLock, 0);

            if (timer.ElapsedMilliseconds > Constants.GlobalClock) System.Diagnostics.Debug.WriteLine("**TickUpdate Duration {0} ms", timer.ElapsedMilliseconds);
        }

        #endregion
    }
}

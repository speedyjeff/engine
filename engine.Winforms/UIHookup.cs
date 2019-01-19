using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using engine.Common;

namespace engine.Winforms
{
    public class UIHookup
    {
        // Pre-requisit, must set ...
        //   DoubleBuffered = true
        //   Height
        //   Width
        public UIHookup(Control home, World world)
        {
            // retain out home control
            Control = home;
            Logic = world;

            // initialize
            Initialize();
        }

        public UIHookup(Control home, Board board)
        {
            // retain out home control
            Control = home;
            Logic = board;

            // initialize
            Initialize();
        }

        public void ProcessCmdKey(Keys keyData)
        {
            // user input
            if (keyData == Keys.Left) Logic.KeyPress(Common.Constants.LeftArrow);
            else if (keyData == Keys.Right) Logic.KeyPress(Common.Constants.RightArrow);
            else if (keyData == Keys.Up) Logic.KeyPress(Common.Constants.UpArrow);
            else if (keyData == Keys.Down) Logic.KeyPress(Common.Constants.DownArrow);
            else if (keyData == Keys.Space) Logic.KeyPress(Common.Constants.Space);
            else if (keyData == Keys.Escape) Logic.KeyPress(Common.Constants.Esc);
        }

        public void ProcessWndProc(ref Message m)
        {
            if (m.Msg == MM_MCINOTIFY)
            {
                // playback has completed
                Sound.Repeat();
            }
        }

        public WritableGraphics Surface { get; private set; }
        public Sounds Sound { get; private set; }

        #region private
        private const int MM_MCINOTIFY = 953;

        private Control Control;

        private IUserInteraction Logic;

        private Timer OnPaintTimer;
        private Timer OnMoveTimer;

        private void Initialize()
        {

            try
            {
                Control.SuspendLayout();

                // double buffer
                Surface = new WritableGraphics(BufferedGraphicsManager.Current, Control.CreateGraphics(), Control.Height, Control.Width);
                Sound = new Sounds(Control.Handle);

                // initialize the graphics
                Logic.InitializeGraphics(
                    Surface,
                    Sound
                    );

                // default handlers
                Control.Resize += OnResize;
                Control.Paint += OnPaint;

                // timers
                OnPaintTimer = new Timer();
                OnPaintTimer.Interval = Common.Constants.GlobalClock / 2;
                OnPaintTimer.Tick += OnPaintTimer_Tick;
                OnMoveTimer = new Timer();
                OnMoveTimer.Interval = Common.Constants.GlobalClock / 2;
                OnMoveTimer.Tick += OnMoveTimer_Tick;

                // setup callbacks
                Control.KeyPress += OnKeyPressed;
                Control.MouseUp += OnMouseUp;
                Control.MouseDown += OnMouseDown;
                Control.MouseMove += OnMouseMove;
                Control.MouseWheel += OnMouseWheel;

                OnPaintTimer.Start();
            }
            finally
            {
                Control.ResumeLayout();
            }
        }

        private void OnPaintTimer_Tick(object sender, EventArgs e)
        {
            Stopwatch duration = new Stopwatch();
            duration.Start();
            Logic.Paint();
            Control.Refresh();
            duration.Stop();
            if (duration.ElapsedMilliseconds > (Common.Constants.GlobalClock / 2) - 5) System.Diagnostics.Debug.WriteLine("**Paint Duration {0} ms", duration.ElapsedMilliseconds);
        }

        private void OnMoveTimer_Tick(object sender, EventArgs e)
        {
            Logic.KeyPress(Common.Constants.RightMouse);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Surface.RawRender(e.Graphics);
        }

        private void OnResize(object sender, EventArgs e)
        {
            Surface.RawResize(Control.CreateGraphics(), Control.Height, Control.Width);
            Logic.Resize();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Logic.Mousewheel(e.Delta);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // translate the location into an angle relative to the mid point
            //        360/0
            //   270         90
            //         180

            // Width/2 and Height/2 act as the center point
            float angle = Common.Collision.CalculateAngleFromPoint(Control.Width / 2.0f, Control.Height / 2.0f, e.X, e.Y);

            Logic.Mousemove(e.X, e.Y, angle);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            // fire a keyboard event
            if (e.Button == MouseButtons.Left) Logic.KeyPress(Common.Constants.LeftMouse);
            else if (e.Button == MouseButtons.Right) OnMoveTimer.Start();
            else if (e.Button == MouseButtons.Middle) Logic.KeyPress(Common.Constants.MiddleMouse);

            // fire a mouse event
            Logic.Mousedown(
                e.Button == MouseButtons.Left ? MouseButton.Left :
                e.Button == MouseButtons.Middle ? MouseButton.Middle :
                MouseButton.Right,
                e.X,
                e.Y);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            // fire the keyboard event
            if (e.Button == MouseButtons.Right) OnMoveTimer.Stop();

            // fire a mouse event
            Logic.Mouseup(
                e.Button == MouseButtons.Left ? MouseButton.Left :
                e.Button == MouseButtons.Middle ? MouseButton.Middle :
                MouseButton.Right,
                e.X,
                e.Y);
        }

        private void OnKeyPressed(object sender, KeyPressEventArgs e)
        {
            Logic.KeyPress(e.KeyChar);
        }
        #endregion
    }
}

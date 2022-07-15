using engine.Common;
using SkiaSharp.Views.Maui;
using SkiaSharp;
using System.Diagnostics;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using System.Threading;
using System;

namespace engine.Maui
{
    public class UIHookup
    {
        public UIHookup(ContentView content, IUserInteraction logic)
        {
            // retain the home page
            Content = content;
            Logic = logic;

            // setup
            var canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasView_PaintSurface;
            Content.Content = canvasView;

            // handle touch/mouse
            canvasView.EnableTouchEvents = true;
            canvasView.Touch += CanvasView_Touch;

            // handle keyboard
            Keyboard = PlatformKeyboardFactory.Create();
            if (Keyboard != null) Keyboard.OnKeyPress += Keyboard_OnKeyPress;
            else System.Diagnostics.Debug.WriteLine("failed to get a platform keyboard");

            // track size changes
            Content.SizeChanged += Page_SizeChanged;
            // do not create the graphics here (the bounds are not established)
            //   create the graphics in SizeChanged

            // initialize
            var width = Math.Max(10, Content.Bounds.Width);
            var height = Math.Max(10, Content.Bounds.Height);
            var canvas = CreateDoubleBuffer(width, height);
            Surface = new MauiGraphics(canvas, (int)width, (int)height);
            Sound = new MauiSounds();

            // todo handle shutdown

            // todo
            // initialize the graphics
            Logic.InitializeGraphics(
                Surface,
                Sound
                );

            // paint timer
            OnPaintTimer = new Timer(
                callback: OnPaintTimer_Tick,
                state: null,
                dueTime: 0,
                period: 50);

            // move timer
            OnMoveTimer = new Timer(
                callback: OnMoveTimer_Tick,
                state: null,
                dueTime: Timeout.Infinite, // do not start
                period: 50); // timer period duplicated below
        }

        public MauiGraphics Surface { get; private set; }
        public MauiSounds Sound { get; private set; }

        #region private
        private ContentView Content;
        private Timer OnPaintTimer;
        private Timer OnMoveTimer;
        private MauiImage DoubleBuffer;
        private IPlatformKeyboard Keyboard;
        private IUserInteraction Logic;

        static UIHookup()
        {
            // set that this is a Winforms type
            engine.Common.Platform.SetType(PlatformType.Maui);
        }

        private SKCanvas CreateDoubleBuffer(double width, double height)
        {
            lock (this)
            {
                if (DoubleBuffer != null) DoubleBuffer.Close();

                // get dpi
                var dpi = Math.Max(1d, Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density);

                // create a new one
                DoubleBuffer = new MauiImage(
                    width: Convert.ToInt32(width * dpi),
                    height: Convert.ToInt32(height * dpi));

                // return canvas
                return new SKCanvas(DoubleBuffer.UnderlyingBitmap);
            }
        }

        //
        // user input
        //
        private void Keyboard_OnKeyPress(char charkey)
        {
            // pass to the logic layer
            Logic.KeyPress(charkey);
        }

        private void CanvasView_Touch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.WheelChanged) { } //Logic.Mousewheel(e.Delta);
            else if (e.ActionType == SKTouchAction.Moved)
            {
                // translate the location into an angle relative to the mid point
                //        360/0
                //   270         90  
                //         180

                // Width/2 and Height/2 act as the center point
                float angle = Common.Collision.CalculateAngleFromPoint((float)Content.Width / 2.0f, (float)Content.Height / 2.0f, e.Location.X, e.Location.Y);

                Logic.Mousemove(e.Location.X, e.Location.Y, angle);
            }
            else if (e.ActionType == SKTouchAction.Pressed)
            {
                // fire a keyboard event
                if (e.MouseButton == SKMouseButton.Left || e.DeviceType == SKTouchDeviceType.Touch || e.DeviceType == SKTouchDeviceType.Pen) { } // Logic.KeyPress(Common.Constants.LeftMouse);
                else if (e.MouseButton == SKMouseButton.Right) OnMoveTimer.Change(dueTime: 0, period: 50);
                else if (e.MouseButton == SKMouseButton.Middle) { } // Logic.KeyPress(Common.Constants.MiddleMouse);

                // fire a mouse event
                Logic.Mousedown(
                    e.MouseButton == SKMouseButton.Left ? MouseButton.Left :
                      (e.MouseButton == SKMouseButton.Middle ? MouseButton.Middle :
                      MouseButton.Right),
                    e.Location.X,
                    e.Location.Y);
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                // fire the keyboard event
                if (e.MouseButton == SKMouseButton.Right) OnMoveTimer.Change(dueTime: Timeout.Infinite, period: 50);

                // fire a mouse event
                Logic.Mouseup(
                    e.MouseButton == SKMouseButton.Left ? MouseButton.Left :
                      (e.MouseButton == SKMouseButton.Middle ? MouseButton.Middle :
                      MouseButton.Right),
                    e.Location.X,
                    e.Location.Y);
            }
        }

        // 
        // timers
        //
        private void OnMoveTimer_Tick(object state)
        {
            Logic.KeyPress(Common.Constants.RightMouse);
        }

        private void OnPaintTimer_Tick(object state)
        {
            if (DoubleBuffer == null) return;
            // todo there is a shutdown race here
            Stopwatch duration = new Stopwatch();
            duration.Start();
            lock (this)
            {
                Logic.Paint();
            }
            // wrap to avoid early entrance issues and shutdown issues
            try
            {
                ISKCanvasView view = null;
                if (Content.Content != null && Content.Content is ISKCanvasView) view = (Content.Content as ISKCanvasView);
                if (view != null) view.InvalidateSurface();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"paint tick error : {e.Message}");
            }
            duration.Stop();
            if (duration.ElapsedMilliseconds > (50) - 5) System.Diagnostics.Debug.WriteLine("**Paint Duration {0} ms", duration.ElapsedMilliseconds);
        }

        //
        // callbacks
        // 
        private void OnCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (DoubleBuffer == null) return;
            lock (this)
            {
                // draw the double buffer to the screen
                args.Surface.Canvas.DrawBitmap(
                    DoubleBuffer.UnderlyingBitmap,
                    new SKRect()
                    {
                        Top = 0,
                        Left = 0,
                        Bottom = DoubleBuffer.Height,
                        Right = DoubleBuffer.Width
                    });
            }
        }

        private void Page_SizeChanged(object sender, EventArgs e)
        {
            var canvas = CreateDoubleBuffer(Content.Bounds.Width, Content.Bounds.Height);
            Surface.RawResize(canvas, (int)Content.Bounds.Width, (int)Content.Bounds.Height);
            Logic.Resize();
        }
        #endregion
    }
}

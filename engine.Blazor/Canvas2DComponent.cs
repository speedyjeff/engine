using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using engine.Common;
using engine.XPlatform;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace engine.Blazor
{
    public class Canvas2DComponent : ComponentBase
    {
        public Canvas2DComponent()
        {
            Surface = null;
            Sound = new BlazorSounds();
        }

        public Canvas2DGraphics Surface { get; private set; }
        public BlazorSounds Sound { get; private set; }

        public int Height { get; private set; }
        public int Width { get; private set; }

        #region protected
        // referenced from Blazor
        protected BECanvasComponent CanvasReference;
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        //
        // events
        //
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Surface == null)
            {
                // get the 2d canvas
                Context = await CanvasReference.CreateCanvas2DAsync();

                // initialize the graphics engine
                Width = (int)CanvasReference.Width;
                Height = (int)CanvasReference.Height;
                Surface = new Canvas2DGraphics(Context, DrawDomImage, width: Width, height: Height);

                // start paint callbacks
                OnPaintTimer = new Timer(OnPaint, state: null, dueTime: 0 /*ms*/, period: (Common.Constants.GlobalClock / 2) /*ms*/);
                OnMoveTimer = new Timer(OnMove, state: null, dueTime: 0, period: 0);

                // check to see if we can initialize the logic
                if (Logic != null)
                {
                    // initialize the graphics
                    Logic.InitializeGraphics(
                        Surface,
                        Sound
                        );
                }
            }
        }

        //
        // Callbacks
        //
        protected void ConfigureCanvas(IUserInteraction logic)
        {
            Logic = logic;

            if (Surface != null)
            {
                // initialize the graphics
                Logic.InitializeGraphics(
                    Surface,
                    Sound
                    );
            }
        }

        protected void KeyPress(KeyboardEventArgs e)
        {
            if (Logic == null) return;

            // pass through keys pressed
            Logic.KeyPress(e.Key[0]);
        }

        protected void KeyDown(KeyboardEventArgs e)
        {
            if (Logic == null) return;

            // capture the special keys
            switch (e.Key.ToLower())
            {
                case "arrowright": Logic.KeyPress(Common.Constants.RightArrow); break;
                case "arrowleft": Logic.KeyPress(Common.Constants.LeftArrow); break;
                case "arrowdown": Logic.KeyPress(Common.Constants.DownArrow); break;
                case "arrowup": Logic.KeyPress(Common.Constants.UpArrow); break;
                case "escape": Logic.KeyPress(Common.Constants.Esc); break;
            }
        }

        protected void MouseUp(MouseEventArgs e)
        {
            if (Logic == null) return;

            if (e.Button != (int)MouseButton.Left && e.Button != (int)MouseButton.Middle && e.Button != (int)MouseButton.Right) throw new Exception("Unknow mouse button : " + e.Button);

            // fire the keyboard event
            if (e.Button == (int)MouseButton.Right) OnMoveTimer.Change(dueTime: 0, period: 0);

            // fire a mouse event
            Logic.Mouseup(
                (MouseButton)e.Button,
                (float)e.ClientX,
                (float)e.ClientY);
        }

        protected void MouseDown(MouseEventArgs e)
        {
            if (Logic == null) return;

            // fire a keyboard event
            if (e.Button == (int)MouseButton.Left) Logic.KeyPress(Common.Constants.LeftMouse);
            else if (e.Button == (int)MouseButton.Right) OnMoveTimer.Change(dueTime: (Common.Constants.GlobalClock / 2) /*ms*/, period: (Common.Constants.GlobalClock / 2) /*ms*/);
            else if (e.Button == (int)MouseButton.Middle) Logic.KeyPress(Common.Constants.MiddleMouse);
            else throw new Exception("Unknow mouse button : " + e.Button);

            // fire a mouse event
            Logic.Mousedown(
                (MouseButton)e.Button,
                (float)e.ClientX,
                (float)e.ClientY);
        }

        protected void MouseMove(MouseEventArgs e)
        {
            if (Logic == null) return;

            // translate the location into an angle relative to the mid point
            //        360/0
            //   270         90
            //         180

            // Width/2 and Height/2 act as the center point
            float angle = Common.Collision.CalculateAngleFromPoint(CanvasReference.Width / 2.0f, CanvasReference.Height / 2.0f, (float)e.ClientX, (float)e.ClientY);

            Logic.Mousemove((float)e.ClientX, (float)e.ClientY, angle);
        }

        protected void MouseWheel(WheelEventArgs e)
        {
            if (Logic == null) return;

            Logic.Mousewheel((float)e.DeltaY);
        }
        #endregion

        #region private
        private Canvas2DContext Context;
        private Timer OnPaintTimer;
        private Timer OnMoveTimer;
        private int PaintLock = 0;
        private IUserInteraction Logic;
        private static int _NextDomId = 0;

        static Canvas2DComponent()
        {
            // set that this is a blazor app
            Platform.SetType(PlatformType.Blazor);
        }

        //
        // paint
        //
        private void OnPaint(object state)
        {
            // exit early if we are not setup yet
            if (Surface == null || Logic == null) return;

            // the timer is reentrant, so only allow one instance to run
            if (System.Threading.Interlocked.CompareExchange(ref PaintLock, 1, 0) != 0) return;

            var duration = new Stopwatch();

            // invoke on the UI thread
            duration.Start();
            this.InvokeAsync(Logic.Paint);
            duration.Stop();

            // set state back to not running
            System.Threading.Volatile.Write(ref PaintLock, 0);

            if (duration.ElapsedMilliseconds > (Common.Constants.GlobalClock / 2) - 5) System.Diagnostics.Debug.WriteLine("**Paint Duration {0} ms", duration.ElapsedMilliseconds);
        }

        //
        // movement
        //
        private void OnMove(object state)
        {
            if (Logic == null) return;
            Logic.KeyPress(Common.Constants.RightMouse);
        }

        //
        // javascript interaction layer
        //
        private static string NextDomId()
        {
            var id = System.Threading.Interlocked.Increment(ref _NextDomId);
            return $"simg{id}";
        }

        private void DrawDomImage(IImage img, float x, float y, float width, float height)
        {
            var simg = img as ImageSharpImage;
            if (simg == null) throw new Exception("Must provide an ImageSharpImage");
            var updateImageSrc = false;
            var timer = new Stopwatch();

            // ensure the base64string is computed
            if (string.IsNullOrWhiteSpace(simg.Base64) || simg.HasChanged)
            {
                timer.Reset(); timer.Start();
                simg.ComputeBase64();
                timer.Stop();
                updateImageSrc = true;
                simg.HasChanged = false;
                System.Diagnostics.Debug.WriteLine($"computebase64 {timer.ElapsedMilliseconds}");
            }

            // ensure this image is backed by a dom element
            if (string.IsNullOrWhiteSpace(simg.DomId))
            {
                // create the image
                simg.DomId = NextDomId();
                CreateImage(simg.DomId);

                // update the img src
                updateImageSrc = true;
            }

            // update the image            
            if (updateImageSrc)
            {
                // update the image src
                timer.Reset(); timer.Start();
                UpdateImage(simg.DomId, simg.Base64);
                timer.Stop();
                System.Diagnostics.Debug.WriteLine($"update {timer.ElapsedMilliseconds}");
            }

            // draw
            DrawImage(simg.DomId, x, y, width, height);
        }

        private async void CreateImage(string id)
        {
            await JSRuntime.InvokeVoidAsync("createImage", Context.Canvas, id);
        }

        private async void UpdateImage(string id, string src)
        {
            await JSRuntime.InvokeVoidAsync("updateImage", id, src);
        }

        private async void DrawImage(string id, float dx, float dy, float dw, float dh)
        {
            await JSRuntime.InvokeVoidAsync("drawImage", Context.Canvas, id, dx, dy, dw, dh);
        }
        #endregion
    }
}

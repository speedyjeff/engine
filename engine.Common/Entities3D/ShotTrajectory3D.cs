using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities3D
{
    public class ShotTrajectory3D : ShotTrajectory
    {
        public Element3D Body { get; set; }

        public ShotTrajectory3D(float x, float y, float z) : base()
        {
            Duration = 50;
            BasePace = 10;
            Health = 100;
            Width = Height = Depth = 10f;
            X = x;
            Y = y;
            Z = z;
            Body = new Pyramid() { Wireframe = false, DisableShading = false, X = x, Y = y, Z = z, Width = Width, Height = Height, Depth = Depth, UniformColor = new RGBA() { R = 255, A = 255 } };
        }

        public override void Draw(IGraphics g)
        {
            if (Body != null) Body.Draw(g);
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            if (Body != null) Body.Move(xDelta, yDelta, zDelta);
            base.Move(xDelta, yDelta, zDelta);
        }

        public override bool Action(out float xdelta, out float ydelta, out float zdelta)
        {
            // setup
            xdelta = ydelta = zdelta = 0f;

            // do not move if dead
            if (IsDead) return false;

            // setup
            if (XDelta == 0f && YDelta == 0f && ZDelta == 0)
            {
                // normalize to origin
                XDelta = X2 - X1;
                YDelta = Y2 - Y1;
                ZDelta = Z2 - Z1;

                // normalize
                var sum = Math.Abs(XDelta) + Math.Abs(YDelta) + Math.Abs(ZDelta);
                XDelta /= sum;
                YDelta /= sum;
                ZDelta /= sum;
            }

            // roll
            if (Body != null) Body.Rotate(yaw: 0f, pitch: 0f, roll: 10f);

            // move
            xdelta = XDelta;
            ydelta = YDelta;
            zdelta = ZDelta;

            return true;
        }

        public override void Feedback(bool result)
        {
            // if we hit something, stop moving
            if (!result) ReduceHealth(Health);
        }

        #region private
        private float XDelta;
        private float YDelta;
        private float ZDelta;
        #endregion
    }
}

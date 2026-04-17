using System;
using engine.Common.Entities;

namespace engine.Common.Entities3D
{
    public class Player3D : Player
    {
        public bool ShowTarget { get; set; }
        public Element3D Body { get; set; }
        public bool LockBodyToCamera { get; set; } = true;

        public Player3D()
        {
            Depth = Math.Max(Width, Height);
        }

        public override void Draw(IGraphics g)
        {
            if (ShowDefaultDrawing && Body == null) Body = CreateDefaultBody();
            if (Body is Humanoid3D humanoid) UpdateHumanoidPose(humanoid);

            if (ShowTarget)
            {
                g.DisableTranslation();
                {
                    g.Line(RGBA.Black, g.Width / 2, (g.Height / 2) - 10, g.Width / 2, (g.Height / 2) + 10, 1);
                    g.Line(RGBA.Black, (g.Width / 2) - 10, g.Height / 2, (g.Width / 2) + 10, g.Height / 2, 1);
                }
                g.EnableTranslation();
            }
            if (Body != null)
            {
                // The local player body can be camera-locked, but other 3D players/NPCs
                // should render in world space with normal perspective.
                if (LockBodyToCamera) g.DisableTranslation(TranslationOptions.Translation | TranslationOptions.Scaling);
                {
                    Body.Draw(g);
                }
                if (LockBodyToCamera) g.EnableTranslation();
            }
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            base.Move(xDelta, yDelta, zDelta);

            if (Body != null)
            {
                var movement = Math.Abs(xDelta) + Math.Abs(yDelta) + Math.Abs(zDelta);
                if (movement > 0.001f) WalkPhase += Math.Min(0.45f, movement * 0.18f);
                Body.Move(xDelta, yDelta, zDelta);

                if (Body is Humanoid3D humanoid) UpdateHumanoidPose(humanoid);
            }
        }

        #region private
        private float WalkPhase;

        private static Humanoid3D CreateDefaultBody()
        {
            var body = new Humanoid3D()
            {
                ShirtColor = new RGBA() { R = 96, G = 108, B = 92, A = 255 },
                PantsColor = new RGBA() { R = 52, G = 60, B = 72, A = 255 },
                BootColor = new RGBA() { R = 86, G = 62, B = 42, A = 255 },
                SkinColor = new RGBA() { R = 244, G = 214, B = 58, A = 255 },
                BackpackColor = new RGBA() { R = 81, G = 95, B = 75, A = 255 },
            };

            return body;
        }

        private void UpdateHumanoidPose(Humanoid3D humanoid)
        {
            humanoid.X = X;
            humanoid.Y = Y;
            humanoid.Z = Z;
            humanoid.Width = Width;
            humanoid.Height = Height;
            humanoid.Depth = Depth;
            humanoid.WalkPhase = WalkPhase;
            humanoid.UpdatePose();
        }
        #endregion
    }
}

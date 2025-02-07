using System;
using engine.Common.Entities;

namespace engine.Common.Entities3D
{
    public class Player3D : Player
    {
        public bool ShowTarget { get; set; }
        public Element3D Body { get; set; }

        public Player3D()
        {
            Depth = Math.Max(Width, Height);
        }

        public override void Draw(IGraphics g)
        {
            // check if we should initialize the default drawing
            if (Body == null && ShowDefaultDrawing)
            {
                // create the body
                Body = new ComboElement3D()
                {
                    Height = Height,
                    Width = Width,
                    Depth = Depth,
                    X = X,
                    Y = Y,
                    Z = Z,
                };
                // create a diamond
                var yellow = new RGBA() { R = 255, G = 255, B = 0, A = 255 };
                var p1 = new Pyramid() { UniformColor = yellow, X = X, Y = Y - (Height * 0.25f), Z = Z, Width = Width, Height = Height * 0.5f, Depth = Depth };
                var p2 = new Pyramid() { UniformColor = yellow, X = X, Y = Y + (Height * 0.25f), Z = Z, Width = Width, Height = Height * 0.5f, Depth = Depth };
                // flip the lower pyramid to make a diamond
                p2.Rotate(yaw: 0, pitch: 180, roll: 0);
                // add them to the combo
                (Body as ComboElement3D).AddInner(p1);
                (Body as ComboElement3D).AddInner(p2);
            }

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
                // do not apply yaw, pitch, or roll when drawing the player (the camera is the one that is moving)
                g.DisableTranslation(TranslationOptions.Translation | TranslationOptions.Scaling);
                {
                    Body.Draw(g);
                }
                g.EnableTranslation();
            }
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            base.Move(xDelta, yDelta, zDelta);

            // also update the body
            if (Body != null) Body.Move(xDelta, yDelta, zDelta);
        }
    }
}

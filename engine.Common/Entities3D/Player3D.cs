using engine.Common;
using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                g.DisableTranslation(TranslationOptions.Translation | TranslationOptions.Scaling | TranslationOptions.RotationPitch);
                {
                    Body.Draw(g);
                };
            }
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            base.Move(xDelta, yDelta, zDelta);

            // also update the body
            Body.Move(xDelta, yDelta, zDelta);
        }
    }
}

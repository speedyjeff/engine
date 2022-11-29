using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities
{
    public class Health : Element
    {
        public Health()
        {
            CanAcquire = true;
            Health = 25;
            Name = "Health";

            Height = 50;
            Width = 50;
        }

        public override void Draw(IGraphics g)
        {
            if (ShowDefaultDrawing)
            {
                g.Rectangle(RGBA.White, X - (Width / 2), Y - (Height / 2), Width, Height);
                g.Line(Red, X - (Width / 2), Y, X + (Width / 2), Y, 5f);
                g.Line(Red, X, Y - (Height / 2), X, Y + (Height / 2), 5f);
            }
            base.Draw(g);
        }

        #region private
        private readonly RGBA Red = new RGBA() { R = 255, G = 0, B = 0, A = 200 };
        #endregion
    }
}

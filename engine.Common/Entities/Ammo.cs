using System;
using System.Collections.Generic;
using System.Text;

using engine.Common;
using engine.Common.Entities;

namespace engine.Common.Entities
{
    public class Ammo : Element
    {
        public Ammo() : base()
        {
            CanAcquire = true;
            Name = "Ammo";
            Width = 25;
            Height = 25;
            // number of shots
            switch(Id % 4)
            {
                case 0: Health = 20; break;
                case 1: Health = 40; break;
                case 2: Health = 80; break;
                default: Health = 100; break;
            }
        }

        public override void Draw(IGraphics g)
        {
            if (ShowDefaultDrawing)
            {
                g.Rectangle(Gray, X - (Width / 2), Y - (Height / 2), Width / 3, Height, fill: false);
                g.Rectangle(Gray, X - (Width / 3), Y - (Height / 2), Width / 3, Height);
                g.Rectangle(Gray, X + (Width / 3), Y - (Height / 2), Width / 3, Height, fill: false);
            }
            base.Draw(g);
        }

        #region private
        private readonly RGBA Gray = new RGBA() { R = 154, G = 166, B = 173, A = 200 };
        #endregion
    }
}

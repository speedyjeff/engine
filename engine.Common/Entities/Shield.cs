﻿using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities
{
    public class Shield : Element
    {
        public Shield() : base()
        {
            Shield = 20;
            CanAcquire = true;
            Name = "Shield";
            Height = 25;
            Width = 25;
        }

        public override void Draw(IGraphics g)
        {
            if (ShowDefaultDrawing)
            {
                g.Ellipse(Gray, X - (Width / 2), Y - (Height / 2), Width, Height);
            }
            base.Draw(g);
        }

        #region private
        private readonly RGBA Gray = new RGBA() { R = 85, G = 85, B = 85, A = 255 };
        #endregion
    }
}

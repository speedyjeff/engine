using engine.Common;
using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer2D
{
    class Platform : Obstacle
    {
        public Platform()
        {
            Name = "Platform";
        }

        public RGBA Color { get; set; }

        public override void Draw(IGraphics g)
        {
            g.Rectangle(Color, X - (Width / 2), Y - (Height / 2), Width, Height, true);
            base.Draw(g);
        }
    }
}

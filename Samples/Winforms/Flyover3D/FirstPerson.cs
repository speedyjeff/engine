using engine.Common;
using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyover3D
{
    class FirstPerson : Player
    {
        public override void Draw(IGraphics g)
        {
            g.DisableTranslation();
            {
                g.Line(RGBA.Black, g.Width / 2, (g.Height / 2) - 10, g.Width / 2, (g.Height / 2) + 10, 1);
                g.Line(RGBA.Black, (g.Width / 2) - 10, g.Height / 2, (g.Width / 2) + 10, g.Height / 2, 1);
            }
            g.EnableTranslation();
        }
    }
}

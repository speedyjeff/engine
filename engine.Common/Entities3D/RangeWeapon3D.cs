using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public class RangeWeapon3D : RangeWeapon
    {
        public Element3D Body { get; set; }

        public override void Draw(IGraphics g)
        {
            if (Body != null) Body.Draw(g);
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            if (Body != null) Body.Move(xDelta, yDelta, zDelta);
            base.Move(xDelta, yDelta, zDelta);
        }
    }
}

using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public struct ActionDetails
    {
        public List<Element> Elements;
        public float AngleToCenter;
        public bool InZone;
        public float XDelta;
        public float YDelta;
        public float Angle;
    }
}

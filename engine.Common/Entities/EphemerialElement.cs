using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities
{
    public class EphemerialElement : Element
    {
        // number of clock cycles to keep alive (Global.Clock/2)
        public int Duration { get; set; }
        // current count towards duration
        public int CurrentDuration { get; internal set; }
        // default pace of the element (if applicable)
        public float BasePace { get; set; }

        // callback to apply movement (if applicable)
        public virtual bool Action(out float xdelta, out float ydelta, out float zdelta)
        {
            xdelta = ydelta = zdelta = 0f;
            return false;
        }

        // feedback on if the move was successful (if applicable)
        public virtual void Feedback(bool result)
        {
        }
    }
}

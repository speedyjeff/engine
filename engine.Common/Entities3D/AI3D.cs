using System;
using engine.Common.Entities;
using engine.Common.Entities.AI;

namespace engine.Common.Entities3D
{
    public class AI3D : Player3D, IAI
    {
        public AI3D()
        {
            ShowDamage = true;
            Color = new RGBA() { R = 0, G = 0, B = 255, A = 255 };
            ShowDefaultDrawing = false;
            LockBodyToCamera = false;
            ShowDiagnostics = Constants.Debug_AIMoveDiag;
        }

        public bool ShowDiagnostics { get; protected set; }

        public virtual ActionEnum Action(System.Collections.Generic.List<Element> elements, float angleToCenter, bool inZone, ref float xdelta, ref float ydelta, ref float zdelta, ref float angle)
        {
            return ActionEnum.None;
        }
    }
}

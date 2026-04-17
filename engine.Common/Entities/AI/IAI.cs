using System.Collections.Generic;
using engine.Common.Entities;

namespace engine.Common.Entities.AI
{
    public interface IAI
    {
        bool ShowDiagnostics { get; }

        ActionEnum Action(List<Element> elements, float angleToCenter, bool inZone, ref float xdelta, ref float ydelta, ref float zdelta, ref float angle);
    }
}

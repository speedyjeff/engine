using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Networking
{
    public struct MoveResult
    {
        public int ElementId { get; set; }
        public bool Outcome { get; set; }
    }
}

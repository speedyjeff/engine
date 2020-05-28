using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public enum PlatformType { None, Winforms, Blazor };
    public static class Platform
    {
        static Platform()
        {
            Type = PlatformType.None;
        }

        public static PlatformType Type { get; private set; }

        public static bool IsType(PlatformType type)
        {
            return Type == type;
        }

        public static void SetType(PlatformType type)
        {
            if (Type == PlatformType.None) Type = type;
        }
    }
}

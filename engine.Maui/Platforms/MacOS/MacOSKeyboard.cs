using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui.Platforms.MacOS
{
    class MacOSKeyboard : IPlatformKeyboard
    {
        public event Action<char> OnKeyPress;

        public static IPlatformKeyboard Create()
        {
            // todo
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui.Platforms.Linux
{
    class LinuxKeyboard : IPlatformKeyboard
    {
        public event Action<char> OnKeyPress;

        public static IPlatformKeyboard Create()
        {
            // todo
            return null;
        }
    }
}

using engine.Maui.Platforms.Linux;
using engine.Maui.Platforms.MacOS;
using engine.Maui.Platforms.Windows;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace engine.Maui
{
    interface IPlatformKeyboard
    {
        public event Action<char> OnKeyPress;
    }

    static class PlatformKeyboardFactory
    { 
        public static IPlatformKeyboard Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return LoadForWindows();   
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // MacCatalyst v iOS?
                return LoadForMacOS();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Andriod?
                return LoadForLinux();
            }
            else
            {
                return null;
            }
        }

        #region private

        // no inlining to avoid eagerly loading platform specific assemblies
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IPlatformKeyboard LoadForWindows()
        {
            return WindowsKeyboard.Create();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IPlatformKeyboard LoadForMacOS()
        {
            return MacOSKeyboard.Create();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IPlatformKeyboard LoadForLinux()
        {
            return LinuxKeyboard.Create();
        }
        #endregion
    }
}

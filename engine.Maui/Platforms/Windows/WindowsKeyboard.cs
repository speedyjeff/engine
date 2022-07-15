using Dapplo.Windows.Input.Enums;
using Dapplo.Windows.Input.Keyboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui.Platforms.Windows
{
    internal class WindowsKeyboard : IObserver<KeyboardHookEventArgs>, IPlatformKeyboard
    {
        public event Action<char> OnKeyPress;

        public static WindowsKeyboard Create()
        {
            var keyboard = new WindowsKeyboard();

            // hook up the keyboard listener
            keyboard.KeyboardHook = Dapplo.Windows.Input.Keyboard.KeyboardHook.KeyboardEvents.Subscribe(keyboard);

            return keyboard;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(KeyboardHookEventArgs value)
        {
            var charkey = '\0';
            switch (value.Key)
            {
                // speical
                case VirtualKeyCode.Escape: charkey = Common.Constants.Esc; break;
                case VirtualKeyCode.Up: charkey = Common.Constants.UpArrow; break;
                case VirtualKeyCode.Down: charkey = Common.Constants.DownArrow; break;
                case VirtualKeyCode.Left: charkey = Common.Constants.LeftArrow; break;
                case VirtualKeyCode.Right: charkey = Common.Constants.RightArrow; break;

                // numerics
                case VirtualKeyCode.Key0: charkey = '0'; break;
                case VirtualKeyCode.Key1: charkey = '1'; break;
                case VirtualKeyCode.Key2: charkey = '2'; break;
                case VirtualKeyCode.Key3: charkey = '3'; break;
                case VirtualKeyCode.Key4: charkey = '4'; break;
                case VirtualKeyCode.Key5: charkey = '5'; break;
                case VirtualKeyCode.Key6: charkey = '6'; break;
                case VirtualKeyCode.Key7: charkey = '7'; break;
                case VirtualKeyCode.Key8: charkey = '8'; break;
                case VirtualKeyCode.Key9: charkey = '9'; break;

                // alpha
                case VirtualKeyCode.KeyA: charkey = 'a'; break;
                case VirtualKeyCode.KeyB: charkey = 'b'; break;
                case VirtualKeyCode.KeyC: charkey = 'c'; break;
                case VirtualKeyCode.KeyD: charkey = 'd'; break;
                case VirtualKeyCode.KeyE: charkey = 'e'; break;
                case VirtualKeyCode.KeyF: charkey = 'f'; break;
                case VirtualKeyCode.KeyG: charkey = 'g'; break;
                case VirtualKeyCode.KeyH: charkey = 'h'; break;
                case VirtualKeyCode.KeyI: charkey = 'i'; break;
                case VirtualKeyCode.KeyJ: charkey = 'j'; break;
                case VirtualKeyCode.KeyK: charkey = 'k'; break;
                case VirtualKeyCode.KeyL: charkey = 'l'; break;
                case VirtualKeyCode.KeyM: charkey = 'm'; break;
                case VirtualKeyCode.KeyN: charkey = 'n'; break;
                case VirtualKeyCode.KeyO: charkey = 'o'; break;
                case VirtualKeyCode.KeyP: charkey = 'p'; break;
                case VirtualKeyCode.KeyQ: charkey = 'q'; break;
                case VirtualKeyCode.KeyR: charkey = 'r'; break;
                case VirtualKeyCode.KeyS: charkey = 's'; break;
                case VirtualKeyCode.KeyT: charkey = 't'; break;
                case VirtualKeyCode.KeyU: charkey = 'u'; break;
                case VirtualKeyCode.KeyV: charkey = 'v'; break;
                case VirtualKeyCode.KeyW: charkey = 'w'; break;
                case VirtualKeyCode.KeyX: charkey = 'x'; break;
                case VirtualKeyCode.KeyY: charkey = 'y'; break;
                case VirtualKeyCode.KeyZ: charkey = 'z'; break;
            }

            if (charkey != '\0')
            {
                if (value.IsCapsLockActive || value.IsRightShift || value.IsLeftShift) charkey = Char.ToUpper(charkey);

                // fire event
                if (OnKeyPress != null) OnKeyPress(charkey);
            }
        }

        #region private
        private IDisposable KeyboardHook;
        #endregion
    }
}

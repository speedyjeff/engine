using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common
{
    public enum MouseButton { Left, Middle, Right };

    public interface IUserInteraction
    {
        // Initialization
        void InitializeGraphics(IGraphics surface, ISounds sounds);

        // keyboard input
        void KeyPress(char key);

        // draw the screen
        void Paint();

        // resize
        void Resize();

        // mouse wheel input
        void Mousewheel(float delta);

        // mouse move input
        void Mousemove(float x, float y, float angle);

        // mouse click
        void Mousedown(MouseButton btn, float x, float y);
        void Mouseup(MouseButton btn, float x, float y);
    }
}

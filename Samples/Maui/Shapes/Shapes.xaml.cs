using engine.Common.Entities;
using engine.Common;
using Microsoft.Maui.Controls;
using engine.Maui;

namespace Shapes;

public partial class Shapes : ContentView
{
    public void InitializeBoard(int width, int height)
    {
        // transparent user
        var me = new Player() { ShowDefaultDrawing = false };

        // all the drawing happens in the background as a canvas
        World = new World(
            new WorldConfiguration()
            {
                Width = width,
                Height = height,
                EnableZoom = false,
                ShowCoordinates = false
            },
            players: new Player[] { me },
            objects: null,
            background: new CanvasBackground()
            );

        UI = new UIHookup(this, World);
    }

    public World World { get; private set; }

    #region private
    private UIHookup UI;
    #endregion
}
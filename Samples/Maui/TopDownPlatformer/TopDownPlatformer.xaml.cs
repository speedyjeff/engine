using engine.Common;
using engine.Common.Entities;
using engine.Maui;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace TopDownPlatformer;

public partial class TopDownPlatformer : ContentView
{
	public TopDownPlatformer()
	{
		InitializeComponent();
	}
    public World World { get; private set; }

    public void InitializeSurface(int width, int height)
    {
        var players = new Player[]
        {
                new Player() { Name = "Me", Z = 1 }
        };
        var obstacles = new List<engine.Common.Element>()
            {
                // top
                new Platform()
                {
                    X = 0,
                    Y = -300,
                    Z = float.MaxValue, // to ensure a player can not escape
                    Width = 600,
                    Height = 20,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // left
                new Platform()
                {
                    X = -300,
                    Y = 0,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 20,
                    Height = 600,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // bottom
                new Platform()
                {
                    X = 0,
                    Y = 300,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 600,
                    Height = 20,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                },
                // right
                new Platform()
                {
                    X = 300,
                    Y = 0,
                    Z = float.MaxValue, // to ensure a player can not escape,
                    Width = 20,
                    Height = 600,
                    Color = new RGBA() {R = 152, G = 107, B = 39, A = 255}
                }
            };
        var background = new Background(width, height)
        {
            GroundColor = new RGBA() { R = 0, G = 255, B = 100, A = 255 }
        };

        World = new World(
            new WorldConfiguration()
            {
                Width = width,
                Height = height,
                EnableZoom = true,
                ShowCoordinates = true
            },
            players,
            obstacles.ToArray(),
            background
            );

        UI = new UIHookup(this, World);
    }

    #region private
    private UIHookup UI;
    #endregion
}
using System.Collections.Generic;
using System;
using Microsoft.Maui.Controls;
using engine.Common;
using engine.Maui;
using engine.Common.Entities;

namespace Platformer2D;

public partial class Platformer2D : ContentView
{
	public Platformer2D()
	{
		InitializeComponent();
	}

    public World World { get; private set; }


    public void InitializeSurface(int width, int height)
    {
        var players = new Player[]
        {
                new Person() { Name = "Me", X = 0, Y = -150 }
        };
        var obstacles = new List<engine.Common.Element>()
            {
                new Platform()
                {
                    X = 0,
                    Y = 120,
                    Width = 10000,
                    Height = 200,
                    Color = new RGBA() {R = 175, G = 175, B = 175, A = 255}
                }
            };
        var background = new Background(width, height)
        {
            GroundColor = new RGBA() { R = 150, G = 150, B = 255, A = 255 }
        };

        // add some obstacles to the left
        for (int x = -1000; x <= 1000; x += 100)
        {
            if (x == 0) continue;

            obstacles.Add(new Platform()
            {
                X = x,
                Y = -60,
                Width = 20,
                Height = 20,
                Color = new RGBA() { R = 175, G = 175, B = 175, A = 255 }
            });
        }

        World = new World(
            new WorldConfiguration()
            {
                Width = width,
                Height = height,
                EnableZoom = true,
                ShowCoordinates = false,
                ForcesApplied = (int)(Forces.Y)
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
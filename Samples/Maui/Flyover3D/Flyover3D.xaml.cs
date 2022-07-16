using engine.Common;
using engine.Common.Entities;
using engine.Common.Entities3D;
using engine.Maui;

namespace Flyover3D;

public partial class Flyover3D : ContentView
{
	public Flyover3D()
	{
		InitializeComponent();
	}

    public void InitializeSurface(int width, int height)
    {
        // basic green background
        var background = new Background(width, height) { GroundColor = new RGBA { R = 255, G = 255, B = 255, A = 255 } };
        // put the player in the middle
        var players = new Player[]
          {
                new Player3D() { Name = "YoBro", X = 0, Y = -75, Z = 1000 }
          };
        // any objects to interact with
        engine.Common.Element[] objects = new engine.Common.Element[100];
        var rand = new Random();
        for (int i = 0; i < objects.Length; i++)
        {

            // x = [-500 ... 600] (100 gap)
            // z = [0 ... 600] (100 gap)

            var x = ((i % 10) * 200) - 700;
            var z = (i / 10) * 200;

            switch (rand.Next() % 9)
            {
                case 0: objects[i] = new Cube() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 1: objects[i] = new Cone() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 2: objects[i] = new Cylinder() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 3: objects[i] = new Hexagon() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 4: objects[i] = new Pyramid() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 5: objects[i] = new Sphere() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 6: objects[i] = new Torus() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 7: objects[i] = new Tree() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
                case 8: objects[i] = new Wedge() { X = x, Y = 0, Z = z, Width = 100, Height = 100, Depth = 100, Wireframe = false }; break;
            }
        }
        var world = new World(
          new WorldConfiguration()
          {
              Width = width,
              Height = height,
              ShowCoordinates = true,
              HorizonX = 1000,
              HorizonY = 1000,
              HorizonZ = 1000,
              Is3D = true
          },
          players,
          objects,
          background
        );

        // start the UI painting
        UI = new UIHookup(this, world);

        // todo hide the cursor
    }

    #region private
    private UIHookup UI;
    #endregion
}
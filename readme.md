# Simple Game 'engine'

I was building games with my son and I needed a simple model to help quickly build simple games.  After the first several games this library took shape.  This is now a sub module for the games.

This engine was written for top-down 2d games and/or Board game within a Winforms shell.

## Getting Started

### Add this as a sub-module to your project

```
git submodule add https://github.com/speedyjeff/engine engine
git submodule init
git submodule update
```

### Samples
![samples](https://github.com/speedyjeff/engine/blob/master/Samples/Winforms/samplecontrols.png)

Check out [sample controls](/Samples) for examples on how to get started.

### Create a Winforms project

### Add engine initialization

#### 2d Platformer (top down)
```C#
private UIHookup UI;

private void InitializeComponent()
{
  ...
  this.Width = 1024;
  this.Height = 800;
  // setting a double buffer eliminates the flicker
  this.DoubleBuffered = true;

  // basic green background
  var width = 10000;
  var height = 800;
  var background = new Background(width, height) { GroundColor = new RGBA { R = 100, G = 255, B = 100, A =   255 } };
  // put the player in the middle
  var players = new Player[]
    {
      new Player() { Name = "YoBro", X = 0, Y = 0 }
    };
  // any objects to interact with
  Element[] objects = null;
  var world = new World(
    new WorldConfiguration()
    {
      Width = width,
      Height = height,
    }, 
    players, 
    objects,
    background
  );
  // start the UI painting
  UI = new UIHookup(this, world);
}  // InitializeComponent

protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
  UI.ProcessCmdKey(keyData);
  return base.ProcessCmdKey(ref msg, keyData);
} // ProcessCmdKey
```

#### Board Game
```C#
private UIHookup UI;

private void InitializeComponent()
{
  ...
  this.Width = 1024;
  this.Height = 800;
  // setting a double buffer eliminates the flicker
  this.DoubleBuffered = true;

  // creat the new board
  var board = new Board(new BoardConfiguration()
  {
    Width = 600,
    Height = 400,
    Rows = 4,
    Columns = 6,
    EdgeAngle = 30,
    Background = new RGBA() { R = 255, G = 255, B = 0, A = 255 }
  });
  board.OnCellClicked += Board_OnCellClicked;

  // link to this control
  UI = new UIHookup(this, board);
}  // InitializeComponent

protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
  UI.ProcessCmdKey(keyData);
  return base.ProcessCmdKey(ref msg, keyData);
} // ProcessCmdKey
```

#### World

<tbd>

```C#
public struct WorldConfiguration
{
  public int Width;
  public int Height;
  public bool CenterIndicator;
  public Menu StartMenu;
  public Menu EndMenu;
  public Menu HUD;
  public bool EnableZoom;
  public bool DisplayStats;
  public bool ShowCoordinates;
  public bool ApplyForces;
}

public struct ActionDetails
{
    public List<Element> Elements;
    public float AngleToCenter;
    public bool InZone;
    public float XDelta;
    public float YDelta;
    public float ZDelta;
    public float Angle;
}

public delegate bool BeforeKeyPressedDelegate(Player player, ref char key);

public event Func<Menu> OnPaused;
public event Action OnResumed;
public event Action<Player, Element> OnContact;
public event Action<Player, Element> OnAttack;
public event Action<Player> OnDeath;
public event BeforeKeyPressedDelegate OnBeforeKeyPressed;
public event Func<Player, char, bool> OnAfterKeyPressed;
public event Func<Player, ActionDetails> OnBeforeAction;
public event Func<Player, ActionEnum, bool> OnAfterAction;

public int Width { get; }
public int Height {  get; }
public int Depth { get; }

public Player Human { get; }
public int Alive { get; }

public void AddItem(Element item);
public void RemoveAllItems(Type type);
public void RemoveItem(Element item);
public void Play(string path);
public void Music(string path, bool repeat);
public void ShowMenu(Menu menu);
public void Teleport(Player player, float x, float y, float z);
```

There are a default set of key bindings which can be overridden by subscribing to OnBeforeKeyPressed.

Key(s) | Action (2d) | Action (3d)
-------|-------------|------------
`s` `S` `arrow:down` | y-axis + 1 | x-z axis relative to Angle
`a` `A` `arrow:left` | x-axis - 1 | x-z axis relative to Angle
`d` `D` `arrow:right` | x-axis + 1 | x-z axis relative to Angle
`w` `W` `arrow:up` | y-axis + 1 | x-z axis relative to Angle
`z` `Z` | z-axis - 1 | z-axis - 1
`c` `C` | z-axis + 1 | z-axis + 1
`1` | switch primary
`q` `Q` `0` `2` | drop primary
`r` `R` `mouse:middle click` | reload
`space` `mouse:left' | attack
`j` `J` | jump
`mouse:right` | x-y axis relative to Angle | x-z axis relative to Angle
`esc` | Show/dismiss menu

#### Board

```C#
public struct BoardConfiguration
{
  public int Width;
  public int Height;
  public RGBA Background;
  public string BackgroundImage;
  public int Rows;
  public int Columns;
  public int EdgeAngle; // 0 = rectangle, 1..89 = hexagon
}

public delegate void CellClickedDelegate(int row, int col, float x, float y);
public delegate void UpdateImageDelegate(IImage img);

public int Width { get; }
public int Height { get; }

public int Rows { get; }
public int Columns { get; }

public event CellClickedDelegate OnCellClicked;
public void UpdateCell(int row, int col, UpdateImageDelegate update);
public void UpdateOverlay(UpdateImageDelegate update);
```

#### Backgrounds
Backgrounds can provide a default look to the screen, but they also can inflict damage to Players and affect how fast a player may move.  Backgrounds can also adjust over time.

##### Methods to override

Override Draw to implement a custom background
```C#
public override void Draw(IGraphics g)
{
  g.Clear(GroundColor);
  base.Draw(g);
}
```

Override Update to make updates to the background during play
```C#
public virtual void Update()
{
  // (default) Every 100 ms the background has a chance to make an update
}
```

Override Pace to affect the players movement speed during play
```C#
public virtual float Pace(float x, float y)
{
  // return a value between 0.1f... with 1 being the default pace
  // the background can choose what portions of the background affect pace
  return 1;
}
```

Override Damage to cause damage to a player during play
```C#
public virtual float Damage(float x, float y)
{
  // return a value between 0...100 to deliver damage
  // the background can choose what portions of the background deliver damage
  return 0;
}
```

#### Players
Players can either accept user input (eg. they are the human) or they can implement `AI` can be controled by overriden methods.

##### Methods to override

Override Draw to implement a custom Player's look and feel
```C#
public override void Draw(IGraphics g)
{
  // will likely want to set ShowDefaultDrawing = false so you an draw your
  // own player
  base.Draw(g);
}
```

##### AI Methods to override
In addition to Draw, AI objects have the following methods

```C#
public enum ActionEnum 
{ 
  // no action
  None, 
  // switch your primary tool with the first in your secondary hand
  SwitchPrimary, 
  // pickup something on the ground
  Pickup, 
  // drop your primary tool
  Drop, 
  // reload (if you are holding a ranged weapon)
  Reload, 
  // attack with your primary tool
  Attack, 
  // move
  Move
};

public virtual ActionEnum Action(List<Element> elements, float angleToCenter, bool inZone, ref float xdelta, ref float ydelta, ref float zdelta, ref float angle)
{
  // [in] elements - A list of all elements within the window of view
  // [in] angleToCenter - An angle that points towards the center of the world
  // [in] inZone - An indicator if the background is delivering damage
  // [out] xdelta - Delta to move left(-) and right(+): Between -1...1 (where |xdelta|+|ydelta|+|zdelta|<=1)
  // [out] ydelta - Delta to move up(-) and down(+): Between -1...1 (where |xdelta|+|ydelta|+|zdelta|<=1)
  // [out] zdelta - Delta to move towards(+) and away (-1): Between -1...1 (where |xdelta|+|ydelta|+|zdelta|<=1)
  // [out] angle - Direction to point
  return ActionEnum.None;
}
```

```C#
public virtual void Feedback(ActionEnum action, object item, bool result)
{
  // [in] Action - Action taken
  // [in] item - Item picked up/dropped
  // [in] result - Indication if the action was successful
}
```

#### Objects
There are two primary types of objects sationary and acquirable.

##### Stationary Objects
Stationary objects should inherit from Obstacle.  These objects have concrete hitboxes which the player must move around.

##### Acquirable Objects
Acquirable objects should inherit from Tool.  These objects can be picked up by the players and used as weapons.

There are a few special types of Acquirable Objects:
* Ammo - Used by a ranged weapon
* Health - Increases the players health
* RangeWeapon - This is a weapon that can shot and hit a target from afar
* Shield - Increases the players shield

# Sample Controls

![samples](https://github.com/speedyjeff/engine/blob/master/samples/winforms/samplecontrol.png)

## Checkers
The checkers user control can easily be imported into your project and used as the UI for a checkers game.

Usage:
```C#
var checkers = new Checkers(width : 500, height : 500);
checkers.Board ...
```

## Catan
The Catan user control uses hexagonal cells to build games similar to Catan.  This user control can easily be imported into your project and used as the UI for a hexagonal game.

Usage:
```C#
var catan = new Catan(width : 500, height : 500);
catan.Board ...
```

## 2D Platformer
The 2D Platformer user control can easily be imported into your project and used as the UI for a side scrolling game.

Usage:
```C#
var platformer = new Platformer2D(width : 500, height : 500);
platformer.World ...
```

## Top down Platformer
The top down platformer user control can easily be imported into your project and used as the UI for a game where the vantage point is seen from the top down.

Usage:
```C#
var topdown = new TopDownPlatformer(width : 500, height : 500);
topdown.World ...
```

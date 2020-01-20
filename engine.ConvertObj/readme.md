# Conversion tool

Microsoft 3D Builder https://www.microsoft.com/en-us/p/3d-builder/9wzdncrfj3t6 can produce out that can be converted into 3D Elements for this game engine.  This tool converts the `.obj` format into code that can be used to add new 3D objects.

## Getting Started

```
./engine.ConvertObj <path to .obj file>
   -n <namespace>
   -x <angle to rotate around the x-axis (pitch)>
   -y <angle to rotate around the y-axis (yaw)>
   -z <angle to rotate around the z-axis (roll)>
Use Microsoft 3D Builder to create a scene and save in obj format
```

### Format

#### OBJ

Comments
```
# comments
```

Declare the start of an object's vertices
```
v x y z
```

List of faces described as indexs from the list of vertices above
```
f v1 v2 v3
```

Reference to an mtllib to describe the color of objects
```
mtllib name
```

Declare which object the following faces belong too
```
usemtl name
```

#### MTL

Comments
```
# comments
```

Declare a new named object
```
newmtl
```

Declare a color for this object
```
Kd 0..1 0..1 0..1
```
# Conversion tool

Blender https://apps.microsoft.com/detail/9PP3C07GTVRH?hl=en-us&gl=US&ocid=pdpshare can produce objects (in the .obj format) that can be converted into 3D Elements for this game engine.  (Previously Microsoft 3D Builder https://www.microsoft.com/en-us/p/3d-builder/9wzdncrfj3t6 was used, but that tool is now retired).  This tool converts the `.obj` format into code that can be used to add new 3D objects.

## Creating a new Object

Open Blender
Add a single object at the origin
Export the object as a Wavefront obj file setting the Triangulate Mesh option

## Getting Started

```
./engine.ConvertObj <path to .obj file>
   -n <namespace>
   -x <angle to rotate around the x-axis (pitch)>
   -y <angle to rotate around the y-axis (yaw)>
   -z <angle to rotate around the z-axis (roll)>
Use Blender to create a scene and save in Wavefront obj format
```

### Format

A full description can be found at https://en.wikipedia.org/wiki/Wavefront_.obj_file

#### OBJ

Comments
```
# comments
```

Declare the start of an object's vertices
```
v x y z
```

List of faces described as indexes from the list of vertices above
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
# MeshUtility

Mesh processing tool in Unity platform.

## Utilities

### MeshSeparator

Separate the target mesh into different categories based on given conditions.

Currently support BlendShape mesh separation. See [documentation](Documentation/notes/MeshSeparator.md) for more details.

<img src="Documentation/images/blendshape_separator.jpg" width="300">

### MeshIntegrator

Integrate all the meshes of a (prefab) GameObject.

### StaticMeshIntegrator

Integrate all the static meshes of a (prefab) GameObject (Root and its children).

### MeshNormalizer

Bake the Hierarchy. This is VRM normalization backend.
MeshNormalizer can do blendShape bake.

## Import MeshUtility

There are two ways to import MeshUtility into a Unity project.

### 1. Unity Package Manager (from Unity 2019)

You can add MeshUtility package via `UPM`. First click `Window` from menu on top, then select `Package Manager`.

<img src="Documentation/images/installation_1.jpg" width="200">

In `Package Manager`, click `Add package from git URL` and paste `https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders` and `https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF`

<img src="Documentation/images/installation_2.jpg" width="200">

Now check your project window. In `Packages`, MeshUtility should be included in the `UniGLTF` folder.

### 2. Add package name and its url in manifest.json

Another way of importing MeshUtility is manually adding necessary information in manifest.json, which is in the directory of `Package folder` in your Unity project. Open manifest.json with text editor and add the followings in `dependencies`:

```json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders",
    "com.vrmc.unigltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF",
  }
}
```

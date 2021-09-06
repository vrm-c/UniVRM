# Package Structure

| unitypackage       | folder                            | contents               |
|--------------------|-----------------------------------|------------------------|
| VRMShaders_UniGLTF | Assets/VRMShaders, Assets/UniGLTF | VRMShaders and UniGLTF |
| UniVRM             | Assets/VRM                        | VRM-0.X                |
| VRM                | Assets/VRM10                      | VRM-1.0(β)             |

```
+----------++----------+
|UniVRM-0.X||UniVRM-1.0|
+----------++----------+
+----------------------+
|       UniGLTF        |
+----------------------+
|      VRMShaders      |
+----------------------+
```

# UPM

```json
// manifest.json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.82.1",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.82.1", // rename unigltf to gltf
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.82.1",
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.82.1", // rename univrm1 to vrm
  }
}
```

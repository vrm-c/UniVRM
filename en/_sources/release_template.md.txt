## Release Notes

- [日本語](https://vrm-c.github.io/UniVRM/ja/release/${dir}/v${version}.html)
- [English](https://vrm-c.github.io/UniVRM/en/release/${dir}/v${version}.html)

## Installation

The UniVRM supports Unity ${unity_version} or later.

You can install UniVRM using the UnityPackage or the UPM Package.

### VRM 1.0 Import/Export

- via UnityPackage
  - **Download [VRM-${version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v${version}/VRM-${version_hash}.unitypackage)**

- via UPM Package
  - `com.vrmc.vrmshaders`
  - `com.vrmc.gltf`
  - `com.vrmc.vrm`

### VRM 0.x Import/Export

- via UnityPackage
  - **Download [UniVRM-${version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v${version}/UniVRM-${version_hash}.unitypackage)**

- via UPM Package
  - `com.vrmc.vrmshaders`
  - `com.vrmc.gltf`
  - `com.vrmc.univrm`

### UPM Package Information
You can install these UPM packages via Package Manager in UnityEditor.

- `Window` -> `Package Manager` -> `+` -> `Add package from git URL...`

| UPM package         | git URL                                                                 |
|:--------------------|:------------------------------------------------------------------------|
| com.vrmc.vrmshaders | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v${version} |
| com.vrmc.gltf       | https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v${version}    |
| com.vrmc.univrm     | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v${version}        |
| com.vrmc.vrm        | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v${version}      |

You can also install via editing `Packages/manifest.json` directly.

```json5
// Packages/manifest.json
{
  "dependencies": {
    // ...
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v${version}",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v${version}",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v${version}",
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v${version}",
    // ...
  }
}
```

## Development
### Samples
- via UnityPackage
  - VRM 1.0 Development Samples
    - [VRM_Samples-${version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v${version}/VRM_Samples-${version_hash}.unitypackage)
  - VRM 0.x Development Samples
    - [UniVRM_Samples-${version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v${version}/UniVRM_Samples-${version_hash}.unitypackage)
- via UPM Package
  - You can find `Samples` in the Package Manager and then submit `Import` button.
    - `Window` -> `Package Manager` -> `Packages: In Project` -> `VRM-1.0` or `VRM`

### Documentation

- [日本語](https://vrm-c.github.io/UniVRM/ja/vrm1/index.html)
- [English](https://vrm-c.github.io/UniVRM/en/vrm1/index.html)

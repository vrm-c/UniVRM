# Download

* for `Unity-2020.3 LTS` or later

* `for vrm-0.x` [UniVRM-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/UniVRM-{version_hash}.unitypackage)
* `for vrm-1.0` [VRM-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/VRM-{version_hash}.unitypackage)

ReleaseNote
* [日本語](https://vrm-c.github.io/UniVRM/ja/release/100/v{version}.html)
* [English](https://vrm-c.github.io/UniVRM/en/release/100/v{version}.html)

## other unitypackage

* `VRM-0.x API sample` [UniVRM_Samples-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/UniVRM_Samples-{version_hash}.unitypackage)
* `VRM-1.0 API sample` [VRM_Samples-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/VRM_Samples-{version_hash}.unitypackage)

## 1.0 development (1.0 開発ドキュメント)

- [日本語](https://vrm-c.github.io/UniVRM/ja/vrm1/index.html)
- [English](https://vrm-c.github.io/UniVRM/en/vrm1/index.html)

# UPM

| UPM package         | rename           | UPM url                                                                |
|---------------------|------------------|------------------------------------------------------------------------|
| com.vrmc.vrmshaders |                  | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v{version} |
| com.vrmc.gltf       | com.vrmc.unigltf | https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v{version}    |
| com.vrmc.univrm     |                  | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v{version}        |
| com.vrmc.vrm        | com.vrmc.univrm1 | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v{version}      |

```json
// manifest.json
{{
  "dependencies": {{
    ///
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v{version}",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v{version}",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v{version}",
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v{version}",
    ///
  }}
}}
```

# UPM packages and folders and unitypackage

| UPM package         | folder          | unitypackage                                                               | note |
|---------------------|------------------|------------------------------------------------------------------------|-|
| com.vrmc.vrmshaders | Assets/VRMShaders |  UniVRM-XXX, VRM-XXX | 0.x and 1.0 shared |
| com.vrmc.gltf       | Assets/UniGLTF |  UniVRM-XXX, VRM-XXX | 0.x and 1.0 shared |
| com.vrmc.univrm     | Assets/VRM  | UniVRM-XXX | import/export 0.x |
| com.vrmc.vrm        | Assets/VRM10 | VRM-XXX | import/export 1.0 and import 0.x |

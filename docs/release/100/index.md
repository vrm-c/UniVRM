# v0.100～ (Unity-2020.3) 最新版をご利用ください

## ReleaseNote

* `0.100.0` からサポートする Unity の最低バージョンを `2020.3LTS` に更新しました。
* `0.104.0` から `VRM-1.0` のパッケージ提供を開始しました。

```{toctree}
:glob:
:maxdepth: 1
 
v0.1*
```

## パッケージ(v0.100~)

| unitypackage       | folder                            | contents              |
|--------------------|-----------------------------------|-----------------------|
| VRMShaders_UniGLTF | Assets/VRMShaders, Assets/UniGLTF | VRMShaders と UniGLTF |
| UniVRM             | Assets/VRM                        | VRM-0.X               |
| VRM                | Assets/VRM10                      | VRM-1.0               |

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

## UPM(v0.100~)

```js
// manifest.json 抜粋
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.105.0",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.105.0",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.105.0",
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.105.0",
  }
}
```

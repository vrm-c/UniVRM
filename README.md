
* [日本語](README.ja.md)

# UniVRM

`UniVRM` is a Unity implementation of [VRM](https://vrm.dev/en/vrm_about/). It can create, import and export VRM models.

`VRM` is a file format for using 3d humanoid avatars (3D models) in VR applications.  
It is based on glTF2.0. Anyone is free to use it.

## Manual

* [UniVRM Manual](https://vrm.dev/en/docs/univrm/)
* [Unity Version](https://vrm.dev/en/docs/univrm/install/unity_version/)
* [UniVRM Version](https://vrm.dev/en/docs/univrm/install/univrm_version/)

## License

* [MIT License](./LICENSE.txt)

## Installation

From version `v0.80.0`, UniVRM supports `Unity-2019.4LTS` or later.

https://vrm.dev/en/docs/univrm/install/

### UnityPackage

Download from [Release](https://github.com/vrm-c/UniVRM/releases)

From `v0.81.0` UniVRM has 3 unitypackages.

|             | UniGLTF_VRMShaders | UniVRM  | VRM     |
|-------------|--------------------|---------|---------|
| for GLTF    | install            |         |         |
| for VRM-0.X | install            | install |         |
| for VRM-1.0 | install            |         | install |

## UPM(developer)

https://vrm.dev/en/docs/univrm/install/univrm_upm/

To upgrade/downgrade UniVRM version, for example, replace `v0.80.0` with any version.

From `v0.81.0` package rename, reversion.

Copy and paste the following git urls in `Packages/manifest.json` for UniVRM installation.  
```json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.81.0",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.81.0", // rename unigltf to gltf
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.81.0",
    // for VRM-1.0β
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.81.0", // rename univrm1 to vrm
  }
}
```

#### Script Samples

Enable sample on Unity `PackageManager` Window.

* `com.vrmc.univrm` and `com.vrmc.vrm` has samples.

* [Programming](https://vrm.dev/en/docs/univrm/programming/)
* [UniVRM Samples](https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM.Samples)
* [UniVRMTest](https://github.com/vrm-c/UniVRMTest)

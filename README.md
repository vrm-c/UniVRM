
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

|                       | UniGLTF_VRMShaders | VRM     | VRM1.0β |
|-----------------------|--------------------|---------|---------|
| for VRM               | install            | install |         |
| for VRM1              | install            |         | install |
| for both VRM and VRM1 | instlal            | install | install |
| only GLTF or Shader   | install            |         |         |

## UPM(developer)

https://vrm.dev/en/docs/univrm/install/univrm_upm/

Copy and paste the following git urls in `Packages/manifest.json` for UniVRM installation.  
To upgrade/downgrade UniVRM version, for example, replace `v0.80.0` with any version.

```json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.80.0",
    "com.vrmc.unigltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.80.0",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.80.0",
    // for VRM-1.0β
    "com.vrmc.univrm1": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.80.0",}
}
```

#### Script Samples

Enable sample on Unity `PackageManager` Window.

* `com.vrmc.univrm` and `com.vrmc.univrm1` has samples.

* [Programming](https://vrm.dev/en/docs/univrm/programming/)
* [UniVRM Samples](https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM.Samples)
* [UniVRMTest](https://github.com/vrm-c/UniVRMTest)

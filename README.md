
* [日本語](README.ja.md)

# UniVRM

https://github.com/vrm-c/UniVRM

`UniVRM` is a Unity implementation of [VRM](https://vrm.dev/en/vrm_about/). It can create, import and export VRM models.

* [Unity Version](https://vrm.dev/en/docs/univrm/install/unity_version/)
* [UniVRM Version](https://vrm.dev/en/docs/univrm/install/univrm_version/)
* [UniVRM Manual](https://vrm.dev/en/docs/univrm/)

`VRM` is a file format for using 3d humanoid avatars (3D models) in VR applications.  
It is based on glTF2.0. Anyone is free to use it.

The imported VRM model has the following Data:

* [Humanoid](https://vrm.dev/en/docs/univrm/humanoid/)
* [Meta](https://vrm.dev/en/docs/univrm/meta/)
* [Material](https://vrm.dev/en/docs/univrm/shaders/)
* [Expression](https://vrm.dev/en/docs/univrm/blendshape/)
* [Eyelook](https://vrm.dev/en/docs/univrm/lookat/)
* [Spring Bone](https://vrm.dev/en/docs/univrm/springbone/)
* [First Person](https://vrm.dev/en/docs/univrm/firstperson/)

## License

* [MIT License](./LICENSE.txt)

## Installation

https://vrm.dev/en/docs/univrm/install/

### Stable Version

Versions without bug fixes will be picked up as stable versions.  
To download the stable version, click the release tag (marked as `Latest`) as shown in the image below:

<img width=400 src=./right_latest.jpg>

1. Download ``UniVRM-0.xx.unitypackage``
1. Import ``UniVRM-0.xx.unitypackage`` into the Unity project

### Latest Version

The latest version is marked as `Pre-release`:

1. Go to the [releases page](https://github.com/vrm-c/UniVRM/releases)
1. Download the latest ``UniVRM-0.xx.unitypackage``
1. Import ``UniVRM-0.xx.unitypackage`` into the Unity project
   
## Script Samples

* [Programming](https://vrm.dev/en/docs/univrm/programming/)
* [UniVRM Samples](https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM.Samples)
* [UniVRMTest](https://github.com/vrm-c/UniVRMTest)

## Contributing to UniVRM

Use the commands below to clone UniVRM repository and then open the UniVRM folder by Unity.

```console
$ git clone https://github.com/vrm-c/UniVRM.git
$ cd UniVRM
# Update submodules such as MToon, etc.
$ git submodule update --init --recursive
```

See [contributing guidelines](https://github.com/vrm-c/UniVRM/wiki/Contributing-Guidelines).

## UPM

https://vrm.dev/en/docs/univrm/install/univrm_upm/

Copy and paste the following git urls in `Packages/manifest.json` for UniVRM installation.  
To upgrade/downgrade UniVRM version, for example, replace `v0.66.0` with `v0.71.0` or `v0.63.2`.

```
{
  "dependencies": {
    // ...
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.66.0",
    "com.vrmc.unigltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.66.0",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v0.66.0",
    // ...
}

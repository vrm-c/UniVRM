# UniGLTF-2.0

* Unity-Supports 2019.4 and later
* You can import and export glTF-2.0 in Unity's editor and runtime.
* implement `KHR_materials_unlit`
* implement `KHR_texture_transform` (partial)

# Material

## PBR

* Convert as much as possible between glTF pbr and Unity Standard Shader.
* Converts metal roughness and occlusion RGBA channel incompatibility.

## Unlit

* import: UniGLTF's own `UniGLTF/UniUnlit` shader.
* export: You can export `UniGLTF/UniUnlit` and Unity unilt materials.

* Only `UniGLTF/UniUnlit` supports vertex colors.

## URP

* import only. experimental

# License

* [MIT license](LICENSE)

# Download

* https://github.com/vrm-c/UniVRM/releases

# Screenshots

You can import almost all of [gltf_sample_models](https://github.com/KhronosGroup/glTF-Sample-Models/tree/master/2.0)

![standard shader](doc/pbr_to_standard.png)
![duck](doc/duck.png)
![animation](doc/animation.gif)
![SciFiHelmet](doc/SciFiHelmet.png)

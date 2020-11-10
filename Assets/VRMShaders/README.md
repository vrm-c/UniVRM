# VRMShaders

VRM model's supported shaders in Unity.

Shader と関連するユーティリティを切り離したパッケージ。

## 含まれるシェーダー

### UniUnlit

* Gltfの Unlit に適合するようにした。Unlit シェーダー

### MToon

* https://github.com/Santarh/MToon

## UniGLTF.ShaderPropExporter.PreShaderPropExporter

Unityでは、ランタイムにMaterialのPropertyを列挙することができない。
Set/Get はできる。
事前に一覧を作成するユーティリティ。

## UPM usage (Unity 2019.3.4f1~)

`Window` -> `Package Manager` -> `Add package from git URL` and paste `https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders`.

or add the package name and git URL in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders",
  }
}
```

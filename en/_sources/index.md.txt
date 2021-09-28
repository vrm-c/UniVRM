# UniVRM Programming Document

これは、開発者(UniVRMを使ったアプリケーションを作成する人)向けのドキュメントです。

UnityでVRMをエクスポートする方法などについては [manual](https://vrm.dev/docs/univrm/) を参照してください。

## 概要

```{toctree}
:maxdepth: 1

package.md
history.md
```

## glTF

```{toctree}
:maxdepth: 1

gltf/0_82_glb_import.md
gltf/how_to_impl_extension.md
gltf/0_36_update.md
```

## VRM

```{toctree}
:maxdepth: 1

vrm0/0_82_runtime_import.md
vrm0/0_79_runtime_import.md
vrm0/0_77_runtime_import.md
vrm0/0_68_runtime_import.md
vrm0/0_44_runtime_import.md
vrm0/0_58_blendshape.md
vrm0/firstperson.md
```

## 実装メモ

```{toctree}

implementation/runtime_resource_management.md
```

### Samples

- SimpleViewer
- RuntimeExporerSample
- FirstPersonSample
- AnimationBridgeSample

## VRM-1.0(β)

```{toctree}
:maxdepth: 2

vrm1/vrm1_runtime_load.md
vrm1/vrm1_get_humanoid.md
vrm1/vrm1_expression.md
vrm1/vrm1_lookat.md
vrm1/vrm1_firstperson.md
```

### Samples

- VRM10Viewer

# Indices and tables

* {ref}`genindex`
* {ref}`modindex`
* {ref}`search`

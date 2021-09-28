# Runtime のリソース管理について

Runtime Import で `Texture`, `Material`, `Mesh` などのリソースを Root の `GameObject.OnDestroy`
と同時に `UnityEngine.Object.Destroy` したい。

## RuntimeGltfInstance

`RuntimeGltfInstance.OnDestroy` で破棄を実行するようにした。

```
ImporterContext
    [Own]List<Mesh>
    AnimationClipFactory[Own]List<Animation>
    TextureFactory[Own]List<Texture>
    MaterialFactory[Own]List<Material>

👇  ImporterContext.LoadAsync
      RuntimeGltfInstance.AttachTo
        ImporterContext.TransferOwnership

RuntimeGltfInstance
    [Own]List<Mesh>
    [Own]List<Animation>
    [Own]List<Texture>
    [Own]List<Material>
```

> VRM-0.X では `RuntimeGltfInstance` にリソースを委譲している。

## Vrm10Instance

```
Vrm10Importer
    [Own]HumanoidAvatar
    [Own]Vrm10Object
    [Own]List<Vrm10Expression>

👇  Vrm10Importer.LoadAsync

RuntimeGltfInstance
    [Own]HumanoidAvatar
    [Own]VrmObject
    [Own]List<VrmExpression>
```

### FirstPerson

```
👇  Vrm10Instance.FirstPerson.SetupAsync

RuntimeGltfInstance
    [Own]List<Mesh>.Add(headless)
```

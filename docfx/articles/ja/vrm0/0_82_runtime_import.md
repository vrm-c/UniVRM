* `Version 0.82.0` `0.82.1` 以降を使ってください。
* `Version 0.82.1~`

Import時の Material 差し替え機能(URP対応)が実装されました。

以下の手順で import します。

1. VRMをパースして、VrmDataを得る
1. VrmImporter 作成して、 Unity のヒエラルキーをロードする
1. ロードされたインスタンスを使う(ShowMeshes)

# 1. Parse

`v0.82.1` VrmData が導入されます。

```cs
var data = new UniGLTF.GlbFileParser(path).Parse();
VrmData vrm = new VRM.VRMData(data);
```

# 2. Load

`v0.82.1` VrmData が導入されます。

```cs
var loader = new VRM.VRMImporterContext(vrm);
RuntimeGltfInstance instance = context.Load();
```

## URP 向けに `materialGenerator` を指定する

`materialGenerator` 引き数(省略可能)を指定することで URP マテリアルを生成するようにカスタムできます。
指定しない場合は `built-in` 向けのデフォルトが使用されます。

```cs
var loader = new VRM.VRMImporterContext(vrm, materialGenerator: new VRMUrpMaterialDescriptorGenerator(vrm.VrmExtension));
```

* まだ URP 向け MToonShader が作成されていないので、URP 向け `UniUnlit` にフォールバックします。

# 3. Instance
## ShowMeshes

`v0.77` ShowMeshes が loader から instance に移動しました。 

```cs
instance.ShowMeshes();
```

## Destroy

`v0.77` 破棄時に関連する Asset を破棄するようになりました。

```cs
GameObject.Destroy(instance);
```

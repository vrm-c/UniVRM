# ScriptedImporter の実装

`v0.68.0` 以降 の `glb/gltf editor importer` と `VRM-1.0 editor importer` の実装で使用している [ScriptedImporter](https://docs.unity3d.com/ja/2019.4/ScriptReference/Experimental.AssetImporters.ScriptedImporter.html) に関して。

> `VRM-1.0` の実装と拡張子がぶつかってしまうので `VRM-0.x` への `ScriptedImporter` 実装はしません

## ScriptedImporter 採用の利点

### Texture の Asset化 が楽

`AssetPostprocessor` だと普通の方法では実装できない Texture のバイト列を出力して Asset 化して、
これを参照する Material をアセット化するということが、
`ScriptedImporter.GetExternalObjectMap` により無理をせずに実装できます。

> UniVRM では抜け道として `EditorApplication.delayCall` を使用しています。

### Import 設定を作れる

`ScriptedImporter` を継承したクラスの `public member` や `[SerializeField]` に情報を保存できます。

```csharp
// 例
[ScriptedImporter(1, "cube")]
public class CubeImporter : ScriptedImporter
{
    public float m_Scale = 1; // これを Asset の Inspector で変更して Apply すると、新しい設定で再importできる

    // 省略
}
```

### ForceText 時に import が低速化するのを回避できる

`ScriptedImporter` の SubAsset にすることで、 `mesh` などがテキスト(yaml)で Asset 化されることを回避できます。
巨大なアセットで差が出ます。

## ScriptedImporter 実装

TODO: ExternalObjectMap の扱い

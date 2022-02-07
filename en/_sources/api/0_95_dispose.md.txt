# `v0.95` GltfData.Dispose

Importer の内部で `NativeArray` を使うようにしたため、
終了時にこれを破棄する必要ができました。
使い終わったら `Dispose` してください。

```cs
class GltfData: IDisposable
```

## 使用例

```cs
// must dispose GltfData
using (GltfData data = new AutoGltfFileParser(path).Parse())
using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: materialGenerator))
{
    return await loader.LoadAsync(awaitCaller);
}
```

## Dispose しなかった場合

NativeArray が Dispose されずに GC に回収されたタイミングで、
以下のエラーメッセージがコンソールに表示されます。

`A Native Collection has not been disposed`

このエラーがどこで起きたか分からない場合があります。
`com.unity.jobs package` により詳細メッセージを得ることができます。

<https://forum.unity.com/threads/a-native-collection-has-not-been-disposed-enable-full-stack.1098973/>

を参考にしてください。


## 関連

* <https://github.com/vrm-c/UniVRM/pull/1483>
* <https://github.com/vrm-c/UniVRM/pull/1503>

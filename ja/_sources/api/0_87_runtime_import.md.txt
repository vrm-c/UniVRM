# `v0.87` RuntimeImport 非同期ロード

## awaitCaller 引き数

```csharp
public virtual async Task<RuntimeGltfInstance> LoadAsync(IAwaitCaller awaitCaller = null, ...)
```

awaitCaller によりロード時の挙動をカスタマイズできます。

```{literalinclude} ../../Assets/VRMShaders/GLTF/IO/Runtime/AwaitCaller/IAwaitCaller.cs
    :language: csharp
    :linenos:
```

* NextFrame: 処理を中断して次のフレームで再開します。ロード処理が長い場合に長時間アプリケーションが固まることを防ぎます
* Run: UnityEngine.Object にアクセスしない処理を別スレッドで実行します。タスクが終了したら await で Unity Script スレッドで続きを実行します。

## ImmediateCaller

デフォルトでは、`ImmediateCaller` が使われます。
`ImmediateCaller` タスクを即時に実行するので、同期実行となります。

* Play(Editor Play, build), Editor(not play), UnitTest でデッドロックしないための実装です。

```{literalinclude} ../../Assets/VRMShaders/GLTF/IO/Runtime/AwaitCaller/ImmediateCaller.cs
    :language: csharp
    :linenos:
```

## RuntimeOnlyAwaitCaller

* Play(Editor Play, build) 時に非同期実行する実装です
* `VRM10_Samples/VRM10Viewer` に使用例があります。

```{literalinclude} ../../Assets/VRMShaders/GLTF/IO/Runtime/AwaitCaller/RuntimeOnlyAwaitCaller.cs
    :language: csharp
    :linenos:
```

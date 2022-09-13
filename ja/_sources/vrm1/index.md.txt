# VRM-1.0 の API

`vrm-0.103.1`

`VRM-1.0` ではフォーマットの更新とともに、 `Unity Component` が変わります。
また、`VRM-0.X` をロードして新しいコンポーネントで動かすことができます。
この場合 `VRM-0` ライセンスで扱ってください。

```csharp
RuntimeGltfInstance instance = await VrmUtility.LoadAsync(path);
```

👇

```csharp
Vrm10Instance vrm10Instance = await Vrm10.LoadPathAsync(path);
```

`VRM-0.x` から設計を変更して `Vrm10Instance` にすべての情報を格納する方式になりました。

```{admonition} Vrm10Instance
:class: info
Expression や LookAt, FirstPerson などまとめて入っています。
```

```{toctree}
:maxdepth: 2

vrm1_load
vrm1_controlrig
vrm1_expression
vrm1_lookat
vrm1_firstperson
```

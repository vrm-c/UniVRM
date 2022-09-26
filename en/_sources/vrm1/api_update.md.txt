# API更新

`VRM-1.0` ではフォーマットの更新とともに、 `Unity Component` が変わります。

## Load

`VRM-0.X` をロードして新しいコンポーネントで動かすことができます。
この場合 `VRM-0` ライセンスで扱ってください。

```csharpharp
RuntimeGltfInstance instance = await VrmUtility.LoadAsync(path);
```

👇

```csharpharp
Vrm10Instance vrm10Instance = await Vrm10.LoadPathAsync(path);
```

`VRM-0.x` から設計を変更して `Vrm10Instance` にすべての情報を格納する方式になりました。

```{admonition} 毎フレーム決まった順番で更新します。
:class: info

1. Control Rig
2. Constraints
3. Gaze control
4. Expression
            
<https://github.com/vrm-c/UniVRM/blob/master/Assets/VRM10/Runtime/Components/Vrm10Runtime/Vrm10Runtime.csharp#L170>
```

## Expression

```{admonition} VRMBlendShapeProxy は Vrm10Instance.Runtime.Expression になりました。
:class: info

ImmediatelySetValue と AccumulateValue は、SetWeight に一本化されました。
ImmediatelySetValue は無くなりました。
```

```csharp
var proxy = root.GetComponent<VRMBlendShapeProxy>();
proxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 0.5f);
```

👇

```csharp
var vrm10 = root.GetComponent<Vrm10Instance>();
vrm10.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), 0.5f);
```

## LookAt

```{admonition} VRMLookAt は Vrm10Instance.Runtime.LookAt になりました。
:class: info

vrm10.Gaze.position か vrm10.Runtime.LookAt.SetLookAtYawPitch で予め更新しておいた値が、
後で vrm10.Runtime により適用されます。
```

### Gaze

```csharp
var lookAt = root.GetComponent<VRMLookAt>();
lookAt.LookWorldPosition(new Vector3(x, y, z));
```

👇

```csharp
var vrm10 = root.GetComponent<Vrm10Instance>();
vrm10.LookAtTargetType = LookAtTargetTypes.CalcYawPitchToGaze;
vrm10.Gaze.position = new Vector3(x, y, z);
```

### SetYawPitch

```csharp
var lookAt = root.GetComponent<VRMLookAt>();
lookAt.ApplyRotations(yaw, pitch);
```

👇

```csharp
var vrm10 = root.GetComponent<Vrm10Instance>();
vrm10.LookAtTargetType = LookAtTargetTypes.SetYawPitch;
vrm10.Runtime.LookAt.SetLookAtYawPitch(yaw, pitch);
```

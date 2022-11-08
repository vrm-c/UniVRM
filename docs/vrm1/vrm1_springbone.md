# SpringBone

## FastSpringBoneService.Instance 
* FastSpringBoneService.Instance はシングルトンですべての VRM-1.0 モデルのスプリングをまとめて処理します

## `v0.106.0` 毎フレーム外力を加える

- [\# 1863](https://github.com/vrm-c/UniVRM/pull/1868)

```{admonition} 外力
:class: info

ジャンプや風など、一時的な力の表現を想定した機能です。
```

```csharp
VRM10Instance instance;

// each frame
// 既存の Gravity に加算されます
instance.Runtime.ExternalForce = new Vector3(0.1f, 0, 0);
```

## `v0.106.0` SpringBone の手動更新

```{admonition} 手動更新
:class: info

開始前に処理を回して SpringBone を安定させるなど、特殊用途向けです。
```

* FastSpringBoneService.UpdateTypes.Manual を追加
* FastSpringBoneService.ManualUpdate を追加

```csharp
// 管理している VRM-1.0 がすべて入っている
List<VRM10Instance> instances;

// setup
foreach(var instance in instances)
{
    // SpringBone を手動にするために、
    // VRM-1.0 本体も手動に変更している。
    // VRM本体 => SpringBone という処理順を守る。
    instance.UpdateType = UpdateTypes.None;
}
FastSpringBoneService.Instance.UpdateType = FastSpringBoneService.UpdateTypes.Manual;

// each frame
foreach(var instance in instances)
{
    // SpringBone よりも先に VRM10Instance を更新
    instance.Runtime.Process();
}
// 最後に FastSpringBoneService を更新
// すべての VRM-1.0 の SpringBone がまとめて処理されます。
FastSpringBoneService.Instance.ManualUpdate(time.deltaTime);
```

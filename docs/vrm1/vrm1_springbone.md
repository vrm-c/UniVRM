# SpringBone

## FastSpringBoneService.Instance 
* FastSpringBoneService.Instance はシングルトンですべての VRM-1.0 モデルのスプリングをまとめて処理します

## `v0.106.0` SpringBone の手動更新

* FastSpringBoneService.UpdateTypes.Manual を追加
* FastSpringBoneService.ManualUpdate を追加

```csharp
List<VRM10Instance> instances;

// setup
foreach(var instance in instances)
{
    instance.UpdateType = UpdateTypes.None;
}
FastSpringBoneService.Instance.UpdateType = FastSpringBoneService.UpdateTypes.Manual;

// each frame
// 先に VRM10Instance を更新
foreach(var instance in instances)
{
    instance.Runtime.Process();
}
// 最後に FastSpringBoneService を更新
FastSpringBoneService.Instance.ManualUpdate(time.deltaTime);
```

## `v0.106.0` 毎フレーム外力を加える

- [\# 1863](https://github.com/vrm-c/UniVRM/pull/1868)

```csharp
VRM10Instance instance;

// each frame
// 既存の Gravity に加算されます
instance.Runtime.ExternalForce = new Vector3(0.1f, 0, 0);
```

# `v0.106` SpringBone の手動更新

from {doc}`v0.106.0</release/100/v0.106.0>`

[\#1866](https://github.com/vrm-c/UniVRM/pull/1886)

* VRMSpringBone.SpringBoneUpdateType.Manual を追加
* VRMSpringBone.ManualUpdate を追加

以下のように呼び出すことができます。

```csharp
VRMSpringBone spring;

// setup
spring.m_updateType = VRMSpringBone.SpringBoneUpdateType.Manual;

// each frame
spring.ManualUpdate(time.deltaTime);
```

* spring.ManualUpdate を使う前に spring.m_updateType を `Manual` に設定する必要があります。

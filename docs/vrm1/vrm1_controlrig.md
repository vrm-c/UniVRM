# ControlRig 正規化されていないモデルを操作する

VRM-1.0 は正規化が仕様から除かれました。

```{admonition} 正規化
:class: info

ヒエラルキーからの 回転、スケールの除去。
その状態での Binding 行列再生成。
です。
すべてのノードの回転が 0 のときが初期姿勢(T-Pose)であるという仕様で、
プログラムから統一的にモデルを操作することが可能でした。
```

正規化されていないモデルも含めて統一的にポーズを付けるインターフェスとして、 `ControlRig` が新規に導入されました。 `v0.103`

基本的な使い方は下記のとおりです。
`UnityEngine.Animator.GetBoneTransform` の代わりに使う想定です。

```csharp
Vrm10RuntimeControlRig rig = instance.Runtime.ControlRig

// 使用例
var transform = rig.GetBoneTransform(HumanBodyBones.Head);
transform.localRotation = rotation;
```

```{admonition} ランタイムロード専用
:class: warn

`v0.103` 現在この機能は Editor で Asset 生成されたモデルでは動作しません。
初期姿勢を確実に取得する方法を検討中です。
```

vrm-0.x でプログラムから HumanoidBone に回転クォータニオンを代入していた
場所を `Vrm10RuntimeControlRig.GetBoneTransform` で置きかえることで
同じポーズを当てることができます。

## 動作の説明

毎フレーム `Vrm10Instance` が `Vrm10RuntimeControlRig` からVRM-1.0のヒエラルキーにポーズを転送します。

`Vrm10RuntimeControlRig` が初期姿勢を記憶していて、
`正規化ポーズ localRotation => 初期姿勢を加味 => 元のヒエラルキーの localRotation` に変換という処理をします。

初期姿勢は `T-Pose` ですが、 `VRM-0.X` のように正規化されている必要がありません。

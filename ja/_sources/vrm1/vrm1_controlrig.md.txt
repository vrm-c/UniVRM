# ControlRig 正規化されていないモデルを操作する

VRM-1.0 は正規化が仕様から除かれました。

```{admonition} 正規化
:class: info

正規化とは、ヒエラルキーからの 回転、スケールの除去。
その状態での Binding 行列再生成。
です。

すべてのノードの回転が 0 のときが初期姿勢(T-Pose)であるという仕様で、
プログラムから統一的にモデルを操作することが可能でした。
```

`v0.103` 正規化されていないモデルも含めて統一的にポーズを付けるインターフェスとして、 `ControlRig` が新規に導入されました。 

> Vrm10RuntimeControlRig.GetBoneTransform が導入されました。`v0.104` で Animator.getBoneTransform が
使えるようになったので特に使う必要が無くなりました。

`v0.104` `UnityEngine.Animator.getBoneTransform` が ControlRig のボーンを返すようになりました。

```{admonition} HumanoidAvatar の材料に ControlRig のボーンを使う
:class: info

ControlRigGenerationOption.Generate の時は、AvatarBuilder.BuildHumanAvatar の引き数にオリジナルのヒエラルキーでは無く、 ControlRig のボーンを渡します。
```

## ControlRig は ランタイムロード時に生成されます

```{admonition} ランタイムロード専用
:class: warning

`v0.103` 現在この機能は Editor で Asset 生成されたモデルでは動作しません。
VRMモデルをセットアップするときに邪魔になってしまうので、Editor では生成しないようにしています。
```

デフォルトで ControlRig を生成 `ControlRigGenerationOption.Generate` します。
`ControlRigGenerationOption.None` は ControlRig を生成しません。

```csharp
public static async Task<Vrm10Instance> Vrm10.LoadPathAsync(
    string path,
    bool canLoadVrm0X = true,
    ControlRigGenerationOption controlRigGenerationOption = ControlRigGenerationOption.Generate, // 👈
    bool showMeshes = true,
    IAwaitCaller awaitCaller = null,
    IMaterialDescriptorGenerator materialGenerator = null,
    VrmMetaInformationCallback vrmMetaInformationCallback = null,
    CancellationToken ct = default)
```

`ControlRigGenerationOption.Generate` でロードしたモデルは、 `Animator.getBoneTransform` が
ControlRig の該当ボーンを返します。
オリジナルのボーンを取得する方法は後述します。

## ControlRig でないオリジナルのボーンを取得する方法

`Vrm10Instance.Humanoid.GetBoneTransform` を使ってください。

## ControlRig によるポーズ適用例

正規化済みの bvh ヒエラルキーの `Animator src` のポーズを、
未正規化かもしれない `vrm-1.0` モデルにコピーする例です。

各ボーンの localRotation を代入するだけで動きます。

```csharp
// VRM10_Samples/VRM10Viewer(v0.104) からの抜粋
/// <summary>
/// from v0.104
/// </summary>
/// <param name="src"></param>
public void UpdateControlRigImplicit(Animator src)
{
    var dst = m_controller.GetComponent<Animator>();

    foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
    {
        if (bone == HumanBodyBones.LastBone)
        {
            continue;
        }

        // v0.104 から Animator.GetBoneTransform が
        // ControlRig のボーンを返します。
        var boneTransform = dst.GetBoneTransform(bone);
        if (boneTransform == null)
        {
            continue;
        }

        var bvhBone = src.GetBoneTransform(bone);
        if (bvhBone != null)
        {
            // set normalized pose
            boneTransform.localRotation = bvhBone.localRotation;
        }

        if (bone == HumanBodyBones.Hips)
        {
            // TODO: hips position scaling ?
            boneTransform.localPosition = bvhBone.localPosition;
        }
    }
}
```

## 詳細

```{figure} ./ControlRig.png
ControlRig
```

毎フレーム `Vrm10Instance` が `Vrm10RuntimeControlRig` からVRM-1.0のヒエラルキーにポーズをコピーします。
コピーするときに、各関節の回転を初期姿勢を加味したものに加工しています。

`GlobalInit.Inverse * localPose * GlobalInit` という式がロジックです。
これにより、正規化済みのポーズ(localPose) を非正規化ポーズに変換できます。

`Vrm10ControlBone.cs`

```csharp
        internal void ProcessRecursively()
        {
            ControlTarget.localRotation = _initialTargetLocalRotation * Quaternion.Inverse(_initialTargetGlobalRotation) * ControlBone.localRotation * _initialTargetGlobalRotation;
            foreach (var child in _children)
            {
                child.ProcessRecursively();
            }
        }
```

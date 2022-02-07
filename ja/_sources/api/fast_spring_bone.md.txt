# `v0.85` FastSpringBoneについて

## 概要
UniVRMでは、DOTSを利用した高速なSpringBone実装である「FastSpringBone」を用意しています。

揺れ物の各房を並列処理・最適化することで1フレームあたりの処理時間を大幅に抑えます。

VRM0.x と VRM1.0 で、それぞれFastSpringBoneの立ち位置・実装が異なります。

## VRM 1.0 での FastSpringBone の概要
VRM1.0ではFastSpringBoneが揺れものの標準実装です。

VRMのライフサイクルに紐付いて自動的に生成・破棄されます。

## VRM 0.x での FastSpringBone の概要
VRM0.xでは後方互換性を保つため、デフォルトでは従来のDOTS実装でないSpringBoneで動作します。 

VRM0.x向けのFastSpringBone実装は `VRM/Runtime/FastSpringBone` ディレクトリにあります。

## VRM 0.x での FastSpringBone の導入方法
差し替えを行いたいタイミングで `FastSpringBoneReplacer.ReplaceAsync` を呼び出してください

これを明示的に呼ばなければ、従来のSpringBoneのまま動作します。

```csharp
using (var loader = new UniGLTF.ImporterContext(data))
{
    var instance = await loader.LoadAsync();
    SetModel(instance);
}
FastSpringBoneReplacer.ReplaceAsync(instance.Root)

instance.EnableUpdateWhenOffscreen();
instance.ShowMeshes();
```

## Burstの導入について
UniVRM に加えて Burst を別途導入すると、 FastSpringBone が Burst によって高速化されます。

Burst の導入方法は [こちら](https://docs.unity3d.com/ja/2019.4/Manual/upm-ui-install.html) をご参照ください。

## FastSpringBoneServiceについて
FastSpringBone が実行されると、`FastSpringBone Service` GameObject が `DontDestroyOnLoad` で生成されます。

これは全 VRM の FastSpringBone を集め、バッファの構築や、 FastSpringBone の実行タイミングの制御などを行う GameObject です。

明示的に破棄を行いたい場合は `FastSpringBoneService.Free` を呼んでください。

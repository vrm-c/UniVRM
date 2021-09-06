---
title: BlendShapeProxyの使い方
date: 2018-04-16T16:30:00+09:00
aliases: [
    "/dev/univrm-0.xx/programming/univrm_use_blendshape/",
    "/univrm/programming/how_to_use_blendshapeproxy/",
    ]
weight: 3
tags: ["api"]
---
## 環境
UniVRM v0.58.0

## 使用するメソッド

* [推奨] `SetValues`
* [非推奨] `ImmediatelySetValue`
* [上級者向け] `AccumulateValue`
* [上級者向け] `Apply`

## スクリプトから BlendShape weight を適用する

`SetValues` 関数のみを使用します。
そのフレームで必要な表情の weight 値をすべて集めてから `SetValues` を 1 回だけ呼んで設定します。

{{< highlight cs >}}
var proxy = GetComponent<VRMBlendShapeProxy>();

proxy.SetValues(new Dictionary<BlendShapeKey, float>
{
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 1f}, // [0, 1] の範囲で Weight を指定
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 1f}, // システム定義の表情は enum で指定
    {BlendShapeKey.CreateUnknown("USER_DEFINED_FACIAL"), 1f}, // ユーザ定義の表情は string で指定
});
{{< / highlight >}}

## 複数の BlendShape weight を適用する際の競合の問題について

この節では、なぜ `SetValues` を使わなければならないのかという疑問に回答します。

たとえば 2 つの VRMBlendShape `Blink_L` と `Blink_R` が

VRMBlendShape `Blink_L`

* Mesh `A` の Blendshape `eye_close_L` の weight 値 `100`
* Mesh `A` の Blendshape `eye_close_R` の weight 値 `1`

VRMBlendShape `Blink_R`

* Mesh `A` の Blendshape `eye_close_L` の weight 値 `1`
* Mesh `A` の Blendshape `eye_close_R` の weight 値 `100`

で定義されているとします。
このとき両目を閉じたいモチベーションから、両方を有効にする意図で下記のように実行します。

{{< highlight cs >}}
proxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f);
proxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f);
{{< / highlight >}}

すると、左目だけが開いてしまいます。
これは後から `ImmediateSetValue` した `Blink_R` が `Blink_L` と競合して weight を上書きしてしまうからです。
したがって VRM の表情制御においては下記の 2 通りのどちらかの方法で書くことが求められます。
これらの方法はこの競合の問題を解決して表情を設定することができます。

{{< highlight cs >}}
proxy.SetValues(new Dictionary<BlendShapeKey, float>
{
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f},
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f},
});
{{< / highlight >}}

または

{{< highlight cs >}}
proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f); // すぐに適用せずにたくわえる
proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f);
proxy.Apply(); // 蓄積した値をまとめて適用する
{{< / highlight >}}

WIP

## 何故、複数のSetterがあるのか

* LipSync
* 瞬き
* 視線制御(BlendShapeで視線を動かすタイプのモデル)
* プログラムによる喜怒哀楽

上記のような複数のBlendShapeが別々のコンポーネントから設定された場合に、
BlendShape同士が競合することがわかりました。
後で設定した値で上書きされて希望のBlendShapeが適用されないという状態になります。
これを解決するために、一か所で中央集権的に制御する必要があります。

合成したり排他制御した、BlendShapeClipの集合のスナップショットをまとめて適用することを想定して `SetValues`

## ImmediatelySetValue

簡単なテストプログラムでの利用を想定しています。

例：

```cs
var proxy = GetComponent<VRMBlendShapeProxy>();

proxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 1.0f);
```

## AccumulateValue + Apply

例：

```cs
var proxy = GetComponent<VRMBlendShapeProxy>();

proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f); // すぐに適用せずにたくわえる
proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f);
proxy.Apply(); // 蓄積した値をまとめて適用する
```

下記のSetValuesを推奨しています。

## SetValues

BlendShape合成器が必要に応じ呼び出すことを想定しています。

例：

```cs
var proxy = GetComponent<VRMBlendShapeProxy>();

proxy.SetValues(new Dictionary<BlendShapeKey, float>
{
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f},
    {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f},
});
```

---
title: 🚧Runtime ロード
weight: 10
---

VRM-1.0 を使うアプリケーションは、 `VRM-0.x` の資産もロードしたいはずです。

`VRM-1.0` と `VRM-0.X` の meta の非互換に [Metaの自動的なマイグレーションは禁止する方針](https://github.com/vrm-c/vrm-specification/issues/181) となりました。

## VRM-0.X を VRM-1.0 コンポーネントに対してロードする

`UniVRM-1.0` では、
`VRM-0.X` のライセンスを保持したまま、`VRM-1.0` のコンポーネントにロードする機能を提供します(予定)。

アプリケーションは、この方法でロードしたモデルは、`VRM-0.X` ライセンスとして扱ってください。

## RuntimeLoad 例

`vrm-0.x` と `vrm-1.0` 両方をロードしてライセンスを取得する例。

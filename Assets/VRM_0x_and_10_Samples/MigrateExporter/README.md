# MigrateExporter

- for runtime
- Convert export vrm0 model directly to vrm1
- input: vrm0 hierarchy + vrm1 meta
- output: vrm1 binary

## Detail

最初に vrm0 モデルをエクスポートします。
このとき後で vrm1 化する前提で通常とは別の設定にします。

- 正規化しない
- z+ もしくは 左手系のまま進める
- divided vertex buffer(glb 向けに実装済み)

得られた、vrm0 バイナリを migrate します。
このとき、vrm1 の meta を外部から注入することで
ユーザー入力無しでの migration で `完全な` vrm1 を出力します。

結果に対して vrm1 バリデーション。

- https://github.com/vrm-c/glTF-Validator/pull/1

## Caution

対象の `vrm-0.x` のライセンスを変更する権利が必要です。

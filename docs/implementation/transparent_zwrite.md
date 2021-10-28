# UnlitTransparentZWrite

対応状況。

| UniVRM                              | export | import | comment                                                                                                              |
|-------------------------------------|--------|--------|----------------------------------------------------------------------------------------------------------------------|
|                                     | ✅      | ✅      | GLTFの仕様外で MToon でもないので仕様の無い存在に。                                                                  |
| {doc}`v0.44 </release/055/v0.44>`   | ❌      | ✅      | MToon のみを extensions.VRM に記述し、 `PBR` と `Unilt` は GLTF として記録する。                                     |
| {doc}`v0.76 </release/068/v0.76.0>` | ❌      | ❌      | [\#1004](https://github.com/vrm-c/UniVRM/pull/1004) 仕様外のため削除。import すると MToon になるが調整されていない。 |
| {doc}`v0.85 </release/079/v0.85.0>` | ❌      | 🔺     | [\#1248](https://github.com/vrm-c/UniVRM/pull/1248) import すると MToon になる。                                     |
| {doc}`v0.88 </release/079/v0.88.0>` | ❌      | ✅      | [\#1331](https://github.com/vrm-c/UniVRM/pull/1331) MToon 変換を見直し。                                             |

* `UniVRM-0.75` 以前はロードできます。
* `UniVRM-0.88` 以降は見た目が同様になるように MToon に変換されます。
* 新規に `ZWriteTransparent` にしたい場合は、 `MToon` を使用してください。

## MToon を Unlit 的に設定する

Emission を使うのがこつです。

- _MainTex: 元の _MainTex
- _Color: (0, 0, 0, 1)
- _ShadeTex: null
- _ShadeColor: (0, 0, 0, 1)
- _EmissiveMap: 元の _MainTex
- _Emission: (1, 1, 1, 1)

# UniGltfPbr ShaderGraph 仕様

glTF 2.0 準拠 PBR (metallic-roughness) を ShaderGraph で実装するための仕様。
Importer/Exporter (`UniGLTF.InvariantRpUniGltfPbrMaterialImporter` / `InvariantRpUniGltfPbrMaterialExporter`)
とこの ShaderGraph は、`UniGLTF.UniGltfPbrContext` を介して以下のプロパティ契約で結合している。

- シェーダ名 (契約): `UniGLTF/UniGltfPbr` (`UniGltfPbrContext.ShaderName`)
- 配置: `Packages/UniGLTF/UniGltfPbr/Shaders/UniGltfPbr.shadergraph`
- 解決: `UniGltfPbrContext.GetShader()` = `Shader.Find("UniGLTF/UniGltfPbr")`

## 使い方 (インポート先シェーダの切替)

既定のインポート先 (`PbrMaterialImportType.UnityStandard`) は従来どおり
Standard (Built-In) / Universal Render Pipeline/Lit (URP) で、動作は変わらない。
`PbrMaterialImportType.GltfCompatible` を指定すると、RenderPipeline 非依存の
`InvariantGltfMaterialDescriptorGenerator` が選択され、PBR マテリアルがこのシェーダになる。

- Editor Import: gltf/glb インポータのインスペクタで `Pbr Material Import Type` を `GltfCompatible` に設定
- Runtime Import: `MaterialDescriptorGeneratorUtility.GetValidGltfMaterialDescriptorGenerator(PbrMaterialImportType.GltfCompatible)`
  で生成した生成器を `GltfUtility.LoadAsync(path, awaitCaller, materialGenerator)` の `materialGenerator` に渡す
- Export: `UniGLTF/UniGltfPbr` のマテリアルは Built-In / URP どちらのエクスポータでも glTF に書き出せる

### Player ビルドでの注意 (Runtime Import)

Player ビルドでは、どのアセットからも参照されないシェーダはビルドに含まれず
`Shader.Find` が null を返す。Runtime Import で `GltfCompatible` を使う場合は
`Project Settings > Graphics > Always Included Shaders` に `UniGLTF/UniGltfPbr` を登録すること
(unlit マテリアルを含む場合は従来どおり `UniGLTF/UniUnlit` も必要)。
ビルドサイズに敏感なプロジェクトでは、Resources 配下の ShaderVariantCollection で
必要なバリアントだけを含める方法もある。

## Targets

1 つの `.shadergraph` に 2 Target を追加する:

- **Universal (URP) → Lit**
- **Built-In → Lit**

両 Target 共通設定:

- **Allow Material Override = ON** (Surface / Blend / Cull / Alpha Clip をマテリアルプロパティ化)
- **Alpha Clipping = ON** (MASK 用。閾値に `_alphaCutoff` を接続する)
- Surface 既定は Opaque (import 側で alphaMode に応じて上書きする)
- Custom Editor GUI: `UniGLTF.UniGltfPbrShaderGUI`
- プロジェクトは **Linear color space** 前提

## Blackboard プロパティ (Reference 名は厳密一致させること)

| Reference | 型 / モード | 既定値 | 備考 |
|---|---|---|---|
| `_baseColorFactor` | Color / **Default** (sRGB) | white | import は linear の .gamma を格納 |
| `_baseColorTexture` | Texture2D | White | **Use Tiling And Offset = ON**, **Set as Main Texture = ON** |
| `_metallicFactor` | Float / Slider [0,1] | 1 | |
| `_roughnessFactor` | Float / Slider [0,1] | 1 | |
| `_metallicRoughnessTexture` | Texture2D | White (Linear) | 生 ORM。G=roughness, B=metallic |
| `_occlusionTexture` | Texture2D | White (Linear) | R=occlusion。MR と同一テクスチャの場合あり |
| `_occlusionStrength` | Float / Slider [0,1] | 1 | |
| `_normalTexture` | Texture2D / **Bump** | Bump | |
| `_normalScale` | Float | 1 | |
| `_emissiveFactor` | Color / **HDR** | black | linear 直格納 |
| `_emissiveTexture` | Texture2D | White | |
| `_emissiveStrength` | Float | 1 | KHR_materials_emissive_strength |
| `_alphaCutoff` | Float / Slider [0,1] | 0.5 | AlphaClipThreshold に接続 |

> マップ既定は白 (factor のみ指定時に効くように)。`_emissiveFactor` のみ黒。
>
> **全テクスチャで Use Tiling And Offset = ON**。Importer/Exporter は各テクスチャ固有の `KHR_texture_transform` (offset/scale) を per-texture で往復する。`_baseColorTexture` のみ **Set as Main Texture = ON**。

## Fragment ノード演算 (この計算が Runtime Conversion を不要にする)

- BaseColor = `_baseColorFactor`.rgb × SampleTexture2D(`_baseColorTexture`).rgb
- Alpha = `_baseColorFactor`.a × SampleTexture2D(`_baseColorTexture`).a
- Metallic = `_metallicFactor` × mr.**b**
- Smoothness = OneMinus(`_roughnessFactor` × mr.**g**)  ← roughness→smoothness を per-fragment で変換
- Ambient Occlusion = Lerp(1, occ.**r**, `_occlusionStrength`)
- Normal (Tangent Space) = NormalStrength(UnpackNormal(SampleTexture2D(`_normalTexture`, Type=Normal)), `_normalScale`)
- Emission = `_emissiveFactor`.rgb × `_emissiveStrength` × SampleTexture2D(`_emissiveTexture`).rgb
- Alpha Clip Threshold = `_alphaCutoff`

mr = SampleTexture2D(`_metallicRoughnessTexture`) (Linear), occ = SampleTexture2D(`_occlusionTexture`) (Linear)

> パイプライン非依存の ORM 演算 (Metallic / Smoothness / AO) は SubGraph `UniGltfPbr_SubGraph.shadersubgraph` に括り出して両 Target で共有すると保守しやすい。

## 意図的な非対応 (既存 Standard/URP-Lit 経路と揃える)

- 頂点カラー (COLOR_0) 乗算はしない (既定経路も無視するため)

（`KHR_texture_transform` は全テクスチャ per-texture で対応。上記「プロパティ契約」の注記を参照）

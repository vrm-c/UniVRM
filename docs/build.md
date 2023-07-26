# アプリケーションのビルド

UniVRMを使うアプリケーションのビルドに関する注意事項

## ビルドに含めるシェーダー

`Project Settings = Graphics - Always Included Shaders` などに設定して、以下のシェーダーがビルドに含まれるようにしてください。

### URP

#### Standard

* `Universal Render Pipeline/Lit`

#### Unlit

* `Assets\VRMShaders\GLTF\UniUnlit\Resources\UniGLTF\UniUnlit.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

#### MToon

* `Assets/VRMShaders/VRM10/MToon10/Resources/VRM10/vrmc_materials_mtoon_urp.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

### builtin

#### Standard

* `Standard`

GLTF の PBR マテリアルが使用します。

```{admonition} Always Included Shaders
:class: warning
明示的に指定する必要があります
```

#### Unlit

* `Assets\VRMShaders\GLTF\UniUnlit\Resources\UniGLTF\UniUnlit.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

#### MToon

* `Assets\VRMShaders\VRM\MToon\MToon\Resources\Shaders\MToon.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

## テクスチャー変換用のシェーダー

import/export 時に使用します。

:::{note}
ビルトイン/URP の両方で使われます
:::

* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\NormalMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapExporter.shader`
* `Assets\VRMShaders\GLTF\IO\Resources\UniGLTF\StandardMapImporter.shader`

```{admonition} Always Included Shaders
`Resources` に配置しているので明示的に指定しなくてもビルドに含まれます。
```

## URP package が必要です

未インストールの場合は PackageManager から URP package をインストールしてください

<img height=300 src=https://github.com/vrm-c/UniVRM/assets/68057/a48816d7-7db2-469e-b762-a0951fa8a670>

**エラーメッセージ**

<details>
Shader error in 'VRM10/Universal Render Pipeline/MToon10': Couldn't open include file 'Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl'. at /ghq/github.com/ousttrue/UniVRM-UPM/Library/PackageCache/com.vrmc.vrmshaders@b4130b9e5d/VRM10/MToon10/Resources/VRM10/vrmc_materials_mtoon_render_pipeline.hlsl(5)
Compiling Subshader: 0, Pass: UniversalForward, Vertex program with DIRECTIONAL
Platform defines: SHADER_API_DESKTOP UNITY_ENABLE_DETAIL_NORMALMAP UNITY_ENABLE_REFLECTION_BUFFERS UNITY_LIGHTMAP_FULL_HDR UNITY_LIGHT_PROBE_PROXY_VOLUME UNITY_PBS_USE_BRDF1 UNITY_SPECCUBE_BLENDING UNITY_SPECCUBE_BOX_PROJECTION UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS
Disabled keywords: FOG_EXP FOG_EXP2 FOG_LINEAR INSTANCING_ON LIGHTMAP_SHADOW_MIXING LIGHTPROBE_SH SHADER_API_GLES30 SHADOWS_SCREEN SHADOWS_SHADOWMASK UNITY_ASTC_NORMALMAP_ENCODING UNITY_COLORSPACE_GAMMA UNITY_ENABLE_NATIVE_SHADOW_LOOKUPS UNITY_FRAMEBUFFER_FETCH_AVAILABLE UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS UNITY_HARDWARE_TIER1 UNITY_HARDWARE_TIER2 UNITY_HARDWARE_TIER3 UNITY_LIGHTMAP_DLDR_ENCODING UNITY_LIGHTMAP_RGBM_ENCODING UNITY_METAL_SHADOWS_USE_POINT_FILTERING UNITY_NO_DXT5nm UNITY_NO_FULL_STANDARD_SHADER UNITY_NO_SCREENSPACE_SHADOWS UNITY_PBS_USE_BRDF2 UNITY_PBS_USE_BRDF3 UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION UNITY_UNIFIED_SHADER_PRECISION_MODEL UNITY_VIRTUAL_TEXTURING _ADDITIONAL_LIGHTS _ADDITIONAL_LIGHTS_VERTEX _ALPHABLEND_ON _ALPHATEST_ON _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MTOON_EMISSIVEMAP _MTOON_PARAMETERMAP _MTOON_RIMMAP _NORMALMAP
</details>

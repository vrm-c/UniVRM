using System;
using System.Collections.Generic;
using MToon;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UniJSON;
using UnityEngine;
using VRMShaders.VRM10.MToon10.Runtime;
using ColorSpace = VRMShaders.ColorSpace;
using OutlineWidthMode = MToon.OutlineWidthMode;
using RenderMode = MToon.RenderMode;

namespace UniVRM10
{
    internal static class MigrationMToonMaterial
    {
        public static void Migrate(glTF gltf, JsonNode vrm0)
        {
            // Create MToonDefinition(0.x) from JSON(0.x)
            var sourceMaterials = new (Vrm0XMToonValue, glTFMaterial)[gltf.materials.Count];
            for (int i = 0; i < gltf.materials.Count; ++i)
            {
                var vrm0XMaterial = vrm0["materialProperties"][i];
                if (MigrationMaterialUtil.GetShaderName(vrm0XMaterial) == "VRM/MToon")
                {
                    sourceMaterials[i] = (new Vrm0XMToonValue(vrm0XMaterial), gltf.materials[i]);
                }
                else
                {
                    // NOTE: MToon ではない場合、マイグレーション先に書き込まない.
                    sourceMaterials[i] = (null, null);
                }
            }

            // Collect RenderQueues Pass
            // 元の描画順序をできるだけ保つようにして RenderQueue を変換する
            var transparentRenderQueues = new SortedSet<int>();
            var transparentZWriteRenderQueues = new SortedSet<int>();
            foreach (var (mtoon, gltfMaterial) in sourceMaterials)
            {
                if (mtoon == null)
                {
                    continue;
                }
                switch (mtoon.Definition.Rendering.RenderMode)
                {
                    case RenderMode.Opaque:
                        break;
                    case RenderMode.Cutout:
                        break;
                    case RenderMode.Transparent:
                        transparentRenderQueues.Add(mtoon.Definition.Rendering.RenderQueueOffsetNumber);
                        break;
                    case RenderMode.TransparentWithZWrite:
                        transparentZWriteRenderQueues.Add(mtoon.Definition.Rendering.RenderQueueOffsetNumber);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var defaultTransparentQueue = 0;
            var transparentRenderQueueMap = new Dictionary<int, int>();
            foreach (var srcQueue in transparentRenderQueues.Reverse())
            {
                transparentRenderQueueMap.Add(srcQueue, defaultTransparentQueue--);
            }
            var defaultTransparentZWriteQueue = 0;
            var transparentZWriteRenderQueueMap = new Dictionary<int, int>();
            foreach (var srcQueue in transparentZWriteRenderQueues)
            {
                transparentZWriteRenderQueueMap.Add(srcQueue, defaultTransparentZWriteQueue++);
            }

            // Main Pass
            foreach (var (mtoon, gltfMaterial) in sourceMaterials)
            {
                if (mtoon == null)
                {
                    continue;
                }
                var extensions = new glTFExtensionExport();
                gltfMaterial.extensions = extensions;
                extensions.Add(
                    glTF_KHR_materials_unlit.ExtensionName,
                    new ArraySegment<byte>(glTF_KHR_materials_unlit.Raw));

                //
                // definition の中身を gltfMaterial と gltfMaterial.extensions.VRMC_materials_mtoon に移し替える
                //
                var dst = new VRMC_materials_mtoon
                {
                    SpecVersion = Vrm10Exporter.MTOON_SPEC_VERSION,
                };

                // Texture Transform
                Vector2? textureScale = default;
                Vector2? textureOffset = default;
                if (mtoon.TextureIndexMap.MainTex.HasValue && mtoon.TextureOffsetScales.TryGetValue("_MainTex", out var offsetScaleArray))
                {
                    textureScale = new Vector2(offsetScaleArray[2], offsetScaleArray[3]);
                    textureOffset = new Vector2(offsetScaleArray[0], offsetScaleArray[1]);
                }

                // Rendering
                switch (mtoon.Definition.Rendering.RenderMode)
                {
                    case RenderMode.Opaque:
                        gltfMaterial.alphaMode = "OPAQUE";
                        dst.TransparentWithZWrite = false;
                        gltfMaterial.alphaCutoff = 0.5f;
                        dst.RenderQueueOffsetNumber = 0;
                        break;
                    case RenderMode.Cutout:
                        gltfMaterial.alphaMode = "MASK";
                        dst.TransparentWithZWrite = false;
                        gltfMaterial.alphaCutoff = mtoon.Definition.Color.CutoutThresholdValue;
                        dst.RenderQueueOffsetNumber = 0;
                        break;
                    case RenderMode.Transparent:
                        gltfMaterial.alphaMode = "BLEND";
                        dst.TransparentWithZWrite = false;
                        gltfMaterial.alphaCutoff = 0.5f;
                        dst.RenderQueueOffsetNumber = Mathf.Clamp(transparentRenderQueueMap[mtoon.Definition.Rendering.RenderQueueOffsetNumber], -9, 0);
                        break;
                    case RenderMode.TransparentWithZWrite:
                        gltfMaterial.alphaMode = "BLEND";
                        dst.TransparentWithZWrite = true;
                        gltfMaterial.alphaCutoff = 0.5f;
                        dst.RenderQueueOffsetNumber = Mathf.Clamp(transparentZWriteRenderQueueMap[mtoon.Definition.Rendering.RenderQueueOffsetNumber], 0, +9);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                switch (mtoon.Definition.Rendering.CullMode)
                {
                    case MToon.CullMode.Back:
                        gltfMaterial.doubleSided = false;
                        break;
                    case MToon.CullMode.Off:
                        gltfMaterial.doubleSided = true;
                        break;
                    case MToon.CullMode.Front:
                        // GLTF not support
                        gltfMaterial.doubleSided = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // Lighting
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = mtoon.Definition.Color.LitColor.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
                if (mtoon.TextureIndexMap.MainTex.HasValue)
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = mtoon.TextureIndexMap.MainTex.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                dst.ShadeColorFactor = mtoon.Definition.Color.ShadeColor.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
                if (mtoon.TextureIndexMap.ShadeTexture.HasValue)
                {
                    dst.ShadeMultiplyTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.ShadeTexture.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            dst.ShadeMultiplyTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                // NOTE: DESTRUCTIVE MIGRATION
                // Lit Texture が存在するが Shade Texture が存在しないとき、 Lit Texture を Shade Texture に設定する.
                // これは破壊的マイグレーションだが、以下の MToon 0.x の状況により、問題が起きるユーザが多いためマイグレーションする.
                //   - MToon 0.x は 2 枚メインテクスチャを設定するのが正しい状態であるという周知の不足
                //   - MToon 0.x は Global Illumination の実装不備で、Shade Texture を設定しなくてもそれなりに表示できてしまっていた
                if (mtoon.TextureIndexMap.MainTex.HasValue && !mtoon.TextureIndexMap.ShadeTexture.HasValue)
                {
                    dst.ShadeMultiplyTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.MainTex.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            dst.ShadeMultiplyTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                if (mtoon.TextureIndexMap.BumpMap.HasValue)
                {
                    gltfMaterial.normalTexture = new glTFMaterialNormalTextureInfo
                    {
                        index = mtoon.TextureIndexMap.BumpMap.Value,
                        scale = mtoon.Definition.Lighting.Normal.NormalScaleValue
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            gltfMaterial.normalTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                dst.ShadingShiftFactor = MToon10Migrator.MigrateToShadingShift(
                    mtoon.Definition.Lighting.LitAndShadeMixing.ShadingToonyValue,
                    mtoon.Definition.Lighting.LitAndShadeMixing.ShadingShiftValue
                );

                dst.ShadingToonyFactor = MToon10Migrator.MigrateToShadingToony(
                    mtoon.Definition.Lighting.LitAndShadeMixing.ShadingToonyValue,
                    mtoon.Definition.Lighting.LitAndShadeMixing.ShadingShiftValue
                );

                // GI
                dst.GiEqualizationFactor = MToon10Migrator.MigrateToGiEqualization(mtoon.Definition.Lighting.LightingInfluence.GiIntensityValue);

                // Emission
                gltfMaterial.emissiveFactor = mtoon.Definition.Emission.EmissionColor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);
                if (mtoon.TextureIndexMap.EmissionMap.HasValue)
                {
                    gltfMaterial.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = mtoon.TextureIndexMap.EmissionMap.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            gltfMaterial.emissiveTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                // Rim Lighting
                if (mtoon.TextureIndexMap.SphereAdd.HasValue)
                {
                    // NOTE: MatCap behaviour will change in VRM 1.0.
                    // Texture transform is not required.
                    dst.MatcapTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.SphereAdd.Value
                    };
                    dst.MatcapFactor = new [] { 1f, 1f, 1f };
                }
                else
                {
                    dst.MatcapFactor = new[] { 0f, 0f, 0f };
                }
                dst.ParametricRimColorFactor = mtoon.Definition.Rim.RimColor.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
                dst.ParametricRimFresnelPowerFactor = mtoon.Definition.Rim.RimFresnelPowerValue;
                dst.ParametricRimLiftFactor = mtoon.Definition.Rim.RimLiftValue;
                if (mtoon.TextureIndexMap.RimTexture.HasValue)
                {
                    dst.RimMultiplyTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.RimTexture.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            dst.RimMultiplyTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }
                // NOTE: DESTRUCTIVE MIGRATION
                // Rim Lighting behaviour will be merged with MatCap in VRM 1.0.
                // So, RimLightingMixFactor set to 1.0, because it is safe appearance.
                dst.RimLightingMixFactor = 1.0f;

                // Outline
                const float centimeterToMeter = 0.01f;
                const float oneHundredth = 0.01f;
                switch (mtoon.Definition.Outline.OutlineWidthMode)
                {
                    case OutlineWidthMode.None:
                        dst.OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.none;
                        dst.OutlineWidthFactor = null;
                        break;
                    case OutlineWidthMode.WorldCoordinates:
                        dst.OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.worldCoordinates;
                        dst.OutlineWidthFactor = mtoon.Definition.Outline.OutlineWidthValue * centimeterToMeter;
                        break;
                    case OutlineWidthMode.ScreenCoordinates:
                        dst.OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.screenCoordinates;
                        // NOTE: 従来は、縦幅の半分を 100% としたときの % の値だった。
                        //       1.0 では縦幅を 1 としたときの値とするので、 1/200 する。
                        dst.OutlineWidthFactor = mtoon.Definition.Outline.OutlineWidthValue * oneHundredth * 0.5f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"OutlineWidthMode: {(int)mtoon.Definition.Outline.OutlineWidthMode}");
                }
                if (mtoon.TextureIndexMap.OutlineWidthTexture.HasValue)
                {
                    dst.OutlineWidthMultiplyTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.OutlineWidthTexture.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            dst.OutlineWidthMultiplyTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }
                dst.OutlineColorFactor = mtoon.Definition.Outline.OutlineColor.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
                switch (mtoon.Definition.Outline.OutlineColorMode)
                {
                    case OutlineColorMode.FixedColor:
                        dst.OutlineLightingMixFactor = 0.0f;
                        break;
                    case OutlineColorMode.MixedLighting:
                        dst.OutlineLightingMixFactor = mtoon.Definition.Outline.OutlineLightingMixValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // UV Animation
                if (mtoon.TextureIndexMap.UvAnimMaskTexture.HasValue)
                {
                    dst.UvAnimationMaskTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.UvAnimMaskTexture.Value
                    };
                    if (textureScale.HasValue && textureOffset.HasValue)
                    {
                        Vrm10MaterialExportUtils.ExportTextureTransform(
                            dst.UvAnimationMaskTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }
                dst.UvAnimationRotationSpeedFactor = mtoon.Definition.TextureOption.UvAnimationRotationSpeedValue;
                dst.UvAnimationScrollXSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollXSpeedValue;
                const float invertY = -1f;
                dst.UvAnimationScrollYSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollYSpeedValue * invertY;

                // Export
                UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref gltfMaterial.extensions, dst);

                if (!gltf.extensionsUsed.Contains(UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon.ExtensionName))
                {
                    gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon.ExtensionName);
                }

                if (!gltf.extensionsUsed.Contains(glTF_KHR_texture_transform.ExtensionName))
                {
                    gltf.extensionsUsed.Add(glTF_KHR_texture_transform.ExtensionName);
                }
            }
        }
    }
}

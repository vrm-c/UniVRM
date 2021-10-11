using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class MigrationMToon
    {
        static Color ToColor(JsonNode node, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            return node.ArrayItems().Select(x => x.GetSingle()).ToArray().ToColor4(srcColorSpace, dstColorSpace);
        }

        static float[] ToFloat4(JsonNode node)
        {
            return node.ArrayItems().Select(x => x.GetSingle()).ToArray();
        }

        struct TextureIndexMap
        {
            // glTF
            public int? MainTex;
            public int? BumpMap;
            public int? EmissionMap;
            // VRMC_materials_mtoon
            public int? ShadeTexture;
            public int? ReceiveShadowTexture;
            public int? ShadingGradeTexture;
            public int? RimTexture;
            public int? SphereAdd;
            public int? OutlineWidthTexture;
            public int? UvAnimMaskTexture;
        }

        /// <summary>
        /// vrm-0 の json から vrm-0 の MToon.Definition を生成する。
        ///
        /// Texture2D は作成せずに、直接 index を操作する。
        ///
        /// </summary>
        private sealed class Vrm0XMToonValue
        {
            public MToon.MToonDefinition Definition { get; }
            public Dictionary<string, float[]> TextureOffsetScales { get; }
            public TextureIndexMap TextureIndexMap { get; }

            public Vrm0XMToonValue(JsonNode vrmMaterial)
            {
                var definition = new MToon.MToonDefinition
                {
                    Color = new MToon.ColorDefinition { },
                    Lighting = new MToon.LightingDefinition
                    {
                        LightingInfluence = new MToon.LightingInfluenceDefinition { },
                        LitAndShadeMixing = new MToon.LitAndShadeMixingDefinition { },
                        Normal = new MToon.NormalDefinition { }
                    },
                    Emission = new MToon.EmissionDefinition { },
                    MatCap = new MToon.MatCapDefinition { },
                    Meta = new MToon.MetaDefinition { },
                    Outline = new MToon.OutlineDefinition { },
                    Rendering = new MToon.RenderingDefinition { },
                    Rim = new MToon.RimDefinition { },
                    TextureOption = new MToon.TextureUvCoordsDefinition { }
                };

                var offsetScale = new Dictionary<string, float[]>();
                foreach (var kv in vrmMaterial["vectorProperties"].ObjectItems())
                {
                    var key = kv.Key.GetString();
                    switch (key)
                    {
                        // Lighting
                        case "_Color":
                            definition.Color.LitColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                            break;
                        case "_ShadeColor":
                            definition.Color.ShadeColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                            break;

                        // Emission
                        case "_EmissionColor":
                            definition.Emission.EmissionColor = ToColor(kv.Value, ColorSpace.Linear, ColorSpace.Linear);
                            break;

                        // Rim Lighting
                        case "_RimColor":
                            definition.Rim.RimColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                            break;

                        // Outline
                        case "_OutlineColor":
                            definition.Outline.OutlineColor = ToColor(kv.Value, ColorSpace.sRGB, ColorSpace.sRGB);
                            break;

                        // Texture ST
                        case "_MainTex":
                        case "_ShadeTexture":
                        case "_BumpMap":
                        case "_EmissionMap":
                        case "_OutlineWidthTexture":
                        case "_ReceiveShadowTexture":
                        case "_RimTexture":
                        case "_ShadingGradeTexture":
                        case "_SphereAdd":
                        case "_UvAnimMaskTexture":
                            // scale, offset
                            offsetScale.Add(key, ToFloat4(kv.Value));
                            break;

                        default:
#if VRM_DEVELOP
                            Debug.LogWarning($"vectorProperties: {kv.Key}: {kv.Value}");
#endif
                            break;
                    }
                }

                foreach (var kv in vrmMaterial["floatProperties"].ObjectItems())
                {
                    var value = kv.Value.GetSingle();
                    switch (kv.Key.GetString())
                    {
                        // Rendering
                        case "_BlendMode":
                            definition.Rendering.RenderMode = (MToon.RenderMode)(int)value;
                            break;
                        case "_CullMode":
                            definition.Rendering.CullMode = (MToon.CullMode)(int)value;
                            break;
                        case "_Cutoff":
                            definition.Color.CutoutThresholdValue = value;
                            break;

                        // Lighting
                        case "_BumpScale":
                            definition.Lighting.Normal.NormalScaleValue = value;
                            break;
                        case "_LightColorAttenuation":
                            definition.Lighting.LightingInfluence.LightColorAttenuationValue = value;
                            break;
                        case "_ShadeShift":
                            definition.Lighting.LitAndShadeMixing.ShadingShiftValue = value;
                            break;
                        case "_ShadeToony":
                            definition.Lighting.LitAndShadeMixing.ShadingToonyValue = value;
                            break;
                        case "_ShadingGradeRate":
                            // Not supported
                            break;
                        case "_ReceiveShadowRate":
                            // Not supported
                            break;

                        // GI
                        case "_IndirectLightIntensity":
                            definition.Lighting.LightingInfluence.GiIntensityValue = value;
                            break;

                        // Rim Lighting
                        case "_RimFresnelPower":
                            definition.Rim.RimFresnelPowerValue = value;
                            break;
                        case "_RimLift":
                            definition.Rim.RimLiftValue = value;
                            break;
                        case "_RimLightingMix":
                            definition.Rim.RimLightingMixValue = value;
                            break;

                        // Outline
                        case "_OutlineColorMode":
                            definition.Outline.OutlineColorMode = (MToon.OutlineColorMode)value;
                            break;
                        case "_OutlineLightingMix":
                            definition.Outline.OutlineLightingMixValue = value;
                            break;
                        case "_OutlineScaledMaxDistance":
                            definition.Outline.OutlineScaledMaxDistanceValue = value;
                            break;
                        case "_OutlineWidth":
                            definition.Outline.OutlineWidthValue = value;
                            break;
                        case "_OutlineWidthMode":
                            if (value > 2)
                            {
                                value = 0;
                            }
                            definition.Outline.OutlineWidthMode = (MToon.OutlineWidthMode)value;
                            break;

                        // UV Animation
                        case "_UvAnimRotation":
                            definition.TextureOption.UvAnimationRotationSpeedValue = value;
                            break;

                        case "_UvAnimScrollX":
                            definition.TextureOption.UvAnimationScrollXSpeedValue = value;
                            break;

                        case "_UvAnimScrollY":
                            definition.TextureOption.UvAnimationScrollYSpeedValue = value;
                            break;

                        case "_OutlineCullMode":
                        case "_ZWrite":
                        case "_DstBlend":
                        case "_SrcBlend":
                        case "_MToonVersion":
                        case "_DebugMode":
                            // Auto generated
                            break;

                        default:
#if VRM_DEVELOP
                            Debug.LogWarning($"floatProperties: {kv.Key} is unknown");
#endif
                            break;
                    }
                }

                var map = new TextureIndexMap();

                foreach (var kv in vrmMaterial["textureProperties"].ObjectItems())
                {
                    var index = kv.Value.GetInt32();
                    switch (kv.Key.GetString())
                    {
                        // Lighting
                        case "_MainTex": map.MainTex = index; break;
                        case "_ShadeTexture": map.ShadeTexture = index; break;
                        case "_BumpMap": map.BumpMap = index; break;
                        case "_ReceiveShadowTexture": map.ReceiveShadowTexture = index; break;
                        case "_ShadingGradeTexture": map.ShadingGradeTexture = index; break;
                        // Emission
                        case "_EmissionMap": map.EmissionMap = index; break;
                        // Rim Lighting
                        case "_RimTexture": map.RimTexture = index; break;
                        case "_SphereAdd": map.SphereAdd = index; break;
                        // Outline
                        case "_OutlineWidthTexture": map.OutlineWidthTexture = index; break;
                        // UV Animation
                        case "_UvAnimMaskTexture": map.UvAnimMaskTexture = index; break;
                        default:
#if VRM_DEVELOP
                            Debug.LogWarning($"textureProperties: {kv.Key} is unknown");
#endif
                            break;
                    }
                }

                definition.Rendering.RenderQueueOffsetNumber =
                    vrmMaterial["renderQueue"].GetInt32() -
                    MToon.Utils.GetRenderQueueRequirement(definition.Rendering.RenderMode).DefaultValue;

                Definition = definition;
                TextureOffsetScales = offsetScale;
                TextureIndexMap = map;
            }
        }

        public static void Migrate(glTF gltf, JsonNode vrm0)
        {
            // Create MToonDefinition(0.x) from JSON(0.x)
            var sourceMaterials = new (Vrm0XMToonValue, glTFMaterial)[gltf.materials.Count];
            for (int i = 0; i < gltf.materials.Count; ++i)
            {
                var vrmMaterial = vrm0["materialProperties"][i];
                if (vrmMaterial["shader"].GetString() != "VRM/MToon")
                {
                    continue;
                }
                // VRM-0 MToon の情報
                var mtoon = new Vrm0XMToonValue(vrmMaterial);

                // KHR_materials_unlit として fallback した情報が入っている
                var gltfMaterial = gltf.materials[i];
                if (!glTF_KHR_materials_unlit.IsEnable(gltfMaterial))
                {
                    // 古いモデルは無い場合がある
                    // throw new Exception($"[{i}]{gltfMaterial.name} has no extensions");
                }
                sourceMaterials[i] = (mtoon, gltfMaterial);
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
                var dst = new VRMC_materials_mtoon();

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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
                            gltfMaterial.emissiveTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }

                // Rim Lighting
                if (mtoon.TextureIndexMap.SphereAdd.HasValue)
                {
                    // Matcap behaviour will change in VRM 1.0.
                    dst.MatcapTexture = new TextureInfo
                    {
                        Index = mtoon.TextureIndexMap.SphereAdd.Value
                    };
                    // Texture transform is not required.
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
                            dst.RimMultiplyTexture,
                            textureScale.Value,
                            textureOffset.Value
                        );
                    }
                }
                dst.RimLightingMixFactor = mtoon.Definition.Rim.RimLightingMixValue;

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
                        dst.OutlineWidthFactor = mtoon.Definition.Outline.OutlineWidthValue * oneHundredth;
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
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
                        Vrm10MToonMaterialExporter.ExportTextureTransform(
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
            }
        }
    }
}

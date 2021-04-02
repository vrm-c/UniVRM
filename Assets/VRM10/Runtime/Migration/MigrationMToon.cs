using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UniJSON;
using UnityEngine;


namespace UniVRM10
{
    public static class MigrationMToon
    {
        static float[] ToFloat4(this Color color)
        {
            return new float[] { color.r, color.g, color.b, color.a };
        }

        static float[] ToFloat3(this Color color)
        {
            return new float[] { color.r, color.g, color.b };
        }

        static Color ToColor(JsonNode node)
        {
            return node.ArrayItems().Select(x => x.GetSingle()).ToArray().ToColor4();
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
        struct MToonValue
        {
            public MToon.MToonDefinition Definition;

            // Texture の Offset/Scale
            public Dictionary<string, float[]> OffsetScale;

            // Texture の Index リスト
            public TextureIndexMap TextureIndexMap;

            public static MToonValue Create(JsonNode vrmMaterial)
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
                        case "_Color":
                            definition.Color.LitColor = ToColor(kv.Value);
                            break;

                        case "_ShadeColor":
                            definition.Color.ShadeColor = ToColor(kv.Value);
                            break;

                        case "_EmissionColor":
                            definition.Emission.EmissionColor = ToColor(kv.Value);
                            break;

                        case "_OutlineColor":
                            definition.Outline.OutlineColor = ToColor(kv.Value);
                            break;

                        case "_RimColor":
                            definition.Rim.RimColor = ToColor(kv.Value);
                            break;

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
                        case "_BlendMode":
                            definition.Rendering.RenderMode = (MToon.RenderMode)(int)value;
                            break;

                        case "_CullMode":
                            definition.Rendering.CullMode = (MToon.CullMode)(int)value;
                            break;

                        case "_Cutoff":
                            definition.Color.CutoutThresholdValue = value;
                            break;

                        case "_BumpScale":
                            definition.Lighting.Normal.NormalScaleValue = value;
                            break;

                        case "_LightColorAttenuation":
                            definition.Lighting.LightingInfluence.LightColorAttenuationValue = value;
                            break;

                        case "_RimFresnelPower":
                            definition.Rim.RimFresnelPowerValue = value;
                            break;

                        case "_RimLift":
                            definition.Rim.RimLiftValue = value;
                            break;

                        case "_RimLightingMix":
                            definition.Rim.RimLightingMixValue = value;
                            break;

                        case "_ShadeShift":
                            definition.Lighting.LitAndShadeMixing.ShadingShiftValue = value;
                            break;

                        case "_ShadeToony":
                            definition.Lighting.LitAndShadeMixing.ShadingToonyValue = value;
                            break;

                        case "_ShadingGradeRate":
                            // definition.Lighting.LightingInfluence.gr
                            break;

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
                            definition.Outline.OutlineWidthMode = (MToon.OutlineWidthMode)value;
                            break;

                        case "_OutlineCullMode":
                            // definition.Outline.
                            break;

                        case "_UvAnimRotation":
                            definition.TextureOption.UvAnimationRotationSpeedValue = value;
                            break;

                        case "_UvAnimScrollX":
                            definition.TextureOption.UvAnimationScrollXSpeedValue = value;
                            break;

                        case "_UvAnimScrollY":
                            definition.TextureOption.UvAnimationScrollYSpeedValue = value;
                            break;

                        case "_ZWrite":
                            break;

                        case "_ReceiveShadowRate":
                        case "_DstBlend":
                        case "_SrcBlend":
                        case "_IndirectLightIntensity":
                        case "_MToonVersion":
                        case "_DebugMode":
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
                        case "_MainTex": map.MainTex = index; break;
                        case "_ShadeTexture": map.ShadeTexture = index; break;
                        case "_BumpMap": map.BumpMap = index; break;
                        case "_ReceiveShadowTexture": map.ReceiveShadowTexture = index; break;
                        case "_ShadingGradeTexture": map.ShadingGradeTexture = index; break;
                        case "_RimTexture": map.RimTexture = index; break;
                        case "_SphereAdd": map.SphereAdd = index; break;
                        case "_EmissionMap": map.EmissionMap = index; break;
                        case "_OutlineWidthTexture": map.OutlineWidthTexture = index; break;
                        case "_UvAnimMaskTexture": map.UvAnimMaskTexture = index; break;
                        default:
#if VRM_DEVELOP                        
                            Debug.LogWarning($"textureProperties: {kv.Key} is unknown");
#endif
                            break;
                    }
                }

                return new MToonValue
                {
                    Definition = definition,
                    OffsetScale = offsetScale,
                    TextureIndexMap = map,
                };
            }
        }

        static (string, bool) GetRenderMode(MToon.RenderMode mode)
        {
            switch (mode)
            {
                case MToon.RenderMode.Opaque: return ("OPAQUE", false);
                case MToon.RenderMode.Cutout: return ("MASK", false);
                case MToon.RenderMode.Transparent: return ("BLEND", false);
                case MToon.RenderMode.TransparentWithZWrite: return ("BLEND", true);
            }

            throw new NotImplementedException();
        }

        public static void Migrate(glTF gltf, JsonNode json)
        {
            for (int i = 0; i < gltf.materials.Count; ++i)
            {
                var vrmMaterial = json["extensions"]["VRM"]["materialProperties"][i];
                if (vrmMaterial["shader"].GetString() != "VRM/MToon")
                {
                    continue;
                }

                // VRM-0 MToon の情報
                var mtoon = MToonValue.Create(vrmMaterial);

                // KHR_materials_unlit として fallback した情報が入っている
                var gltfMaterial = gltf.materials[i];
                if (!glTF_KHR_materials_unlit.IsEnable(gltfMaterial))
                {
                    // 古いモデルは無い場合がある                    
                    // throw new Exception($"[{i}]{gltfMaterial.name} has no extensions");
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

                // Color
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = mtoon.Definition.Color.LitColor.ToFloat4();
                if (mtoon.TextureIndexMap.MainTex.HasValue)
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = mtoon.TextureIndexMap.MainTex.Value
                    };
                    if (!mtoon.OffsetScale.TryGetValue("_MainTex", out float[] value))
                    {
                        value = new float[] { 0, 0, 1, 1 };
                    }
                    glTF_KHR_texture_transform.Serialize(
                        gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                        (value[0], value[1]),
                        (value[2], value[3])
                        );
                }
                dst.ShadeFactor = mtoon.Definition.Color.ShadeColor.ToFloat3();
                if (mtoon.TextureIndexMap.ShadeTexture.HasValue)
                {
                    dst.ShadeMultiplyTexture = mtoon.TextureIndexMap.ShadeTexture.Value;
                }
                gltfMaterial.alphaCutoff = mtoon.Definition.Color.CutoutThresholdValue;

                // Outline
                dst.OutlineColorMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineColorMode)mtoon.Definition.Outline.OutlineColorMode;
                dst.OutlineFactor = ToFloat3(mtoon.Definition.Outline.OutlineColor);
                dst.OutlineLightingMixFactor = mtoon.Definition.Outline.OutlineLightingMixValue;
                dst.OutlineScaledMaxDistanceFactor = mtoon.Definition.Outline.OutlineScaledMaxDistanceValue;
                dst.OutlineWidthMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode)mtoon.Definition.Outline.OutlineWidthMode;
                dst.OutlineWidthFactor = mtoon.Definition.Outline.OutlineWidthValue;
                if (mtoon.TextureIndexMap.OutlineWidthTexture.HasValue)
                {
                    dst.OutlineWidthMultiplyTexture = mtoon.TextureIndexMap.OutlineWidthTexture.Value;
                }

                // Emission
                gltfMaterial.emissiveFactor = mtoon.Definition.Emission.EmissionColor.ToFloat3();
                if (mtoon.TextureIndexMap.EmissionMap.HasValue)
                {
                    gltfMaterial.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = mtoon.TextureIndexMap.EmissionMap.Value
                    };
                    var value = mtoon.OffsetScale["_EmissionMap"];
                    glTF_KHR_texture_transform.Serialize(
                        gltfMaterial.emissiveTexture,
                        (value[0], value[1]),
                        (value[2], value[3])
                        );
                }

                // Light
                dst.GiIntensityFactor = mtoon.Definition.Lighting.LightingInfluence.GiIntensityValue;
                dst.LightColorAttenuationFactor = mtoon.Definition.Lighting.LightingInfluence.LightColorAttenuationValue;
                dst.ShadingShiftFactor = mtoon.Definition.Lighting.LitAndShadeMixing.ShadingShiftValue;
                dst.ShadingToonyFactor = mtoon.Definition.Lighting.LitAndShadeMixing.ShadingToonyValue;
                if (mtoon.TextureIndexMap.BumpMap.HasValue)
                {
                    gltfMaterial.normalTexture = new glTFMaterialNormalTextureInfo
                    {
                        index = mtoon.TextureIndexMap.BumpMap.Value,
                        scale = mtoon.Definition.Lighting.Normal.NormalScaleValue
                    };
                    var value = mtoon.OffsetScale["_BumpMap"];
                    glTF_KHR_texture_transform.Serialize(
                        gltfMaterial.normalTexture,
                        (value[0], value[1]),
                        (value[2], value[3])
                        );
                }

                // matcap
                if (mtoon.TextureIndexMap.SphereAdd.HasValue)
                {
                    dst.AdditiveTexture = mtoon.TextureIndexMap.SphereAdd.Value;
                }

                // rendering
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
                        gltfMaterial.doubleSided = false;
                        break;

                    default:
                        throw new NotImplementedException();
                }
                (gltfMaterial.alphaMode, dst.TransparentWithZWrite) = GetRenderMode(mtoon.Definition.Rendering.RenderMode);
                dst.RenderQueueOffsetNumber = mtoon.Definition.Rendering.RenderQueueOffsetNumber;

                // rim
                dst.RimFactor = mtoon.Definition.Rim.RimColor.ToFloat3();
                if (mtoon.TextureIndexMap.RimTexture.HasValue)
                {
                    dst.RimMultiplyTexture = mtoon.TextureIndexMap.RimTexture.Value;
                }
                dst.RimLiftFactor = mtoon.Definition.Rim.RimLiftValue;
                dst.RimFresnelPowerFactor = mtoon.Definition.Rim.RimFresnelPowerValue;
                dst.RimLightingMixFactor = mtoon.Definition.Rim.RimLightingMixValue;

                // texture option
                if (mtoon.TextureIndexMap.UvAnimMaskTexture.HasValue)
                {
                    dst.UvAnimationMaskTexture = mtoon.TextureIndexMap.UvAnimMaskTexture.Value;
                }
                dst.UvAnimationRotationSpeedFactor = mtoon.Definition.TextureOption.UvAnimationRotationSpeedValue;
                dst.UvAnimationScrollXSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollXSpeedValue;
                dst.UvAnimationScrollYSpeedFactor = mtoon.Definition.TextureOption.UvAnimationScrollYSpeedValue;

                UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref gltfMaterial.extensions, dst);
            }
        }
    }
}

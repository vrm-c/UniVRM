using System;
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

        static MToon.MToonDefinition VRM0_MToon(JsonNode json)
        {
            // TODO: がんばって json から中身を作る
            var definition = new MToon.MToonDefinition
            {
                Color = new MToon.ColorDefinition
                {

                },
                Lighting = new MToon.LightingDefinition
                {
                    LightingInfluence = new MToon.LightingInfluenceDefinition
                    {

                    },
                    LitAndShadeMixing = new MToon.LitAndShadeMixingDefinition
                    {

                    },
                    Normal = new MToon.NormalDefinition
                    {

                    }
                },
                Emission = new MToon.EmissionDefinition
                {

                },
                MatCap = new MToon.MatCapDefinition
                {

                },
                Meta = new MToon.MetaDefinition
                {

                },
                Outline = new MToon.OutlineDefinition
                {

                },
                Rendering = new MToon.RenderingDefinition
                {

                },
                Rim = new MToon.RimDefinition
                {

                },
                TextureOption = new MToon.TextureUvCoordsDefinition
                {

                }
            };
            return definition;
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

        struct TextureIndexMap
        {
            public int? MainTex;
            public int? ShadeTexture;
            public int? BumpMap;
            public int? ReceiveShadowTexture;
            public int? ShadingGradeTexture;
            public int? RimTexture;
            public int? SphereAdd;
            public int? EmissionMap;
            public int? OutlineWidthTexture;
            public int? UvAnimMaskTexture;

            public static TextureIndexMap Create(JsonNode vrmMaterial)
            {
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
                            throw new NotImplementedException($"{kv.Key} is unknown");
                    }
                }

                return map;
            }
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
                var definition = VRM0_MToon(json);

                // KHR_materials_unlit として fallback した情報が入っている
                var gltfMaterial = gltf.materials[i];
                if (!glTF_KHR_materials_unlit.IsEnable(gltfMaterial))
                {
                    throw new Exception($"[{i}]{gltfMaterial.name} has no extensions");
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

                var textureIndexMap = TextureIndexMap.Create(vrmMaterial);

                // Color
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = definition.Color.LitColor.ToFloat4();
                if (textureIndexMap.MainTex.HasValue)
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = textureIndexMap.MainTex.Value
                    };
                }
                dst.ShadeFactor = definition.Color.ShadeColor.ToFloat3();
                if (textureIndexMap.ShadeTexture.HasValue)
                {
                    dst.ShadeMultiplyTexture = textureIndexMap.ShadeTexture.Value;
                }
                gltfMaterial.alphaCutoff = definition.Color.CutoutThresholdValue;

                // Outline
                dst.OutlineColorMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineColorMode)definition.Outline.OutlineColorMode;
                dst.OutlineFactor = ToFloat3(definition.Outline.OutlineColor);
                dst.OutlineLightingMixFactor = definition.Outline.OutlineLightingMixValue;
                dst.OutlineScaledMaxDistanceFactor = definition.Outline.OutlineScaledMaxDistanceValue;
                dst.OutlineWidthMode = (UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode)definition.Outline.OutlineWidthMode;
                dst.OutlineWidthFactor = definition.Outline.OutlineWidthValue;
                // if(textureIndexMap)
                // dst.OutlineWidthMultiplyTexture = definition.Outline.OutlineWidthMultiplyTexture.ToIndex(textures);

                // Emission
                gltfMaterial.emissiveFactor = definition.Emission.EmissionColor.ToFloat3();
                if (textureIndexMap.EmissionMap.HasValue)
                {
                    gltfMaterial.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = textureIndexMap.EmissionMap.Value
                    };
                }

                // Light
                dst.GiIntensityFactor = definition.Lighting.LightingInfluence.GiIntensityValue;
                dst.LightColorAttenuationFactor = definition.Lighting.LightingInfluence.LightColorAttenuationValue;
                dst.ShadingShiftFactor = definition.Lighting.LitAndShadeMixing.ShadingShiftValue;
                dst.ShadingToonyFactor = definition.Lighting.LitAndShadeMixing.ShadingToonyValue;
                if (textureIndexMap.BumpMap.HasValue)
                {
                    gltfMaterial.normalTexture = new glTFMaterialNormalTextureInfo
                    {
                        index = textureIndexMap.BumpMap.Value,
                        scale = definition.Lighting.Normal.NormalScaleValue
                    };
                }

                // matcap
                if (textureIndexMap.SphereAdd.HasValue)
                {
                    dst.AdditiveTexture = textureIndexMap.SphereAdd.Value;
                }

                // rendering
                switch (definition.Rendering.CullMode)
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
                (gltfMaterial.alphaMode, dst.TransparentWithZWrite) = GetRenderMode(definition.Rendering.RenderMode);
                dst.RenderQueueOffsetNumber = definition.Rendering.RenderQueueOffsetNumber;

                // rim
                dst.RimFactor = definition.Rim.RimColor.ToFloat3();
                if (textureIndexMap.RimTexture.HasValue)
                {
                    dst.RimMultiplyTexture = textureIndexMap.RimTexture.Value;
                }
                dst.RimLiftFactor = definition.Rim.RimLiftValue;
                dst.RimFresnelPowerFactor = definition.Rim.RimFresnelPowerValue;
                dst.RimLightingMixFactor = definition.Rim.RimLightingMixValue;

                // texture option
                if (textureIndexMap.UvAnimMaskTexture.HasValue)
                {
                    dst.UvAnimationMaskTexture = textureIndexMap.UvAnimMaskTexture.Value;
                }
                dst.UvAnimationRotationSpeedFactor = definition.TextureOption.UvAnimationRotationSpeedValue;
                dst.UvAnimationScrollXSpeedFactor = definition.TextureOption.UvAnimationScrollXSpeedValue;
                dst.UvAnimationScrollYSpeedFactor = definition.TextureOption.UvAnimationScrollYSpeedValue;
                if (gltfMaterial.pbrMetallicRoughness.baseColorTexture != null)
                {
                    var offset = definition.TextureOption.MainTextureLeftBottomOriginOffset;
                    var scale = definition.TextureOption.MainTextureLeftBottomOriginScale;
                    glTF_KHR_texture_transform.Serialize(
                        gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                        (offset.x, offset.y),
                        (scale.x, scale.y)
                        );
                }

                UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref gltfMaterial.extensions, dst);
            }
        }
    }
}

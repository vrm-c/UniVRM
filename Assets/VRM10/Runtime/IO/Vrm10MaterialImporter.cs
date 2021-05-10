using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10MaterialImporter
    {
        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="i"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            var m = parser.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions,
                out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
            {
                // fallback to gltf
                param = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            param = new MaterialImportParam(m.name, MToon.Utils.ShaderName);

            param.Actions.Add(material =>
            {
                // Texture 以外をここで設定。Texture は TextureSlots へ
                {
                    // material.SetFloat(PropVersion, mtoon.Version);
                }
                {
                    // var rendering = mtoon.Rendering;
                    // SetRenderMode(material, rendering.RenderMode, rendering.RenderQueueOffsetNumber,
                    //     useDefaultRenderQueue: false);
                    // SetCullMode(material, rendering.CullMode);
                }
                {
                    // var color = mtoon.Color;
                    material.SetColor(MToon.Utils.PropColor, m.pbrMetallicRoughness.baseColorFactor
                        .ToColor4(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.sRGB)
                    );
                    if (mtoon.ShadeColorFactor != null)
                    {
                        material.SetColor(MToon.Utils.PropShadeColor, mtoon.ShadeColorFactor
                            .ToColor3(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.sRGB)
                        );
                    }
                    material.SetFloat(MToon.Utils.PropCutoff, m.alphaCutoff);
                }
                {
                    {
                        if (mtoon.ShadingShiftFactor.HasValue) material.SetFloat(MToon.Utils.PropShadeShift, mtoon.ShadingShiftFactor.Value);
                        if (mtoon.ShadingToonyFactor.HasValue) material.SetFloat(MToon.Utils.PropShadeToony, mtoon.ShadingToonyFactor.Value);
                        // material.SetFloat(PropReceiveShadowRate, mtoon.prop.ShadowReceiveMultiplierValue);
                        // material.SetFloat(PropShadingGradeRate, mtoon.mix  prop.LitAndShadeMixingMultiplierValue);
                    }
                    {
                        if (mtoon.GiIntensityFactor.HasValue) material.SetFloat(MToon.Utils.PropIndirectLightIntensity, mtoon.GiIntensityFactor.Value);
                    }
                }
                {
                    material.SetColor(MToon.Utils.PropEmissionColor,
                        m.emissiveFactor.ToColor3(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.Linear)
                    );
                }
                {
                    if (mtoon.ParametricRimColorFactor != null)
                    {
                        material.SetColor(MToon.Utils.PropRimColor, mtoon.ParametricRimColorFactor
                            .ToColor3(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.sRGB)
                        );
                    }
                    if (mtoon.RimLightingMixFactor.HasValue) material.SetFloat(MToon.Utils.PropRimLightingMix, mtoon.RimLightingMixFactor.Value);
                    if (mtoon.ParametricRimFresnelPowerFactor.HasValue) material.SetFloat(MToon.Utils.PropRimFresnelPower, mtoon.ParametricRimFresnelPowerFactor.Value);
                    if (mtoon.ParametricRimLiftFactor.HasValue) material.SetFloat(MToon.Utils.PropRimLift, mtoon.ParametricRimLiftFactor.Value);
                }
                {
                    if (mtoon.OutlineWidthFactor.HasValue) material.SetFloat(MToon.Utils.PropOutlineWidth, mtoon.OutlineWidthFactor.Value);
                    if (mtoon.OutlineColorFactor != null)
                    {
                        material.SetColor(MToon.Utils.PropOutlineColor, mtoon.OutlineColorFactor
                            .ToColor3(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.sRGB)
                        );
                    }
                    if (mtoon.OutlineLightingMixFactor.HasValue) material.SetFloat(MToon.Utils.PropOutlineLightingMix, mtoon.OutlineLightingMixFactor.Value);

                    // private
                    // MToon.Utils.SetOutlineMode(material, outline.OutlineWidthMode, outline.OutlineColorMode);
                }
                {
                    // material.SetTextureScale(PropMainTex, mtoon.MainTextureLeftBottomOriginScale);
                    // material.SetTextureOffset(PropMainTex, mtoon.MainTextureLeftBottomOriginOffset);
                    if (mtoon.UvAnimationScrollXSpeedFactor.HasValue) material.SetFloat(MToon.Utils.PropUvAnimScrollX, mtoon.UvAnimationScrollXSpeedFactor.Value);
                    if (mtoon.UvAnimationScrollYSpeedFactor.HasValue) material.SetFloat(MToon.Utils.PropUvAnimScrollY, mtoon.UvAnimationScrollYSpeedFactor.Value);
                    if (mtoon.UvAnimationRotationSpeedFactor.HasValue) material.SetFloat(MToon.Utils.PropUvAnimRotation, mtoon.UvAnimationRotationSpeedFactor.Value);
                }

                MToon.Utils.ValidateProperties(material, isBlendModeChangedByUser: false);
            });

            // SetTexture(material, PropMainTex, color.LitMultiplyTexture);
            // SetNormalMapping(material, prop.NormalTexture, prop.NormalScaleValue);
            // SetTexture(material, PropEmissionMap, emission.EmissionMultiplyTexture);

            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    param.TextureSlots.Add("_MainTex", GltfPBRMaterial.BaseColorTexture(parser, m).Param);
                }
            }

            if (m.normalTexture != null && m.normalTexture.index != -1)
            {
                // normal map
                param.Actions.Add(material => material.EnableKeyword("_NORMALMAP"));
                var textureParam = GltfPBRMaterial.NormalTexture(parser, m).Param;
                param.TextureSlots.Add("_BumpMap", textureParam);
                param.FloatValues.Add("_BumpScale", m.normalTexture.scale);
            }

            if (m.emissiveTexture != null && m.emissiveTexture.index != -1)
            {
                var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(m.emissiveTexture);
                var textureParam = GltfTextureImporter.CreateSRGB(parser, m.emissiveTexture.index, offset, scale).Param;
                param.TextureSlots.Add("_EmissionMap", textureParam);
            }

            // TODO:
            if (mtoon.ShadeMultiplyTexture != null)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.ShadeMultiplyTexture.Index.Value, Vector2.zero, Vector2.one).Param;
                param.TextureSlots.Add("_ShadeTexture", textureParam);
            }
            if (mtoon.OutlineWidthMultiplyTexture != null)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.OutlineWidthMultiplyTexture.Index.Value, Vector2.zero, Vector2.one).Param;
                param.TextureSlots.Add("_OutlineWidthTexture", textureParam);
            }
            if (mtoon.MatcapTexture != null)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.MatcapTexture.Index.Value, Vector2.zero, Vector2.one).Param;
                param.TextureSlots.Add("_SphereAdd", textureParam);
            }
            if (mtoon.RimMultiplyTexture != null)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.RimMultiplyTexture.Index.Value, Vector2.zero, Vector2.one).Param;
                param.TextureSlots.Add("_RimTexture", textureParam); ;
            }
            if (mtoon.UvAnimationMaskTexture != null)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.UvAnimationMaskTexture.Index.Value, Vector2.zero, Vector2.one).Param;
                param.TextureSlots.Add("_UvAnimMaskTexture", textureParam);
            }

            return true;
        }

        public static MaterialImportParam GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!TryCreateParam(parser, i, out MaterialImportParam param))
            {
                // unlit
                if (!GltfUnlitMaterial.TryCreateParam(parser, i, out param))
                {
                    // pbr
                    GltfPBRMaterial.TryCreateParam(parser, i, out param);
                }
            }
            return param;
        }
    }
}

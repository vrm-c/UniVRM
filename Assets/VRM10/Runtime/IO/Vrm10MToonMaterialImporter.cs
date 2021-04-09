using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;


namespace UniVRM10
{
    public static class Vrm10MToonMaterialImporter
    {
        public static Color ToColor4(this float[] src, Color defaultValue = default)
        {
            if (src == null || src.Length != 4)
            {
                throw new NotImplementedException();
            }

            var v = new Vector4(
                src[0],
                src[1],
                src[2],
                src[3]
            );
            return v;
        }
        public static Color ToColor3(this float[] src, Color defaultValue = default)
        {
            if (src == null || src.Length != 3)
            {
                throw new NotImplementedException();
            }

            var v = new Vector4(
                src[0],
                src[1],
                src[2]
            );
            return v;
        }

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
                    material.SetColor(MToon.Utils.PropColor, m.pbrMetallicRoughness.baseColorFactor.ToColor4());
                    material.SetColor(MToon.Utils.PropShadeColor, mtoon.ShadeFactor.ToColor3());
                    material.SetFloat(MToon.Utils.PropCutoff, m.alphaCutoff);
                }
                {
                    {
                        material.SetFloat(MToon.Utils.PropShadeShift, mtoon.ShadingShiftFactor.Value);
                        material.SetFloat(MToon.Utils.PropShadeToony, mtoon.ShadingToonyFactor.Value);
                        // material.SetFloat(PropReceiveShadowRate, mtoon.prop.ShadowReceiveMultiplierValue);
                        // material.SetFloat(PropShadingGradeRate, mtoon.mix  prop.LitAndShadeMixingMultiplierValue);
                    }
                    {
                        material.SetFloat(MToon.Utils.PropLightColorAttenuation, mtoon.LightColorAttenuationFactor.Value);
                        material.SetFloat(MToon.Utils.PropIndirectLightIntensity, mtoon.GiIntensityFactor.Value);
                    }
                }
                {
                    material.SetColor(MToon.Utils.PropEmissionColor, m.emissiveFactor.ToColor3());
                }
                {
                    material.SetColor(MToon.Utils.PropRimColor, mtoon.RimFactor.ToColor3());
                    material.SetFloat(MToon.Utils.PropRimLightingMix, mtoon.RimLightingMixFactor.Value);
                    material.SetFloat(MToon.Utils.PropRimFresnelPower, mtoon.RimFresnelPowerFactor.Value);
                    material.SetFloat(MToon.Utils.PropRimLift, mtoon.RimLiftFactor.Value);
                }
                {
                    material.SetFloat(MToon.Utils.PropOutlineWidth, mtoon.OutlineWidthFactor.Value);
                    material.SetFloat(MToon.Utils.PropOutlineScaledMaxDistance, mtoon.OutlineScaledMaxDistanceFactor.Value);
                    material.SetColor(MToon.Utils.PropOutlineColor, mtoon.OutlineFactor.ToColor3());
                    material.SetFloat(MToon.Utils.PropOutlineLightingMix, mtoon.OutlineLightingMixFactor.Value);
                    // private
                    // MToon.Utils.SetOutlineMode(material, outline.OutlineWidthMode, outline.OutlineColorMode);
                }
                {
                    // material.SetTextureScale(PropMainTex, mtoon.MainTextureLeftBottomOriginScale);
                    // material.SetTextureOffset(PropMainTex, mtoon.MainTextureLeftBottomOriginOffset);
                    material.SetFloat(MToon.Utils.PropUvAnimScrollX, mtoon.UvAnimationScrollXSpeedFactor.Value);
                    material.SetFloat(MToon.Utils.PropUvAnimScrollY, mtoon.UvAnimationScrollYSpeedFactor.Value);
                    material.SetFloat(MToon.Utils.PropUvAnimRotation, mtoon.UvAnimationRotationSpeedFactor.Value);
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
                    param.TextureSlots.Add("_MainTex", GltfPBRMaterial.BaseColorTexture(parser, m));
                }
            }

            if (m.normalTexture != null && m.normalTexture.index != -1)
            {
                // normal map
                param.Actions.Add(material => material.EnableKeyword("_NORMALMAP"));
                var textureParam = GltfPBRMaterial.NormalTexture(parser, m);
                param.TextureSlots.Add("_BumpMap", textureParam);
                param.FloatValues.Add("_BumpScale", m.normalTexture.scale);
            }

            if (m.emissiveTexture != null && m.emissiveTexture.index != -1)
            {
                var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(m.emissiveTexture);
                var textureParam = GltfTextureImporter.CreateSRGB(parser, m.emissiveTexture.index, offset, scale);
                param.TextureSlots.Add("_EmissionMap", textureParam);
            }

            // TODO:
            if (mtoon.ShadeMultiplyTexture.HasValue)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.ShadeMultiplyTexture.Value, Vector2.zero, Vector2.one);
                param.TextureSlots.Add("_ShadeTexture", textureParam);
            }
            if (mtoon.OutlineWidthMultiplyTexture.HasValue)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.OutlineWidthMultiplyTexture.Value, Vector2.zero, Vector2.one);
                param.TextureSlots.Add("_OutlineWidthTexture", textureParam);
            }
            if (mtoon.AdditiveTexture.HasValue)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.AdditiveTexture.Value, Vector2.zero, Vector2.one);
                param.TextureSlots.Add("_SphereAdd", textureParam);
            }
            if (mtoon.RimMultiplyTexture.HasValue)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.RimMultiplyTexture.Value, Vector2.zero, Vector2.one);
                param.TextureSlots.Add("_RimTexture", textureParam); ;
            }
            if (mtoon.UvAnimationMaskTexture.HasValue)
            {
                var textureParam = GltfTextureImporter.CreateSRGB(parser, mtoon.UvAnimationMaskTexture.Value, Vector2.zero, Vector2.one);
                param.TextureSlots.Add("_UvAnimMaskTexture", textureParam);
            }

            return true;
        }

        /// <summary>
        /// Material一つ分のテクスチャーを列挙する。重複する場合がある
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static IEnumerable<TextureImportParam> EnumerateTexturesForMaterial(GltfParser parser, int i)
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

            foreach (var kv in param.TextureSlots)
            {
                yield return kv.Value;
            }
        }

        /// <summary>
        /// VRM-1 の thumbnail テクスチャー。gltf.textures ではなく gltf.images の参照であることに注意(sampler等の設定が無い)
        /// 
        /// MToonとは無関係だがとりあえずここに
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="vrm"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetMetaThumbnailTextureImportParam(GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, out TextureImportParam value)
        {
            if (vrm?.Meta == null || !vrm.Meta.ThumbnailImage.HasValue)
            {
                value = default;
                return false;
            }

            // thumbnail
            var imageIndex = vrm.Meta.ThumbnailImage.Value;
            var gltfImage = parser.GLTF.images[imageIndex];
            var name = new TextureImportName(TextureImportTypes.sRGB, gltfImage.name, gltfImage.GetExt(), "");

            GetTextureBytesAsync getBytesAsync = () =>
            {
                var bytes = parser.GLTF.GetImageBytes(parser.Storage, imageIndex);
                return Task.FromResult(GltfTextureImporter.ToArray(bytes));
            };
            value = new TextureImportParam(name, Vector2.zero, Vector2.one, default, TextureImportTypes.sRGB, default, default,
               getBytesAsync, default, default,
               default, default, default
               );
            return true;
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーをユニークになるように列挙する
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IEnumerable<TextureImportParam> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            if (TryGetMetaThumbnailTextureImportParam(parser, vrm, out TextureImportParam thumbnail))
            {
                yield return thumbnail;
            }

            var used = new HashSet<string>();
            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                foreach (var textureInfo in EnumerateTexturesForMaterial(parser, i))
                {
                    if (used.Add(textureInfo.ExtractKey))
                    {
                        yield return textureInfo;
                    }
                }
            }
        }
    }
}

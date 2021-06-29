using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    /// <summary>
    /// Gltf から MaterialImportParam に変換する
    ///
    /// StandardShader variables
    ///
    /// _Color
    /// _MainTex
    /// _Cutoff
    /// _Glossiness
    /// _Metallic
    /// _MetallicGlossMap
    /// _BumpScale
    /// _BumpMap
    /// _Parallax
    /// _ParallaxMap
    /// _OcclusionStrength
    /// _OcclusionMap
    /// _EmissionColor
    /// _EmissionMap
    /// _DetailMask
    /// _DetailAlbedoMap
    /// _DetailNormalMapScale
    /// _DetailNormalMap
    /// _UVSec
    /// _EmissionScaleUI
    /// _EmissionColorUI
    /// _Mode
    /// _SrcBlend
    /// _DstBlend
    /// _ZWrite
    ///
    /// </summary>
    public static class GltfPbrMaterialImporter
    {
        public const string ShaderName = "Standard";

        private enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public static bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (i < 0 || i >= data.GLTF.materials.Count)
            {
                matDesc = default;
                return false;
            }

            var src = data.GLTF.materials[i];
            matDesc = new MaterialDescriptor(GltfMaterialDescriptorGenerator.GetMaterialName(i, src), ShaderName);

            var standardTexDesc = default(TextureDescriptor);
            if (src.pbrMetallicRoughness != null || src.occlusionTexture != null)
            {
                if (src.pbrMetallicRoughness.metallicRoughnessTexture != null || src.occlusionTexture != null)
                {
                    SubAssetKey key;
                    (key, standardTexDesc) = GltfPbrTextureImporter.StandardTexture(data, src);
                }

                if (src.pbrMetallicRoughness.baseColorFactor != null && src.pbrMetallicRoughness.baseColorFactor.Length == 4)
                {
                    matDesc.Colors.Add("_Color",
                        src.pbrMetallicRoughness.baseColorFactor.ToColor4(ColorSpace.Linear, ColorSpace.sRGB)
                    );
                }

                if (src.pbrMetallicRoughness.baseColorTexture != null && src.pbrMetallicRoughness.baseColorTexture.index != -1)
                {
                    var (key, textureParam) = GltfPbrTextureImporter.BaseColorTexture(data, src);
                    matDesc.TextureSlots.Add("_MainTex", textureParam);
                }

                if (src.pbrMetallicRoughness.metallicRoughnessTexture != null && src.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                {
                    matDesc.Actions.Add(material => material.EnableKeyword("_METALLICGLOSSMAP"));
                    matDesc.TextureSlots.Add("_MetallicGlossMap", standardTexDesc);
                    // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                    matDesc.FloatValues.Add("_Metallic", 1.0f);
                    matDesc.FloatValues.Add("_GlossMapScale", 1.0f);
                }
                else
                {
                    matDesc.FloatValues.Add("_Metallic", src.pbrMetallicRoughness.metallicFactor);
                    matDesc.FloatValues.Add("_Glossiness", 1.0f - src.pbrMetallicRoughness.roughnessFactor);
                }
            }

            if (src.normalTexture != null && src.normalTexture.index != -1)
            {
                matDesc.Actions.Add(material => material.EnableKeyword("_NORMALMAP"));
                var (key, textureParam) = GltfPbrTextureImporter.NormalTexture(data, src);
                matDesc.TextureSlots.Add("_BumpMap", textureParam);
                matDesc.FloatValues.Add("_BumpScale", src.normalTexture.scale);
            }

            if (src.occlusionTexture != null && src.occlusionTexture.index != -1)
            {
                matDesc.TextureSlots.Add("_OcclusionMap", standardTexDesc);
                matDesc.FloatValues.Add("_OcclusionStrength", src.occlusionTexture.strength);
            }

            if (src.emissiveFactor != null
                || (src.emissiveTexture != null && src.emissiveTexture.index != -1))
            {
                matDesc.Actions.Add(material =>
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                });

                if (src.emissiveFactor != null && src.emissiveFactor.Length == 3)
                {
                    var emissiveFactor = src.emissiveFactor.ToColor3(ColorSpace.Linear, ColorSpace.Linear);
                    if (UniGLTF.Extensions.VRMC_materials_hdr_emissiveMultiplier.GltfDeserializer.TryGet(src.extensions,
                        out UniGLTF.Extensions.VRMC_materials_hdr_emissiveMultiplier.VRMC_materials_hdr_emissiveMultiplier ex))
                    {
                        emissiveFactor *= ex.EmissiveMultiplier.Value;
                    }
                    matDesc.Colors.Add("_EmissionColor", emissiveFactor);
                }

                if (src.emissiveTexture != null && src.emissiveTexture.index != -1)
                {
                    var (key, textureParam) = GltfPbrTextureImporter.EmissiveTexture(data, src);
                    matDesc.TextureSlots.Add("_EmissionMap", textureParam);
                }
            }

            matDesc.Actions.Add(material =>
            {
                BlendMode blendMode = BlendMode.Opaque;
                // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/#post-2229980
                switch (src.alphaMode)
                {
                    case "BLEND":
                        blendMode = BlendMode.Fade;
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        break;

                    case "MASK":
                        blendMode = BlendMode.Cutout;
                        material.SetOverrideTag("RenderType", "TransparentCutout");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.SetFloat("_Cutoff", src.alphaCutoff);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 2450;

                        break;

                    default: // OPAQUE
                        blendMode = BlendMode.Opaque;
                        material.SetOverrideTag("RenderType", "");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        break;
                }

                material.SetFloat("_Mode", (float)blendMode);
            });

            return true;
        }
    }
}

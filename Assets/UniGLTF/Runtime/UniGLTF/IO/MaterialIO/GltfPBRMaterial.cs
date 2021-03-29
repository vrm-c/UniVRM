using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;


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
    public static class GltfPBRMaterial
    {
        public const string ShaderName = "Standard";

        private enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public static TextureImportParam BaseColorTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
            return GltfTextureImporter.CreateSRGB(parser, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
        }

        public static TextureImportParam StandardTexture(GltfParser parser, glTFMaterial src)
        {
            var metallicFactor = 1.0f;
            var roughnessFactor = 1.0f;
            if (src.pbrMetallicRoughness != null)
            {
                metallicFactor = src.pbrMetallicRoughness.metallicFactor;
                roughnessFactor = src.pbrMetallicRoughness.roughnessFactor;
            }
            var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
            return GltfTextureImporter.CreateStandard(parser,
                            src.pbrMetallicRoughness?.metallicRoughnessTexture?.index,
                            src.occlusionTexture?.index,
                            offset, scale,
                            metallicFactor,
                            roughnessFactor);
        }

        public static TextureImportParam NormalTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(src.normalTexture);
            return GltfTextureImporter.CreateNormal(parser, src.normalTexture.index, offset, scale);
        }

        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            if (i < 0 || i >= parser.GLTF.materials.Count)
            {
                param = default;
                return false;
            }

            var src = parser.GLTF.materials[i];
            param = new MaterialImportParam(GltfMaterialImporter.MaterialName(i, src), ShaderName);

            var standardParam = default(TextureImportParam);
            if (src.pbrMetallicRoughness != null || src.occlusionTexture != null)
            {
                if (src.pbrMetallicRoughness.metallicRoughnessTexture != null || src.occlusionTexture != null)
                {
                    standardParam = StandardTexture(parser, src);
                }

                if (src.pbrMetallicRoughness.baseColorFactor != null && src.pbrMetallicRoughness.baseColorFactor.Length == 4)
                {
                    var color = src.pbrMetallicRoughness.baseColorFactor;
                    param.Colors.Add("_Color", (new Color(color[0], color[1], color[2], color[3])).gamma);
                }

                if (src.pbrMetallicRoughness.baseColorTexture != null && src.pbrMetallicRoughness.baseColorTexture.index != -1)
                {
                    var textureParam = BaseColorTexture(parser, src);
                    param.TextureSlots.Add("_MainTex", textureParam);
                }

                if (src.pbrMetallicRoughness.metallicRoughnessTexture != null && src.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                {
                    param.Actions.Add(material => material.EnableKeyword("_METALLICGLOSSMAP"));
                    param.TextureSlots.Add("_MetallicGlossMap", standardParam);
                    // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                    param.FloatValues.Add("_Metallic", 1.0f);
                    param.FloatValues.Add("_GlossMapScale", 1.0f);
                }
                else
                {
                    param.FloatValues.Add("_Metallic", src.pbrMetallicRoughness.metallicFactor);
                    param.FloatValues.Add("_Glossiness", 1.0f - src.pbrMetallicRoughness.roughnessFactor);
                }
            }

            if (src.normalTexture != null && src.normalTexture.index != -1)
            {
                param.Actions.Add(material => material.EnableKeyword("_NORMALMAP"));
                var textureParam = NormalTexture(parser, src);
                param.TextureSlots.Add("_BumpMap", textureParam);
                param.FloatValues.Add("_BumpScale", src.normalTexture.scale);
            }

            if (src.occlusionTexture != null && src.occlusionTexture.index != -1)
            {
                param.TextureSlots.Add("_OcclusionMap", standardParam);
                param.FloatValues.Add("_OcclusionStrength", src.occlusionTexture.strength);
            }

            if (src.emissiveFactor != null
                || (src.emissiveTexture != null && src.emissiveTexture.index != -1))
            {
                param.Actions.Add(material => {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                });

                if (src.emissiveFactor != null && src.emissiveFactor.Length == 3)
                {
                    param.Colors.Add("_EmissionColor", new Color(src.emissiveFactor[0], src.emissiveFactor[1], src.emissiveFactor[2]));
                }

                if (src.emissiveTexture != null && src.emissiveTexture.index != -1)
                {
                    var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(src.emissiveTexture);
                    var textureParam = GltfTextureImporter.CreateSRGB(parser, src.emissiveTexture.index, offset, scale);
                    param.TextureSlots.Add("_EmissionMap", textureParam);
                }
            }

            param.Actions.Add(material => {
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

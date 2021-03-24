
using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
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
    public static class PBRMaterialItem
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
            var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
            return TextureFactory.CreateSRGB(parser, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
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
            var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
            return TextureFactory.CreateStandard(parser,
                            src.pbrMetallicRoughness?.metallicRoughnessTexture?.index,
                            src.occlusionTexture?.index,
                            offset, scale,
                            metallicFactor,
                            roughnessFactor);
        }

        public static TextureImportParam NormalTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.normalTexture);
            return TextureFactory.CreateNormal(parser, src.normalTexture.index, offset, scale);
        }

        public static async Task<Material> CreateAsync(IAwaitCaller awaitCaller, GltfParser parser, int i, GetTextureAsyncFunc getTexture)
        {
            if (getTexture == null)
            {
                getTexture = (IAwaitCaller _awaitCaller, glTF _gltf, TextureImportParam _param) => Task.FromResult<Texture2D>(null);
            }

            var material = new Material(Shader.Find(ShaderName));
            if (i < 0 || i >= parser.GLTF.materials.Count)
            {
                material.name = MaterialFactory.MaterialName(i, null);
                return material;
            }

            var src = parser.GLTF.materials[i];
            material.name = MaterialFactory.MaterialName(i, src);
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
                    material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
                }

                if (src.pbrMetallicRoughness.baseColorTexture != null && src.pbrMetallicRoughness.baseColorTexture.index != -1)
                {
                    var param = BaseColorTexture(parser, src);
                    material.mainTexture = await getTexture(awaitCaller, parser.GLTF, param);

                    // Texture Offset and Scale
                    MaterialFactory.SetTextureOffsetAndScale(material, "_MainTex", param.Offset, param.Scale);
                }

                if (src.pbrMetallicRoughness.metallicRoughnessTexture != null && src.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                {
                    material.EnableKeyword("_METALLICGLOSSMAP");

                    var texture = await getTexture(awaitCaller, parser.GLTF, standardParam);
                    if (texture != null)
                    {
                        material.SetTexture(TextureImportParam.METALLIC_GLOSS_PROP, texture);
                    }

                    material.SetFloat("_Metallic", 1.0f);
                    // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                    material.SetFloat("_GlossMapScale", 1.0f);

                    // Texture Offset and Scale
                    var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
                    MaterialFactory.SetTextureOffsetAndScale(material, "_MetallicGlossMap", offset, scale);
                }
                else
                {
                    material.SetFloat("_Metallic", src.pbrMetallicRoughness.metallicFactor);
                    material.SetFloat("_Glossiness", 1.0f - src.pbrMetallicRoughness.roughnessFactor);
                }
            }

            if (src.normalTexture != null && src.normalTexture.index != -1)
            {
                material.EnableKeyword("_NORMALMAP");
                var param = NormalTexture(parser, src);
                var texture = await getTexture(awaitCaller, parser.GLTF, param);
                if (texture != null)
                {
                    material.SetTexture(TextureImportParam.NORMAL_PROP, texture);
                    material.SetFloat("_BumpScale", src.normalTexture.scale);
                }

                // Texture Offset and Scale
                MaterialFactory.SetTextureOffsetAndScale(material, "_BumpMap", param.Offset, param.Scale);
            }

            if (src.occlusionTexture != null && src.occlusionTexture.index != -1)
            {
                var texture = await getTexture(awaitCaller, parser.GLTF, standardParam);
                if (texture != null)
                {
                    material.SetTexture(TextureImportParam.OCCLUSION_PROP, texture);
                    material.SetFloat("_OcclusionStrength", src.occlusionTexture.strength);
                }

                // Texture Offset and Scale
                var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.occlusionTexture);
                MaterialFactory.SetTextureOffsetAndScale(material, "_OcclusionMap", offset, scale);
            }

            if (src.emissiveFactor != null
                || (src.emissiveTexture != null && src.emissiveTexture.index != -1))
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                if (src.emissiveFactor != null && src.emissiveFactor.Length == 3)
                {
                    material.SetColor("_EmissionColor", new Color(src.emissiveFactor[0], src.emissiveFactor[1], src.emissiveFactor[2]));
                }

                if (src.emissiveTexture != null && src.emissiveTexture.index != -1)
                {
                    var (offset, scale) = MaterialFactory.GetTextureOffsetAndScale(src.emissiveTexture);
                    var param = TextureFactory.CreateSRGB(parser, src.emissiveTexture.index, offset, scale);
                    var texture = await getTexture(awaitCaller, parser.GLTF, param);
                    if (texture != null)
                    {
                        material.SetTexture("_EmissionMap", texture);
                    }

                    // Texture Offset and Scale
            
                    MaterialFactory.SetTextureOffsetAndScale(material, "_EmissionMap", param.Offset, param.Scale);
                }
            }

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

            return material;
        }
    }
}

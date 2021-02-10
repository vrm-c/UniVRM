using UnityEngine;

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
    public class PBRMaterialItem : MaterialItemBase
    {
        public const string PBRShaderName = "Standard";

        private enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public PBRMaterialItem(int i, glTFMaterial src) : base(i, src)
        {
        }

        public override Material GetOrCreate(GetTextureItemFunc getTexture)
        {
            if (getTexture == null)
            {
                getTexture = _ => null;
            }

            var material = new Material(Shader.Find(PBRShaderName));
#if UNITY_EDITOR
            // textureImporter.SaveAndReimport(); may destroy this material
            material.hideFlags = HideFlags.DontUnloadUnusedAsset;
#endif
            material.name = (base.m_src == null || string.IsNullOrEmpty(base.m_src.name))
                ? string.Format("material_{0:00}", m_index)
                : base.m_src.name
                ;

            // PBR material
            if (m_src != null)
            {
                if (m_src.pbrMetallicRoughness != null)
                {
                    if (m_src.pbrMetallicRoughness.baseColorFactor != null && m_src.pbrMetallicRoughness.baseColorFactor.Length == 4)
                    {
                        var color = m_src.pbrMetallicRoughness.baseColorFactor;
                        material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
                    }

                    if (m_src.pbrMetallicRoughness.baseColorTexture != null && m_src.pbrMetallicRoughness.baseColorTexture.index != -1)
                    {
                        var texture = getTexture(m_src.pbrMetallicRoughness.baseColorTexture.index);
                        if (texture != null)
                        {
                            material.mainTexture = texture.Texture;
                        }

                        // Texture Offset and Scale
                        SetTextureOffsetAndScale(material, m_src.pbrMetallicRoughness.baseColorTexture, "_MainTex");
                    }

                    if (m_src.pbrMetallicRoughness.metallicRoughnessTexture != null && m_src.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                    {
                        material.EnableKeyword("_METALLICGLOSSMAP");
                        var texture = getTexture(m_src.pbrMetallicRoughness.metallicRoughnessTexture.index);
                        if (texture != null)
                        {
                            var prop = "_MetallicGlossMap";
                            // Bake roughnessFactor values into a texture.
                            material.SetTexture(prop, texture.ConvertTexture(prop, m_src.pbrMetallicRoughness.roughnessFactor));
                        }

                        material.SetFloat("_Metallic", 1.0f);
                        // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                        material.SetFloat("_GlossMapScale", 1.0f);

                        // Texture Offset and Scale
                        SetTextureOffsetAndScale(material, m_src.pbrMetallicRoughness.metallicRoughnessTexture, "_MetallicGlossMap");
                    }
                    else
                    {
                        material.SetFloat("_Metallic", m_src.pbrMetallicRoughness.metallicFactor);
                        material.SetFloat("_Glossiness", 1.0f - m_src.pbrMetallicRoughness.roughnessFactor);
                    }
                }

                if (m_src.normalTexture != null && m_src.normalTexture.index != -1)
                {
                    material.EnableKeyword("_NORMALMAP");
                    var texture = getTexture(m_src.normalTexture.index);
                    if (texture != null)
                    {
                        var prop = "_BumpMap";
                        material.SetTexture(prop, texture.ConvertTexture(prop));
                        material.SetFloat("_BumpScale", m_src.normalTexture.scale);
                    }

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, m_src.normalTexture, "_BumpMap");
                }

                if (m_src.occlusionTexture != null && m_src.occlusionTexture.index != -1)
                {
                    var texture = getTexture(m_src.occlusionTexture.index);
                    if (texture != null)
                    {
                        var prop = "_OcclusionMap";
                        material.SetTexture(prop, texture.ConvertTexture(prop));
                        material.SetFloat("_OcclusionStrength", m_src.occlusionTexture.strength);
                    }

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, m_src.occlusionTexture, "_OcclusionMap");
                }

                if (m_src.emissiveFactor != null
                    || (m_src.emissiveTexture != null && m_src.emissiveTexture.index != -1))
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                    if (m_src.emissiveFactor != null && m_src.emissiveFactor.Length == 3)
                    {
                        material.SetColor("_EmissionColor", new Color(m_src.emissiveFactor[0], m_src.emissiveFactor[1], m_src.emissiveFactor[2]));
                    }

                    if (m_src.emissiveTexture != null && m_src.emissiveTexture.index != -1)
                    {
                        var texture = getTexture(m_src.emissiveTexture.index);
                        if (texture != null)
                        {
                            material.SetTexture("_EmissionMap", texture.Texture);
                        }

                        // Texture Offset and Scale
                        SetTextureOffsetAndScale(material, m_src.emissiveTexture, "_EmissionMap");
                    }
                }

                BlendMode blendMode = BlendMode.Opaque;
                // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/#post-2229980
                switch (m_src.alphaMode)
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
                        material.SetFloat("_Cutoff", m_src.alphaCutoff);
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
            }

            return material;
        }
    }
}

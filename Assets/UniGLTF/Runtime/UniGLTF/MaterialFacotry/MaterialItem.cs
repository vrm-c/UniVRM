using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public abstract class MaterialItemBase
    {
        protected int m_index;
        protected glTFMaterial m_src;

        public string Name { get; set; }

        public MaterialItemBase(int i, glTFMaterial src)
        {
            m_index = i;
            m_src = src;
            Name = src != null ? m_src.name : "";
        }

        public abstract Material GetOrCreate(GetTextureItemFunc getTexture);

        protected static void SetTextureOffsetAndScale(Material material, glTFTextureInfo textureInfo, string propertyName)
        {
            if (glTF_KHR_texture_transform.TryGet(textureInfo, out glTF_KHR_texture_transform textureTransform))
            {
                Vector2 offset = new Vector2(0, 0);
                Vector2 scale = new Vector2(1, 1);
                if (textureTransform.offset != null && textureTransform.offset.Length == 2)
                {
                    offset = new Vector2(textureTransform.offset[0], textureTransform.offset[1]);
                }
                if (textureTransform.scale != null && textureTransform.scale.Length == 2)
                {
                    scale = new Vector2(textureTransform.scale[0], textureTransform.scale[1]);
                }

                offset.y = (offset.y + scale.y - 1.0f) * -1.0f;

                material.SetTextureOffset(propertyName, offset);
                material.SetTextureScale(propertyName, scale);
            }
        }
    }

    public class UnlitMaterialItem : MaterialItemBase
    {
        public const string UniUnlitShaderName = "UniGLTF/UniUnlit";

        bool m_hasVertexColor;

        public UnlitMaterialItem(int i, glTFMaterial src, bool hasVertexColor) : base(i, src)
        {
            m_hasVertexColor = hasVertexColor;
        }

        public override Material GetOrCreate(GetTextureItemFunc getTexture)
        {
            if (getTexture == null)
            {
                getTexture = _ => null;
            }

            var material = new Material(Shader.Find(UniUnlitShaderName));
#if UNITY_EDITOR
            // textureImporter.SaveAndReimport(); may destroy this material
            material.hideFlags = HideFlags.DontUnloadUnusedAsset;
#endif
            material.name = (m_src == null || string.IsNullOrEmpty(m_src.name))
                ? string.Format("material_{0:00}", m_index)
                : m_src.name
                ;

            // texture
            if (m_src.pbrMetallicRoughness.baseColorTexture != null)
            {
                var texture = getTexture(m_src.pbrMetallicRoughness.baseColorTexture.index);
                if (texture != null)
                {
                    material.mainTexture = texture.Texture;
                }

                // Texture Offset and Scale
                SetTextureOffsetAndScale(material, m_src.pbrMetallicRoughness.baseColorTexture, "_MainTex");
            }

            // color
            if (m_src.pbrMetallicRoughness.baseColorFactor != null && m_src.pbrMetallicRoughness.baseColorFactor.Length == 4)
            {
                var color = m_src.pbrMetallicRoughness.baseColorFactor;
                material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
            }

            //renderMode
            if (m_src.alphaMode == "OPAQUE")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
            }
            else if (m_src.alphaMode == "BLEND")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Transparent);
            }
            else if (m_src.alphaMode == "MASK")
            {
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Cutout);
                material.SetFloat("_Cutoff", m_src.alphaCutoff);
            }
            else
            {
                // default OPAQUE
                UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
            }

            // culling
            if (m_src.doubleSided)
            {
                UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Off);
            }
            else
            {
                UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Back);
            }

            // VColor
            if (m_hasVertexColor)
            {
                UniUnlit.Utils.SetVColBlendMode(material, UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
            }

            UniUnlit.Utils.ValidateProperties(material, true);

            return material;
        }
    }

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

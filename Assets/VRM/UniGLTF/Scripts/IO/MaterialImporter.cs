using UnityEngine;
using System;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public interface IMaterialImporter
    {
        Material CreateMaterial(int i, glTFMaterial src, bool hasVertexColor);
    }

    public class MaterialImporter : IMaterialImporter
    {
        IShaderStore m_shaderStore;

        protected Func<int, TextureItem> GetTextureFunc;

        public MaterialImporter(IShaderStore shaderStore, Func<int, TextureItem> getTextureFunc)
        {
            m_shaderStore = shaderStore;
            GetTextureFunc = getTextureFunc;
        }

        private enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
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
        public virtual Material CreateMaterial(int i, glTFMaterial x, bool hasVertexColor)
        {
            var shader = m_shaderStore.GetShader(x);
            //Debug.LogFormat("[{0}]{1}", i, shader.name);
            var material = new Material(shader);
#if UNITY_EDITOR
            // textureImporter.SaveAndReimport(); may destroy this material
            material.hideFlags = HideFlags.DontUnloadUnusedAsset;
#endif

            material.name = (x == null || string.IsNullOrEmpty(x.name))
                ? string.Format("material_{0:00}", i)
                : x.name
                ;

            if (x == null)
            {
                Debug.LogWarning("glTFMaterial is empty");
                return material;
            }

            // unlit material
            if (x.extensions != null && x.extensions.KHR_materials_unlit != null)
            {
                // texture
                if (x.pbrMetallicRoughness.baseColorTexture != null)
                {
                    var texture = GetTextureFunc(x.pbrMetallicRoughness.baseColorTexture.index);
                    if (texture != null)
                    {
                        material.mainTexture = texture.Texture;
                    }

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, x.pbrMetallicRoughness.baseColorTexture, "_MainTex");
                }

                // color
                if (x.pbrMetallicRoughness.baseColorFactor != null && x.pbrMetallicRoughness.baseColorFactor.Length == 4)
                {
                    var color = x.pbrMetallicRoughness.baseColorFactor;
                    material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
                }

                //renderMode
                if (x.alphaMode == "OPAQUE")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
                }
                else if (x.alphaMode == "BLEND")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Transparent);
                }
                else if(x.alphaMode == "MASK")
                {
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Cutout);
                }
                else
                {
                    // default OPAQUE
                    UniUnlit.Utils.SetRenderMode(material, UniUnlit.UniUnlitRenderMode.Opaque);
                }

                // culling
                if (x.doubleSided)
                {
                    UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Off);
                }
                else
                {
                    UniUnlit.Utils.SetCullMode(material, UniUnlit.UniUnlitCullMode.Back);
                }

                // VColor
                if (hasVertexColor)
                {
                    UniUnlit.Utils.SetVColBlendMode(material, UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
                }

                UniUnlit.Utils.ValidateProperties(material, true);

                return material;
            }

            // PBR material
            if (x.pbrMetallicRoughness != null)
            {
                if (x.pbrMetallicRoughness.baseColorFactor != null && x.pbrMetallicRoughness.baseColorFactor.Length == 4)
                {
                    var color = x.pbrMetallicRoughness.baseColorFactor;
                    material.color = (new Color(color[0], color[1], color[2], color[3])).gamma;
                }

                if (x.pbrMetallicRoughness.baseColorTexture != null && x.pbrMetallicRoughness.baseColorTexture.index != -1)
                {
                    var texture = GetTextureFunc(x.pbrMetallicRoughness.baseColorTexture.index);
                    if (texture != null)
                    {
                        material.mainTexture = texture.Texture;
                    }

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, x.pbrMetallicRoughness.baseColorTexture, "_MainTex");
                }

                if (x.pbrMetallicRoughness.metallicRoughnessTexture != null && x.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                {
                    material.EnableKeyword("_METALLICGLOSSMAP");
                    var texture = GetTextureFunc(x.pbrMetallicRoughness.metallicRoughnessTexture.index);
                    if (texture != null)
                    {
                        var prop = "_MetallicGlossMap";
                        // Bake roughnessFactor values into a texture.
                        material.SetTexture(prop, texture.ConvertTexture(prop, x.pbrMetallicRoughness.roughnessFactor));
                    }

                    material.SetFloat("_Metallic", 1.0f);
                    // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                    material.SetFloat("_GlossMapScale", 1.0f);

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, x.pbrMetallicRoughness.metallicRoughnessTexture, "_MetallicGlossMap");
                }
                else
                {
                    material.SetFloat("_Metallic", x.pbrMetallicRoughness.metallicFactor);
                    material.SetFloat("_Glossiness", 1.0f - x.pbrMetallicRoughness.roughnessFactor);
                }
            }

            if (x.normalTexture != null && x.normalTexture.index != -1)
            {
                material.EnableKeyword("_NORMALMAP");
                var texture = GetTextureFunc(x.normalTexture.index);
                if (texture != null)
                {
                    var prop = "_BumpMap";
                    material.SetTexture(prop, texture.ConvertTexture(prop));
                    material.SetFloat("_BumpScale", x.normalTexture.scale);
                }

                // Texture Offset and Scale
                SetTextureOffsetAndScale(material, x.normalTexture, "_BumpMap");
            }

            if (x.occlusionTexture != null && x.occlusionTexture.index != -1)
            {
                var texture = GetTextureFunc(x.occlusionTexture.index);
                if (texture != null)
                {
                    var prop = "_OcclusionMap";
                    material.SetTexture(prop, texture.ConvertTexture(prop));
                    material.SetFloat("_OcclusionStrength", x.occlusionTexture.strength);
                }

                // Texture Offset and Scale
                SetTextureOffsetAndScale(material, x.occlusionTexture, "_OcclusionMap");
            }

            if (x.emissiveFactor != null
                || (x.emissiveTexture != null && x.emissiveTexture.index != -1))
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                if (x.emissiveFactor != null && x.emissiveFactor.Length == 3)
                {
                    material.SetColor("_EmissionColor", new Color(x.emissiveFactor[0], x.emissiveFactor[1], x.emissiveFactor[2]));
                }

                if (x.emissiveTexture != null && x.emissiveTexture.index != -1)
                {
                    var texture = GetTextureFunc(x.emissiveTexture.index);
                    if (texture != null)
                    {
                        material.SetTexture("_EmissionMap", texture.Texture);
                    }

                    // Texture Offset and Scale
                    SetTextureOffsetAndScale(material, x.emissiveTexture, "_EmissionMap");
                }
            }

            BlendMode blendMode = BlendMode.Opaque;
            // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/#post-2229980
            switch (x.alphaMode)
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
                    material.SetFloat("_Cutoff", x.alphaCutoff);
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

        private static void SetTextureOffsetAndScale(Material material, glTFTextureInfo textureInfo, string propertyName)
        {
            if (textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform != null)
            {
                var textureTransform = textureInfo.extensions.KHR_texture_transform;
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
}

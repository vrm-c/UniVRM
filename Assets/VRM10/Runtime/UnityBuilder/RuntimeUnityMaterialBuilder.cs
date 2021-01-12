using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public static class RuntimeUnityMaterialBuilder
    {
        public static UnityEngine.Material CreateMaterialAsset(VrmLib.Material src, bool hasVertexColor, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            if (src is VrmLib.MToonMaterial mtoonSrc)
            {
                // MTOON
                var material = new Material(Shader.Find(MToon.Utils.ShaderName));
                MToon.Utils.SetMToonParametersToMaterial(material, mtoonSrc.Definition.ToUnity(textures));
                return material;
            }

            if (src is VrmLib.UnlitMaterial unlitSrc)
            {
                return CreateUnlitMaterial(unlitSrc, hasVertexColor, textures);
            }

            if (src is VrmLib.PBRMaterial pbrSrc)
            {
                return CreateStandardMaterial(pbrSrc, textures);
            }

            throw new NotImplementedException($"unknown material: {src}");
        }

        static UnityEngine.Material CreateUnlitMaterial(VrmLib.UnlitMaterial src, bool hasVertexColor, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            var material = new Material(Shader.Find(UniGLTF.UniUnlit.Utils.ShaderName));

            // texture
            if (src.BaseColorTexture != null)
            {
                material.mainTexture = textures[src.BaseColorTexture.Texture];
            }

            // color
            material.color = src.BaseColorFactor.ToUnitySRGB();

            //renderMode
            switch (src.AlphaMode)
            {
                case VrmLib.AlphaModeType.OPAQUE:
                    UniGLTF.UniUnlit.Utils.SetRenderMode(material, UniGLTF.UniUnlit.UniUnlitRenderMode.Opaque);
                    break;

                case VrmLib.AlphaModeType.BLEND:
                    UniGLTF.UniUnlit.Utils.SetRenderMode(material, UniGLTF.UniUnlit.UniUnlitRenderMode.Transparent);
                    break;

                case VrmLib.AlphaModeType.MASK:
                    UniGLTF.UniUnlit.Utils.SetRenderMode(material, UniGLTF.UniUnlit.UniUnlitRenderMode.Cutout);
                    material.SetFloat(UniGLTF.UniUnlit.Utils.PropNameCutoff, src.AlphaCutoff);
                    break;

                default:
                    UniGLTF.UniUnlit.Utils.SetRenderMode(material, UniGLTF.UniUnlit.UniUnlitRenderMode.Opaque);
                    break;
            }

            // culling
            if (src.DoubleSided)
            {
                UniGLTF.UniUnlit.Utils.SetCullMode(material, UniGLTF.UniUnlit.UniUnlitCullMode.Off);
            }
            else
            {
                UniGLTF.UniUnlit.Utils.SetCullMode(material, UniGLTF.UniUnlit.UniUnlitCullMode.Back);
            }

            // VColor
            if (hasVertexColor)
            {
                UniGLTF.UniUnlit.Utils.SetVColBlendMode(material, UniGLTF.UniUnlit.UniUnlitVertexColorBlendOp.Multiply);
            }

            UniGLTF.UniUnlit.Utils.ValidateProperties(material, true);

            return material;
        }

        // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/#post-2229980
        internal enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,        // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        static UnityEngine.Material CreateStandardMaterial(VrmLib.PBRMaterial x, Dictionary<VrmLib.Texture, Texture2D> textures)
        {
            var material = new Material(Shader.Find("Standard"));

            material.color = x.BaseColorFactor.ToUnitySRGB();

            if (x.BaseColorTexture != null)
            {
                material.mainTexture = textures[x.BaseColorTexture.Texture];
            }

            if (x.MetallicRoughnessTexture != null)
            {
                material.EnableKeyword("_METALLICGLOSSMAP");
                var texture = textures[x.MetallicRoughnessTexture];
                if (texture != null)
                {
                    var prop = "_MetallicGlossMap";
                    material.SetTexture(prop, texture);
                }

                material.SetFloat("_Metallic", 1.0f);
                // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                material.SetFloat("_GlossMapScale", 1.0f);
            }
            else
            {
                material.SetFloat("_Metallic", x.MetallicFactor);
                material.SetFloat("_Glossiness", 1.0f - x.RoughnessFactor);
            }

            if (x.NormalTexture != null)
            {
                material.EnableKeyword("_NORMALMAP");
                var texture = textures[x.NormalTexture];
                if (texture != null)
                {
                    var prop = "_BumpMap";
                    material.SetTexture(prop, texture);
                    material.SetFloat("_BumpScale", x.NormalTextureScale);
                }
            }

            if (x.OcclusionTexture != null)
            {
                var texture = textures[x.OcclusionTexture];
                if (texture != null)
                {
                    var prop = "_OcclusionMap";
                    material.SetTexture(prop, texture);
                    material.SetFloat("_OcclusionStrength", x.OcclusionTextureStrength);
                }
            }

            if (x.EmissiveFactor != System.Numerics.Vector3.Zero || x.EmissiveTexture != null)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                material.SetColor("_EmissionColor", x.EmissiveFactor.ToUnityColor());

                if (x.EmissiveTexture != null)
                {
                    var texture = textures[x.EmissiveTexture];
                    if (texture != null)
                    {
                        material.SetTexture("_EmissionMap", texture);
                    }
                }
            }

            BlendMode blendMode = BlendMode.Opaque;
            // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/#post-2229980
            switch (x.AlphaMode)
            {
                case VrmLib.AlphaModeType.BLEND:
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

                case VrmLib.AlphaModeType.MASK:
                    blendMode = BlendMode.Cutout;
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.SetFloat("_Cutoff", x.AlphaCutoff);
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

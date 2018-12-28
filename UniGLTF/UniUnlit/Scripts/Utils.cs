using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF.UniUnlit
{
    public enum UniUnlitRenderMode
    {
        Opaque = 0,
        Cutout = 1,
        Transparent = 2,
    }

    public enum UniUnlitCullMode
    {
        Off = 0,
//        Front = 1,
        Back = 2,
    }

    public enum UniUnlitVertexColorBlendOp
    {
        None = 0,
        Multiply = 1,
    }
    
    public static class Utils
    {
        public const string PropNameMainTex = "_MainTex";
        public const string PropNameColor = "_Color";
        public const string PropNameCutoff = "_Cutoff";
        public const string PropNameBlendMode = "_BlendMode";
        public const string PropNameCullMode = "_CullMode";
        public const string PropeNameVColBlendMode = "_VColBlendMode";
        public const string PropNameSrcBlend = "_SrcBlend";
        public const string PropNameDstBlend = "_DstBlend";
        public const string PropNameZWrite = "_ZWrite";

        public const string PropNameStandardShadersRenderMode = "_Mode";

        public const string KeywordAlphaTestOn = "_ALPHATEST_ON";
        public const string KeywordAlphaBlendOn = "_ALPHABLEND_ON";
        public const string KeywordVertexColMul = "_VERTEXCOL_MUL";

        public const string TagRenderTypeKey = "RenderType";
        public const string TagRenderTypeValueOpaque = "Opaque";
        public const string TagRenderTypeValueTransparentCutout = "TransparentCutout";
        public const string TagRenderTypeValueTransparent = "Transparent";

        public static void SetRenderMode(Material material, UniUnlitRenderMode mode)
        {
            material.SetInt(PropNameBlendMode, (int)mode);
        }

        public static void SetCullMode(Material material, UniUnlitCullMode mode)
        {
            material.SetInt(PropNameCullMode, (int) mode);
        }

        public static UniUnlitRenderMode GetRenderMode(Material material)
        {
            return (UniUnlitRenderMode)material.GetInt(PropNameBlendMode);
        }

        public static UniUnlitCullMode GetCullMode(Material material)
        {
            return (UniUnlitCullMode)material.GetInt(PropNameCullMode);
        }

        /// <summary>
        /// Validate target material's UniUnlitRenderMode, UniUnlitVertexColorBlendOp.
        /// Set appropriate hidden properites & keywords.
        /// This will change RenderQueue independent to UniUnlitRenderMode if isRenderModeChagedByUser is true.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="isRenderModeChangedByUser">Is changed by user</param>
        public static void ValidateProperties(Material material, bool isRenderModeChangedByUser = false)
        {
            SetupBlendMode(material, (UniUnlitRenderMode)material.GetFloat(PropNameBlendMode),
                isRenderModeChangedByUser);
            SetupVertexColorBlendOp(material, (UniUnlitVertexColorBlendOp)material.GetFloat(PropeNameVColBlendMode));
        }

        private static void SetupBlendMode(Material material, UniUnlitRenderMode renderMode,
            bool isRenderModeChangedByUser = false)
        {
            switch (renderMode)
            {
                case UniUnlitRenderMode.Opaque:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueOpaque);
                    material.SetInt(PropNameSrcBlend, (int)BlendMode.One);
                    material.SetInt(PropNameDstBlend, (int)BlendMode.Zero);
                    material.SetInt(PropNameZWrite, 1);
                    SetKeyword(material, KeywordAlphaTestOn, false);
                    SetKeyword(material, KeywordAlphaBlendOn, false);
                    if (isRenderModeChangedByUser) material.renderQueue = -1;
                    break;
                case UniUnlitRenderMode.Cutout:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparentCutout);
                    material.SetInt(PropNameSrcBlend, (int)BlendMode.One);
                    material.SetInt(PropNameDstBlend, (int)BlendMode.Zero);
                    material.SetInt(PropNameZWrite, 1);
                    SetKeyword(material, KeywordAlphaTestOn, true);
                    SetKeyword(material, KeywordAlphaBlendOn, false);
                    if (isRenderModeChangedByUser) material.renderQueue = (int)RenderQueue.AlphaTest;
                    break;
                case UniUnlitRenderMode.Transparent:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropNameSrcBlend, (int)BlendMode.SrcAlpha);
                    material.SetInt(PropNameDstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropNameZWrite, 0);
                    SetKeyword(material, KeywordAlphaTestOn, false);
                    SetKeyword(material, KeywordAlphaBlendOn, true);
                    if (isRenderModeChangedByUser) material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }

        private static void SetupVertexColorBlendOp(Material material, UniUnlitVertexColorBlendOp vColBlendOp)
        {
            switch (vColBlendOp)
            {
                case UniUnlitVertexColorBlendOp.None:
                    SetKeyword(material, KeywordVertexColMul, false);
                    break;
                case UniUnlitVertexColorBlendOp.Multiply:
                    SetKeyword(material, KeywordVertexColMul, true);
                    break;
            }
        }

        private static void SetKeyword(Material mat, string keyword, bool required)
        {
            if (required)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// "Universal Render Pipeline/Lit" シェーダーのプロパティを操作するためのクラス
    ///
    /// glTF との読み書きに必要な機能だけ実装する
    ///
    /// 非対応項目
    /// - Detail Texture
    /// - Specular Highlights Toggle
    /// - Environment Reflections Toggle
    /// - Sorting Priority
    /// </summary>
    public class UrpLitContext : UrpBaseShaderContext
    {
        private static readonly int WorkflowMode = Shader.PropertyToID("_WorkflowMode");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        private static readonly int ParallaxMap = Shader.PropertyToID("_ParallaxMap");
        private static readonly int SmoothnessProp = Shader.PropertyToID("_Smoothness");
        private static readonly int SmoothnessTextureChannelProp = Shader.PropertyToID("_SmoothnessTextureChannel");
        private static readonly int MetallicProp = Shader.PropertyToID("_Metallic");
        private static readonly int MetallicGlossMapProp = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int SpecColorProp = Shader.PropertyToID("_SpecColor");
        private static readonly int SpecGlossMapProp = Shader.PropertyToID("_SpecGlossMap");
        private static readonly int BumpScaleProp = Shader.PropertyToID("_BumpScale");
        private static readonly int BumpMapProp = Shader.PropertyToID("_BumpMap");

        private static readonly string SpecularSetupKeyword = "_SPECULAR_SETUP";
        private static readonly string MetallicSpecGlossMapKeyword = "_METALLICSPECGLOSSMAP";
        private static readonly string SmoothnessTextureAlbedoChannelAKeyword = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
        private static readonly string NormalMapKeyword = "_NORMALMAP";
        private static readonly string EmissionKeyword = "_EMISSION";
        private static readonly string OcclusionMapKeyword = "_OCCLUSIONMAP";
        private static readonly string ParallaxMapKeyword = "_PARALLAXMAP";
        private static readonly int OcclusionStrengthProp = Shader.PropertyToID("_OcclusionStrength");
        private static readonly int ParallaxProp = Shader.PropertyToID("_Parallax");

        public UrpLitContext(Material material) : base(material)
        {

        }

        public UrpLitWorkflowType WorkflowType
        {
            get => (UrpLitWorkflowType)Material.GetFloat(WorkflowMode);
            set
            {
                Material.SetFloat(WorkflowMode, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public float Smoothness
        {
            get => Material.GetFloat(SmoothnessProp);
            set => Material.SetFloat(SmoothnessProp, value);
        }

        public UrpLitSmoothnessMapChannel SmoothnessTextureChannel
        {
            // NOTE: Float Prop 以外に条件があるので、Keyword から読み取った方が確実
            get => Material.IsKeywordEnabled(SmoothnessTextureAlbedoChannelAKeyword) ? UrpLitSmoothnessMapChannel.AlbedoAlpha : UrpLitSmoothnessMapChannel.SpecularMetallicAlpha;
            set
            {
                Material.SetFloat(SmoothnessTextureChannelProp, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public float Metallic
        {
            // NOTE: Metallic ワークフロー専用
            get => Material.GetFloat(MetallicProp);
            set => Material.SetFloat(MetallicProp, value);
        }

        public Texture MetallicGlossMap
        {
            // NOTE: Metallic ワークフロー専用
            get => Material.GetTexture(MetallicGlossMapProp);
            set => Material.SetTexture(MetallicGlossMapProp, value);
        }

        public Color SpecColorSrgb
        {
            // NOTE: Specular ワークフロー専用
            get => Material.GetColor(SpecColorProp);
            set => Material.SetColor(SpecColorProp, value);
        }

        public Texture SpecGlossMap
        {
            // NOTE: Specular ワークフロー専用
            get => Material.GetTexture(SpecGlossMapProp);
            set => Material.SetTexture(SpecGlossMapProp, value);
        }

        public float BumpScale
        {
            get => Material.GetFloat(BumpScaleProp);
            set => Material.SetFloat(BumpScaleProp, value);
        }

        public Texture BumpMap
        {
            get => Material.GetTexture(BumpMapProp);
            set
            {
                Material.SetTexture(BumpMapProp, value);
                Material.SetKeyword(NormalMapKeyword, value != null);
            }
        }

        public bool IsEmissionEnabled
        {
            get => Material.IsKeywordEnabled(EmissionKeyword);
            set
            {
                Material.SetKeyword(EmissionKeyword, value);
                if (value)
                {
                    Material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
                else
                {
                    Material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
            }
        }

        public Color EmissionColorLinear
        {
            get => Material.GetColor(EmissionColor);
            set => Material.SetColor(EmissionColor, value);
        }

        public Texture EmissionTexture
        {
            get => Material.GetTexture(EmissionMap);
            set => Material.SetTexture(EmissionMap, value);
        }

        public float OcclusionStrength
        {
            get => Material.GetFloat(OcclusionStrengthProp);
            set => Material.SetFloat(OcclusionStrengthProp, value);
        }

        public Texture OcclusionTexture
        {
            get => Material.GetTexture(OcclusionMap);
            set
            {
                Material.SetTexture(OcclusionMap, value);
                Material.SetKeyword(OcclusionMapKeyword, value != null);
            }
        }

        public float Parallax
        {
            get => Material.GetFloat(ParallaxProp);
            set => Material.SetFloat(ParallaxProp, value);
        }

        public Texture ParallaxTexture
        {
            get => Material.GetTexture(ParallaxMap);
            set
            {
                Material.SetTexture(ParallaxMap, value);
                Material.SetKeyword(ParallaxMapKeyword, value != null);
            }
        }

        /// <summary>
        /// 複数のプロパティに関連して設定されるキーワードやプロパティなどを更新する
        /// </summary>
        public override void Validate()
        {
            base.Validate();

            // Workflow
            var workflowType = (UrpLitWorkflowType)Material.GetFloat(WorkflowMode);
            var isSpecularSetup = workflowType == UrpLitWorkflowType.Specular;
            Material.SetKeyword(SpecularSetupKeyword, isSpecularSetup);

            // GlossMap
            var glossMapName = isSpecularSetup ? SpecGlossMapProp : MetallicGlossMapProp;
            var hasGlossMap = Material.GetTexture(glossMapName) != null;
            Material.SetKeyword(MetallicSpecGlossMapKeyword, hasGlossMap);

            // SmoothnessTextureChannel
            var isOpaque = SurfaceType == UrpLitSurfaceType.Opaque;
            var smoothnessMapChannel = (UrpLitSmoothnessMapChannel)Material.GetFloat(SmoothnessTextureChannelProp);
            Material.SetKeyword(SmoothnessTextureAlbedoChannelAKeyword, isOpaque && smoothnessMapChannel == UrpLitSmoothnessMapChannel.AlbedoAlpha);
        }
    }
}
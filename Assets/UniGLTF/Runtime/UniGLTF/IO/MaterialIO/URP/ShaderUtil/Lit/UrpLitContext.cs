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
    public class UrpLitContext
    {
        private readonly Material _mat;

        private static readonly int WorkflowMode = Shader.PropertyToID("_WorkflowMode");
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int Blend = Shader.PropertyToID("_Blend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        private static readonly int ParallaxMap = Shader.PropertyToID("_ParallaxMap");
        private static readonly int CutoffProp = Shader.PropertyToID("_Cutoff");
        private static readonly int SmoothnessProp = Shader.PropertyToID("_Smoothness");
        private static readonly int SmoothnessTextureChannelProp = Shader.PropertyToID("_SmoothnessTextureChannel");
        private static readonly int MetallicProp = Shader.PropertyToID("_Metallic");
        private static readonly int MetallicGlossMapProp = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int SpecColorProp = Shader.PropertyToID("_SpecColor");
        private static readonly int SpecGlossMapProp = Shader.PropertyToID("_SpecGlossMap");
        private static readonly int BumpScaleProp = Shader.PropertyToID("_BumpScale");
        private static readonly int BumpMapProp = Shader.PropertyToID("_BumpMap");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

        private static readonly string SpecularSetupKeyword = "_SPECULAR_SETUP";
        private static readonly string MetallicSpecGlossMapKeyword = "_METALLICSPECGLOSSMAP";
        private static readonly string SurfaceTypeTransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";
        private static readonly string SmoothnessTextureAlbedoChannelAKeyword = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
        private static readonly string NormalMapKeyword = "_NORMALMAP";
        private static readonly string EmissionKeyword = "_EMISSION";
        private static readonly string OcclusionMapKeyword = "_OCCLUSIONMAP";
        private static readonly string ParallaxMapKeyword = "_PARALLAXMAP";
        private static readonly string AlphaTestOnKeyword = "_ALPHATEST_ON";
        private static readonly string AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";
        private static readonly string AlphaModulateOnKeyword = "_ALPHAMODULATE_ON";
        private static readonly int OcclusionStrengthProp = Shader.PropertyToID("_OcclusionStrength");
        private static readonly int ParallaxProp = Shader.PropertyToID("_Parallax");

        public UrpLitContext(Material material)
        {
            _mat = material;
        }

        public Material Material => _mat;

        /// <summary>
        /// これが有効な場合、Validate() 関数が呼ばれるべき場合でも自動的に呼ばれなくなります。
        ///
        /// 処理最適化目的で使用できます。
        /// </summary>
        public bool UnsafeEditMode { get; set; } = false;

        public UrpLitWorkflowType WorkflowType
        {
            get => (UrpLitWorkflowType)_mat.GetFloat(WorkflowMode);
            set
            {
                _mat.SetFloat(WorkflowMode, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public UrpLitSurfaceType SurfaceType
        {
            get => (UrpLitSurfaceType)_mat.GetFloat(Surface);
            set
            {
                _mat.SetFloat(Surface, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public UrpLitBlendMode BlendMode
        {
            get => (UrpLitBlendMode)_mat.GetFloat(Blend);
            set
            {
                _mat.SetFloat(Blend, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public bool IsAlphaClipEnabled
        {
            get => _mat.GetFloat(AlphaClip) >= 0.5f;
            set
            {
                _mat.SetFloat(AlphaClip, value ? 1.0f : 0.0f);
                if (!UnsafeEditMode) Validate();
            }
        }

        public CullMode CullMode
        {
            get => (CullMode)_mat.GetFloat(Cull);
            set
            {
                _mat.SetFloat(Cull, (float)value);
                _mat.doubleSidedGI = value != CullMode.Back;
            }
        }

        public Color BaseColorSrgb
        {
            get => _mat.GetColor(BaseColorProp);
            set => _mat.SetColor(BaseColorProp, value);
        }

        public Texture BaseTexture
        {
            get => _mat.GetTexture(BaseMap);
            set => _mat.SetTexture(BaseMap, value);
        }

        public Vector2 BaseTextureOffset
        {
            get => _mat.GetTextureOffset(BaseMap);
            set => _mat.SetTextureOffset(BaseMap, value);
        }

        public Vector2 BaseTextureScale
        {
            get => _mat.GetTextureScale(BaseMap);
            set => _mat.SetTextureScale(BaseMap, value);
        }

        public float Cutoff
        {
            get => _mat.GetFloat(CutoffProp);
            set => _mat.SetFloat(CutoffProp, value);
        }

        public float Smoothness
        {
            get => _mat.GetFloat(SmoothnessProp);
            set => _mat.SetFloat(SmoothnessProp, value);
        }

        public UrpLitSmoothnessMapChannel SmoothnessTextureChannel
        {
            // NOTE: Float Prop 以外に条件があるので、Keyword から読み取った方が確実
            get => _mat.IsKeywordEnabled(SmoothnessTextureAlbedoChannelAKeyword) ? UrpLitSmoothnessMapChannel.AlbedoAlpha : UrpLitSmoothnessMapChannel.SpecularMetallicAlpha;
            set
            {
                _mat.SetFloat(SmoothnessTextureChannelProp, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public float Metallic
        {
            // NOTE: Metallic ワークフロー専用
            get => _mat.GetFloat(MetallicProp);
            set => _mat.SetFloat(MetallicProp, value);
        }

        public Texture MetallicGlossMap
        {
            // NOTE: Metallic ワークフロー専用
            get => _mat.GetTexture(MetallicGlossMapProp);
            set => _mat.SetTexture(MetallicGlossMapProp, value);
        }

        public Color SpecColorSrgb
        {
            // NOTE: Specular ワークフロー専用
            get => _mat.GetColor(SpecColorProp);
            set => _mat.SetColor(SpecColorProp, value);
        }

        public Texture SpecGlossMap
        {
            // NOTE: Specular ワークフロー専用
            get => _mat.GetTexture(SpecGlossMapProp);
            set => _mat.SetTexture(SpecGlossMapProp, value);
        }

        public float BumpScale
        {
            get => _mat.GetFloat(BumpScaleProp);
            set => _mat.SetFloat(BumpScaleProp, value);
        }

        public Texture BumpMap
        {
            get => _mat.GetTexture(BumpMapProp);
            set
            {
                _mat.SetTexture(BumpMapProp, value);
                _mat.SetKeyword(NormalMapKeyword, value != null);
            }
        }

        public bool IsEmissionEnabled
        {
            get => _mat.IsKeywordEnabled(EmissionKeyword);
            set
            {
                _mat.SetKeyword(EmissionKeyword, value);
                if (value)
                {
                    _mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
                else
                {
                    _mat.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
            }
        }

        public Color EmissionColorLinear
        {
            get => _mat.GetColor(EmissionColor);
            set => _mat.SetColor(EmissionColor, value);
        }

        public Texture EmissionTexture
        {
            get => _mat.GetTexture(EmissionMap);
            set => _mat.SetTexture(EmissionMap, value);
        }

        public float OcclusionStrength
        {
            get => _mat.GetFloat(OcclusionStrengthProp);
            set => _mat.SetFloat(OcclusionStrengthProp, value);
        }

        public Texture OcclusionTexture
        {
            get => _mat.GetTexture(OcclusionMap);
            set
            {
                _mat.SetTexture(OcclusionMap, value);
                _mat.SetKeyword(OcclusionMapKeyword, value != null);
            }
        }

        public float Parallax
        {
            get => _mat.GetFloat(ParallaxProp);
            set => _mat.SetFloat(ParallaxProp, value);
        }

        public Texture ParallaxTexture
        {
            get => _mat.GetTexture(ParallaxMap);
            set
            {
                _mat.SetTexture(ParallaxMap, value);
                _mat.SetKeyword(ParallaxMapKeyword, value != null);
            }
        }

        /// <summary>
        /// 複数のプロパティに関連して設定されるキーワードやプロパティなどを更新する
        /// </summary>
        public void Validate()
        {
            // Workflow
            var workflowType = (UrpLitWorkflowType)_mat.GetFloat(WorkflowMode);
            var isSpecularSetup = workflowType == UrpLitWorkflowType.Specular;
            _mat.SetKeyword(SpecularSetupKeyword, isSpecularSetup);

            // GlossMap
            var glossMapName = isSpecularSetup ? SpecGlossMapProp : MetallicGlossMapProp;
            var hasGlossMap = _mat.GetTexture(glossMapName) != null;
            _mat.SetKeyword(MetallicSpecGlossMapKeyword, hasGlossMap);

            // Surface Type
            var surfaceType = (UrpLitSurfaceType)_mat.GetFloat(Surface);
            _mat.SetKeyword(SurfaceTypeTransparentKeyword, surfaceType != UrpLitSurfaceType.Opaque);

            // SmoothnessTextureChannel
            var isOpaque = surfaceType == UrpLitSurfaceType.Opaque;
            var smoothnessMapChannel = (UrpLitSmoothnessMapChannel)_mat.GetFloat(SmoothnessTextureChannelProp);
            _mat.SetKeyword(SmoothnessTextureAlbedoChannelAKeyword, isOpaque && smoothnessMapChannel == UrpLitSmoothnessMapChannel.AlbedoAlpha);

            // Alpha Clip
            var alphaClip = _mat.GetFloat(AlphaClip) >= 0.5f;
            _mat.SetKeyword(AlphaTestOnKeyword, alphaClip);

            // Blend Mode
            var blendMode = (UrpLitBlendMode)_mat.GetFloat(Blend);
            _mat.SetKeyword(AlphaPremultiplyOnKeyword, blendMode == UrpLitBlendMode.Premultiply);
            _mat.SetKeyword(AlphaModulateOnKeyword, blendMode == UrpLitBlendMode.Additive);

            // ZWrite
            var zWrite = surfaceType == UrpLitSurfaceType.Opaque;
            _mat.SetFloat(ZWrite, zWrite ? 1.0f : 0.0f);
            _mat.SetShaderPassEnabled("DepthOnly", zWrite);

            // Render Settings
            _mat.SetFloat(SrcBlend, (surfaceType, blendMode) switch
            {
                (UrpLitSurfaceType.Opaque, _) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Alpha) => (float)UnityEngine.Rendering.BlendMode.SrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Premultiply) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Additive) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Multiply) => (float)UnityEngine.Rendering.BlendMode.DstColor,
                _ => (float)UnityEngine.Rendering.BlendMode.One,
            });
            _mat.SetFloat(DstBlend, (surfaceType, blendMode) switch
            {
                (UrpLitSurfaceType.Opaque, _) => (float)UnityEngine.Rendering.BlendMode.Zero,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Alpha) => (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Premultiply) => (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Additive) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Multiply) => (float)UnityEngine.Rendering.BlendMode.Zero,
                _ => (float) UnityEngine.Rendering.BlendMode.Zero,
            });
            _mat.SetOverrideTag("RenderType", (surfaceType, alphaClip) switch
            {
                (UrpLitSurfaceType.Opaque, false) => "Opaque",
                (UrpLitSurfaceType.Opaque, true) => "TransparentCutout",
                (UrpLitSurfaceType.Transparent, _) => "Transparent",
                _ => "Opaque",
            });
            _mat.renderQueue = (surfaceType, alphaClip) switch
            {
                (UrpLitSurfaceType.Opaque, false) => (int)RenderQueue.Geometry,
                (UrpLitSurfaceType.Opaque, true) => (int)RenderQueue.AlphaTest,
                (UrpLitSurfaceType.Transparent, _) => (int)RenderQueue.Transparent,
                _ => _mat.shader.renderQueue,
            };
        }
    }
}
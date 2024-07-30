using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public sealed class UrpLitContext
    {
        private readonly Material _mat;

        private static readonly int WorkflowMode = Shader.PropertyToID("_WorkflowMode");
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        private static readonly int ParallaxMap = Shader.PropertyToID("_ParallaxMap");
        private static readonly int CutoffProp = Shader.PropertyToID("_Cutoff");
        private static readonly int SmoothnessProp = Shader.PropertyToID("_Smoothness");
        private static readonly int SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        private static readonly int MetallicProp = Shader.PropertyToID("_Metallic");
        private static readonly int MetallicGlossMapProp = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int SpecColorProp = Shader.PropertyToID("_SpecColor");
        private static readonly int SpecGlossMapProp = Shader.PropertyToID("_SpecGlossMap");
        private static readonly int BumpScaleProp = Shader.PropertyToID("_BumpScale");
        private static readonly int BumpMapProp = Shader.PropertyToID("_BumpMap");
        private static readonly int EmissionEnabled = Shader.PropertyToID("_EmissionEnabled");

        private static readonly string SpecularSetupKeyword = "_SPECULAR_SETUP";
        private static readonly string MetallicSpecGlossMapKeyword = "_METALLICSPECGLOSSMAP";
        private static readonly string SurfaceTypeTransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";
        private static readonly string SmoothnessTextureAlbedoChannelAKeyword = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
        private static readonly string NormalMapKeyword = "_NORMALMAP";
        private static readonly string EmissionKeyword = "_EMISSION";
        private static readonly string OcclusionMapKeyword = "_OCCLUSIONMAP";
        private static readonly string ParallaxMapKeyword = "_PARALLAXMAP";

        public UrpLitContext(Material material)
        {
            _mat = material;
        }

        public UrpLitWorkflowType WorkflowType
        {
            get => (UrpLitWorkflowType)_mat.GetFloat(WorkflowMode);
            set
            {
                _mat.SetFloat(WorkflowMode, (float)value);
                var isSpecularSetup = value == UrpLitWorkflowType.Specular;
                _mat.SetKeyword(SpecularSetupKeyword, isSpecularSetup);

                var glossMapName = isSpecularSetup ? SpecGlossMapProp : MetallicGlossMapProp;
                var hasGlossMap = _mat.GetTexture(glossMapName) != null;
                _mat.SetKeyword(MetallicSpecGlossMapKeyword, hasGlossMap);
            }
        }

        public UrpLitSurfaceType SurfaceType
        {
            get => (UrpLitSurfaceType)_mat.GetFloat(Surface);
            set
            {
                _mat.SetFloat(Surface, (float)value);
                _mat.SetKeyword(SurfaceTypeTransparentKeyword, value != UrpLitSurfaceType.Opaque);
            }
        }

        public bool AlphaClipping
        {
            get => _mat.GetFloat(AlphaClip) >= 0.5f;
            set => _mat.SetFloat(AlphaClip, value ? 1.0f : 0.0f);
        }

        public CullMode CullMode
        {
            get => (CullMode)_mat.GetFloat(Cull);
            set => _mat.SetFloat(Cull, (float)value);
        }

        public Color BaseColorSrgb
        {
            get => _mat.GetColor(BaseColorProp);
            set => _mat.SetColor(BaseColorProp, value);
        }

        public Texture BaseTexture
        {
            get => _mat.GetTexture(BaseMap);
        }

        public Vector2 BaseTextureOffset
        {
            get => _mat.GetTextureOffset(BaseMap);
        }

        public Vector2 BaseTextureScale
        {
            get => _mat.GetTextureScale(BaseMap);
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

        public UrpLitSmoothnessMapChannel SmoothnessMapChannel
        {
            // NOTE: Float Prop 以外に条件があるので、Keyword から読み取った方が確実
            get => _mat.IsKeywordEnabled(SmoothnessTextureAlbedoChannelAKeyword) ? UrpLitSmoothnessMapChannel.AlbedoAlpha : UrpLitSmoothnessMapChannel.SpecularMetallicAlpha;
            set
            {
                _mat.SetFloat(SmoothnessTextureChannel, (float)value);
                var isOpaque = SurfaceType == UrpLitSurfaceType.Opaque;
                _mat.SetKeyword(SmoothnessTextureAlbedoChannelAKeyword, isOpaque && value == UrpLitSmoothnessMapChannel.AlbedoAlpha);
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
                _mat.SetFloat(EmissionEnabled, value ? 1.0f : 0.0f);
                _mat.SetKeyword(EmissionKeyword, value);
            }
        }

        public Color EmissionLinear
        {
            get => _mat.GetColor(EmissionColor);
        }

        public Texture EmissionTexture
        {
            get => _mat.GetTexture(EmissionMap);
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

        public Texture ParallaxTexture
        {
            get => _mat.GetTexture(ParallaxMap);
            set
            {
                _mat.SetTexture(ParallaxMap, value);
                _mat.SetKeyword(ParallaxMapKeyword, value != null);
            }
        }
    }
}
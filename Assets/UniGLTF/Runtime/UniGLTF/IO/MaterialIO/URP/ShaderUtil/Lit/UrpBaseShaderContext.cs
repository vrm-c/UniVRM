using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public abstract class UrpBaseShaderContext
    {
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int Blend = Shader.PropertyToID("_Blend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int CutoffProp = Shader.PropertyToID("_Cutoff");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly string SurfaceTypeTransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";
        private static readonly string AlphaTestOnKeyword = "_ALPHATEST_ON";
        private static readonly string AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";
        private static readonly string AlphaModulateOnKeyword = "_ALPHAMODULATE_ON";

        protected UrpBaseShaderContext(Material material)
        {
            Material = material;
        }

        public Material Material { get; }

        /// <summary>
        /// これが有効な場合、Validate() 関数が呼ばれるべき場合でも自動的に呼ばれなくなります。
        ///
        /// 処理最適化目的で使用できます。
        /// </summary>
        public bool UnsafeEditMode { get; set; } = false;

        public UrpLitSurfaceType SurfaceType
        {
            get => (UrpLitSurfaceType)Material.GetFloat(Surface);
            set
            {
                Material.SetFloat(Surface, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public UrpLitBlendMode BlendMode
        {
            get => (UrpLitBlendMode)Material.GetFloat(Blend);
            set
            {
                Material.SetFloat(Blend, (float)value);
                if (!UnsafeEditMode) Validate();
            }
        }

        public bool IsAlphaClipEnabled
        {
            get => Material.GetFloat(AlphaClip) >= 0.5f;
            set
            {
                Material.SetFloat(AlphaClip, value ? 1.0f : 0.0f);
                if (!UnsafeEditMode) Validate();
            }
        }

        public CullMode CullMode
        {
            get => (CullMode)Material.GetFloat(Cull);
            set
            {
                Material.SetFloat(Cull, (float)value);
                Material.doubleSidedGI = value != CullMode.Back;
            }
        }

        public Color BaseColorSrgb
        {
            get => Material.GetColor(BaseColorProp);
            set => Material.SetColor(BaseColorProp, value);
        }

        public Texture BaseTexture
        {
            get => Material.GetTexture(BaseMap);
            set => Material.SetTexture(BaseMap, value);
        }

        public Vector2 BaseTextureOffset
        {
            get => Material.GetTextureOffset(BaseMap);
            set => Material.SetTextureOffset(BaseMap, value);
        }

        public Vector2 BaseTextureScale
        {
            get => Material.GetTextureScale(BaseMap);
            set => Material.SetTextureScale(BaseMap, value);
        }

        public float Cutoff
        {
            get => Material.GetFloat(CutoffProp);
            set => Material.SetFloat(CutoffProp, value);
        }


        public virtual void Validate()
        {
            // Surface Type
            var surfaceType = (UrpLitSurfaceType)Material.GetFloat(Surface);
            Material.SetKeyword(SurfaceTypeTransparentKeyword, surfaceType != UrpLitSurfaceType.Opaque);

            // Alpha Clip
            var alphaClip = Material.GetFloat(AlphaClip) >= 0.5f;
            Material.SetKeyword(AlphaTestOnKeyword, alphaClip);

            // Blend Mode
            var blendMode = (UrpLitBlendMode)Material.GetFloat(Blend);
            Material.SetKeyword(AlphaPremultiplyOnKeyword, blendMode == UrpLitBlendMode.Premultiply);
            Material.SetKeyword(AlphaModulateOnKeyword, blendMode == UrpLitBlendMode.Additive);

            // ZWrite
            var zWrite = surfaceType == UrpLitSurfaceType.Opaque;
            Material.SetFloat(ZWrite, zWrite ? 1.0f : 0.0f);
            Material.SetShaderPassEnabled("DepthOnly", zWrite);

            // Render Settings
            Material.SetFloat(SrcBlend, (surfaceType, blendMode) switch
            {
                (UrpLitSurfaceType.Opaque, _) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Alpha) => (float)UnityEngine.Rendering.BlendMode.SrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Premultiply) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Additive) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Multiply) => (float)UnityEngine.Rendering.BlendMode.DstColor,
                _ => (float)UnityEngine.Rendering.BlendMode.One,
            });
            Material.SetFloat(DstBlend, (surfaceType, blendMode) switch
            {
                (UrpLitSurfaceType.Opaque, _) => (float)UnityEngine.Rendering.BlendMode.Zero,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Alpha) => (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Premultiply) => (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Additive) => (float)UnityEngine.Rendering.BlendMode.One,
                (UrpLitSurfaceType.Transparent, UrpLitBlendMode.Multiply) => (float)UnityEngine.Rendering.BlendMode.Zero,
                _ => (float) UnityEngine.Rendering.BlendMode.Zero,
            });
            Material.SetOverrideTag("RenderType", (surfaceType, alphaClip) switch
            {
                (UrpLitSurfaceType.Opaque, false) => "Opaque",
                (UrpLitSurfaceType.Opaque, true) => "TransparentCutout",
                (UrpLitSurfaceType.Transparent, _) => "Transparent",
                _ => "Opaque",
            });
            Material.renderQueue = (surfaceType, alphaClip) switch
            {
                (UrpLitSurfaceType.Opaque, false) => (int)RenderQueue.Geometry,
                (UrpLitSurfaceType.Opaque, true) => (int)RenderQueue.AlphaTest,
                (UrpLitSurfaceType.Transparent, _) => (int)RenderQueue.Transparent,
                _ => Material.shader.renderQueue,
            };
        }
    }
}
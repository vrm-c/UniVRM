using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// glTF 準拠 PBR ShaderGraph (UniGltfPbr) のマテリアルプロパティを操作するクラス。
    /// import (書き込み) / export (読み取り) の双方で使う。
    ///
    /// プロパティ名は glTF 2.0 仕様ネイティブ (baseColorFactor / metallicRoughnessTexture ...)。
    /// テクスチャは glTF のパッキングをそのまま (roughness 未反転, ORM 未変換) 格納し、
    /// roughness->smoothness などの変換は ShaderGraph 内の per-fragment 演算で行う。
    ///
    /// Surface(Opaque/Transparent) / Blend / Cull / AlphaClip は ShaderGraph の
    /// "Allow Material Override" により露出するプロパティで制御する。
    /// 1 つの .shadergraph に Built-In / URP の 2 Target を同居させるため、
    /// URP 系 (_Surface ...) と Built-In 系 (_BUILTIN_* ...) の両方を HasProperty ガード付きで扱う。
    ///
    /// 参考: UniGLTF.UrpLitContext / UniGLTF.UrpBaseShaderContext
    /// </summary>
    public sealed class UniGltfPbrContext
    {
        public const string ShaderName = "UniGLTF/UniGltfPbr";
        public static Shader GetShader() => Shader.Find(ShaderName);
        
        // --- glTF native pbr properties ---
        private static readonly int BaseColorFactorProp = Shader.PropertyToID("_baseColorFactor");
        private static readonly int BaseColorTextureProp = Shader.PropertyToID("_baseColorTexture");
        private static readonly int MetallicFactorProp = Shader.PropertyToID("_metallicFactor");
        private static readonly int RoughnessFactorProp = Shader.PropertyToID("_roughnessFactor");
        private static readonly int MetallicRoughnessTextureProp = Shader.PropertyToID("_metallicRoughnessTexture");
        private static readonly int OcclusionTextureProp = Shader.PropertyToID("_occlusionTexture");
        private static readonly int OcclusionStrengthProp = Shader.PropertyToID("_occlusionStrength");
        private static readonly int NormalTextureProp = Shader.PropertyToID("_normalTexture");
        private static readonly int NormalScaleProp = Shader.PropertyToID("_normalScale");
        private static readonly int EmissiveFactorProp = Shader.PropertyToID("_emissiveFactor");
        private static readonly int EmissiveTextureProp = Shader.PropertyToID("_emissiveTexture");
        private static readonly int EmissiveStrengthProp = Shader.PropertyToID("_emissiveStrength");
        private static readonly int AlphaCutoffProp = Shader.PropertyToID("_alphaCutoff");

        // --- surface state (URP ShaderGraph target / stock URP Lit compatible) ---
        private static readonly int UrpSurfaceProp = Shader.PropertyToID("_Surface");
        private static readonly int UrpBlendProp = Shader.PropertyToID("_Blend");
        private static readonly int UrpAlphaClipProp = Shader.PropertyToID("_AlphaClip");
        private static readonly int UrpCullProp = Shader.PropertyToID("_Cull");
        private static readonly int UrpZWriteProp = Shader.PropertyToID("_ZWrite");
        private static readonly int UrpSrcBlendProp = Shader.PropertyToID("_SrcBlend");
        private static readonly int UrpDstBlendProp = Shader.PropertyToID("_DstBlend");
        private static readonly int UrpCutoffProp = Shader.PropertyToID("_Cutoff");
        private const string UrpSurfaceTypeTransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";
        private const string UrpAlphaTestOnKeyword = "_ALPHATEST_ON";

        // --- surface state (Built-In ShaderGraph target) ---
        private static readonly int BuiltInSurfaceProp = Shader.PropertyToID("_BUILTIN_Surface");
        private static readonly int BuiltInBlendProp = Shader.PropertyToID("_BUILTIN_Blend");
        private static readonly int BuiltInAlphaClipProp = Shader.PropertyToID("_BUILTIN_AlphaClip");
        private static readonly int BuiltInCullProp = Shader.PropertyToID("_BUILTIN_CullMode");
        private static readonly int BuiltInZWriteProp = Shader.PropertyToID("_BUILTIN_ZWrite");
        private static readonly int BuiltInSrcBlendProp = Shader.PropertyToID("_BUILTIN_SrcBlend");
        private static readonly int BuiltInDstBlendProp = Shader.PropertyToID("_BUILTIN_DstBlend");
        private const string BuiltInSurfaceTypeTransparentKeyword = "_BUILTIN_SURFACE_TYPE_TRANSPARENT";
        private const string BuiltInAlphaTestOnKeyword = "_BUILTIN_ALPHATEST_ON";

        public Material Material { get; }

        private readonly HashSet<string> _declaredKeywords;

        public UniGltfPbrContext(Material material)
        {
            Material = material;
            _declaredKeywords = new HashSet<string>();
            if (material != null && material.shader != null)
            {
                foreach (var kw in material.shader.keywordSpace.keywordNames)
                {
                    _declaredKeywords.Add(kw);
                }
            }
        }

        // ---- base color ----

        /// <summary>
        /// _baseColorFactor は ShaderGraph "Default" カラー (sRGB 格納) 想定。
        /// glTF の linear baseColorFactor は .gamma して格納し、export 時に sRGB->Linear で戻す。
        /// (UniGLTF.UrpBaseShaderContext.BaseColorSrgb と同じ規約)
        /// </summary>
        public Color BaseColorSrgb
        {
            get => Material.GetColor(BaseColorFactorProp);
            set => Material.SetColor(BaseColorFactorProp, value);
        }

        public Texture BaseColorTexture
        {
            get => Material.GetTexture(BaseColorTextureProp);
            set => Material.SetTexture(BaseColorTextureProp, value);
        }

        public Vector2 BaseColorTextureOffset
        {
            get => Material.GetTextureOffset(BaseColorTextureProp);
            set => Material.SetTextureOffset(BaseColorTextureProp, value);
        }

        public Vector2 BaseColorTextureScale
        {
            get => Material.GetTextureScale(BaseColorTextureProp);
            set => Material.SetTextureScale(BaseColorTextureProp, value);
        }

        // ---- metallic roughness (glTF native packing, no conversion) ----

        public float MetallicFactor
        {
            get => Material.GetFloat(MetallicFactorProp);
            set => Material.SetFloat(MetallicFactorProp, value);
        }

        public float RoughnessFactor
        {
            get => Material.GetFloat(RoughnessFactorProp);
            set => Material.SetFloat(RoughnessFactorProp, value);
        }

        /// <summary>
        /// glTF metallicRoughnessTexture をそのまま (G=roughness, B=metallic) 格納する。
        /// </summary>
        public Texture MetallicRoughnessTexture
        {
            get => Material.GetTexture(MetallicRoughnessTextureProp);
            set => Material.SetTexture(MetallicRoughnessTextureProp, value);
        }

        public Vector2 MetallicRoughnessTextureOffset
        {
            get => Material.GetTextureOffset(MetallicRoughnessTextureProp);
            set => Material.SetTextureOffset(MetallicRoughnessTextureProp, value);
        }

        public Vector2 MetallicRoughnessTextureScale
        {
            get => Material.GetTextureScale(MetallicRoughnessTextureProp);
            set => Material.SetTextureScale(MetallicRoughnessTextureProp, value);
        }

        // ---- occlusion ----

        public float OcclusionStrength
        {
            get => Material.GetFloat(OcclusionStrengthProp);
            set => Material.SetFloat(OcclusionStrengthProp, value);
        }

        /// <summary>
        /// glTF occlusionTexture をそのまま (R=occlusion) 格納する。
        /// metallicRoughnessTexture と同一テクスチャ (ORM 1 枚) の場合もある。
        /// </summary>
        public Texture OcclusionTexture
        {
            get => Material.GetTexture(OcclusionTextureProp);
            set => Material.SetTexture(OcclusionTextureProp, value);
        }

        public Vector2 OcclusionTextureOffset
        {
            get => Material.GetTextureOffset(OcclusionTextureProp);
            set => Material.SetTextureOffset(OcclusionTextureProp, value);
        }

        public Vector2 OcclusionTextureScale
        {
            get => Material.GetTextureScale(OcclusionTextureProp);
            set => Material.SetTextureScale(OcclusionTextureProp, value);
        }

        // ---- normal ----

        public float NormalScale
        {
            get => Material.GetFloat(NormalScaleProp);
            set => Material.SetFloat(NormalScaleProp, value);
        }

        public Texture NormalTexture
        {
            get => Material.GetTexture(NormalTextureProp);
            set => Material.SetTexture(NormalTextureProp, value);
        }

        public Vector2 NormalTextureOffset
        {
            get => Material.GetTextureOffset(NormalTextureProp);
            set => Material.SetTextureOffset(NormalTextureProp, value);
        }

        public Vector2 NormalTextureScale
        {
            get => Material.GetTextureScale(NormalTextureProp);
            set => Material.SetTextureScale(NormalTextureProp, value);
        }

        // ---- emissive ----

        /// <summary>
        /// _emissiveFactor は HDR カラー (linear 直格納) 想定。
        /// (UniGLTF.UrpLitContext.EmissionColorLinear と同じ規約)
        /// </summary>
        public Color EmissiveFactorLinear
        {
            get => Material.GetColor(EmissiveFactorProp);
            set => Material.SetColor(EmissiveFactorProp, value);
        }

        public Texture EmissiveTexture
        {
            get => Material.GetTexture(EmissiveTextureProp);
            set => Material.SetTexture(EmissiveTextureProp, value);
        }

        public Vector2 EmissiveTextureOffset
        {
            get => Material.GetTextureOffset(EmissiveTextureProp);
            set => Material.SetTextureOffset(EmissiveTextureProp, value);
        }

        public Vector2 EmissiveTextureScale
        {
            get => Material.GetTextureScale(EmissiveTextureProp);
            set => Material.SetTextureScale(EmissiveTextureProp, value);
        }

        /// <summary>
        /// KHR_materials_emissive_strength の emissiveStrength (default 1)。
        /// </summary>
        public float EmissiveStrength
        {
            get => Material.HasProperty(EmissiveStrengthProp) ? Material.GetFloat(EmissiveStrengthProp) : 1.0f;
            set => Material.SetFloat(EmissiveStrengthProp, value);
        }

        public float AlphaCutoff
        {
            get => Material.GetFloat(AlphaCutoffProp);
            set => Material.SetFloat(AlphaCutoffProp, value);
        }

        // ---- surface / alpha / cull (material-backed; URP と Built-In の両プロパティを設定) ----

        public UniGltfPbrSurfaceType SurfaceType
        {
            get
            {
                if (Material.HasProperty(UrpSurfaceProp)) return (UniGltfPbrSurfaceType)(int)Material.GetFloat(UrpSurfaceProp);
                if (Material.HasProperty(BuiltInSurfaceProp)) return (UniGltfPbrSurfaceType)(int)Material.GetFloat(BuiltInSurfaceProp);
                return UniGltfPbrSurfaceType.Opaque;
            }
            set
            {
                SetFloatIfExists(UrpSurfaceProp, (float)value);
                SetFloatIfExists(BuiltInSurfaceProp, (float)value);
            }
        }

        public bool IsAlphaClipEnabled
        {
            get
            {
                if (Material.HasProperty(UrpAlphaClipProp)) return Material.GetFloat(UrpAlphaClipProp) >= 0.5f;
                if (Material.HasProperty(BuiltInAlphaClipProp)) return Material.GetFloat(BuiltInAlphaClipProp) >= 0.5f;
                return false;
            }
            set
            {
                SetFloatIfExists(UrpAlphaClipProp, value ? 1.0f : 0.0f);
                SetFloatIfExists(BuiltInAlphaClipProp, value ? 1.0f : 0.0f);
            }
        }

        public CullMode CullMode
        {
            get
            {
                if (Material.HasProperty(UrpCullProp)) return (CullMode)(int)Material.GetFloat(UrpCullProp);
                if (Material.HasProperty(BuiltInCullProp)) return (CullMode)(int)Material.GetFloat(BuiltInCullProp);
                return CullMode.Back;
            }
            set
            {
                SetFloatIfExists(UrpCullProp, (float)value);
                SetFloatIfExists(BuiltInCullProp, (float)value);
                Material.doubleSidedGI = value != CullMode.Back;
            }
        }

        /// <summary>
        /// 現在の SurfaceType / IsAlphaClipEnabled から、派生するキーワード・ブレンド・renderQueue を反映する。
        /// (surface/cull の float 自体は setter で設定済み)
        /// 存在しないプロパティは HasProperty で読み飛ばす。
        /// </summary>
        public void Validate()
        {
            var isOpaque = SurfaceType == UniGltfPbrSurfaceType.Opaque;
            var alphaClip = IsAlphaClipEnabled;

            Material.SetOverrideTag("RenderType", (isOpaque, alphaClip) switch
            {
                (true, false) => "Opaque",
                (true, true) => "TransparentCutout",
                (false, _) => "Transparent",
            });
            Material.renderQueue = (isOpaque, alphaClip) switch
            {
                (true, false) => (int)RenderQueue.Geometry,
                (true, true) => (int)RenderQueue.AlphaTest,
                (false, _) => (int)RenderQueue.Transparent,
            };

            var srcBlend = isOpaque ? (float)BlendMode.One : (float)BlendMode.SrcAlpha;
            var dstBlend = isOpaque ? (float)BlendMode.Zero : (float)BlendMode.OneMinusSrcAlpha;
            var zWrite = isOpaque ? 1.0f : 0.0f;

            // URP target
            SetFloatIfExists(UrpBlendProp, 0.0f); // 0 == Alpha
            SetFloatIfExists(UrpZWriteProp, zWrite);
            SetFloatIfExists(UrpSrcBlendProp, srcBlend);
            SetFloatIfExists(UrpDstBlendProp, dstBlend);
            SetKeywordIfDeclared(UrpSurfaceTypeTransparentKeyword, !isOpaque);
            SetKeywordIfDeclared(UrpAlphaTestOnKeyword, alphaClip);
            Material.SetShaderPassEnabled("DepthOnly", isOpaque);
            Material.SetShaderPassEnabled("ShadowCaster", isOpaque);

            // Built-In target
            SetFloatIfExists(BuiltInBlendProp, 0.0f);
            SetFloatIfExists(BuiltInZWriteProp, zWrite);
            SetFloatIfExists(BuiltInSrcBlendProp, srcBlend);
            SetFloatIfExists(BuiltInDstBlendProp, dstBlend);
            SetKeywordIfDeclared(BuiltInSurfaceTypeTransparentKeyword, !isOpaque);
            SetKeywordIfDeclared(BuiltInAlphaTestOnKeyword, alphaClip);

            // cutoff: URP の _Cutoff と本シェーダ独自の _alphaCutoff の両方があれば揃える
            if (Material.HasProperty(UrpCutoffProp) && Material.HasProperty(AlphaCutoffProp))
            {
                Material.SetFloat(UrpCutoffProp, Material.GetFloat(AlphaCutoffProp));
            }
        }

        private void SetFloatIfExists(int prop, float value)
        {
            if (Material.HasProperty(prop))
            {
                Material.SetFloat(prop, value);
            }
        }

        private void SetKeywordIfDeclared(string keyword, bool enabled)
        {
            if (_declaredKeywords.Contains(keyword))
            {
                if (enabled)
                {
                    Material.EnableKeyword(keyword);
                }
                else
                {
                    Material.DisableKeyword(keyword);
                }
            }
        }
    }
}

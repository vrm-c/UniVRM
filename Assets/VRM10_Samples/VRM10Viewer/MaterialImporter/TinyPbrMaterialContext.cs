using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class TinyPbrMaterialContext
    {
        //
        // When using shadergraph, you need to expose the following properties.
        //

        /// <summary>
        /// Color = White
        /// </summary>
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        /// <summary>
        /// Texture2D = white
        /// When using shadergraph, require "Set as Main Texture"
        /// </summary>
        private static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");
        /// <summary>
        /// float = 1.0
        /// </summary>
        private static readonly int OcclusionStrengthProp = Shader.PropertyToID("_OcclusionStrength");
        /// <summary>
        /// Texture2D.Red = 1.0.
        /// </summary>
        private static readonly int OcclusionMapProp = Shader.PropertyToID("_OcclusionMap");
        private static readonly int RoughnessProp = Shader.PropertyToID("_Roughness");
        private static readonly int MetallicProp = Shader.PropertyToID("_Metallic");
        /// <summary>
        /// Texture2D.Green = 1.0. The roughness
        /// Texture2D.Blue = 1.0. The metalness
        /// </summary>
        private static readonly int MetallicRoughnessMapProp = Shader.PropertyToID("_MetallicRoughnessMap");
        /// <summary>
        /// float = 1.0
        /// </summary>
        private static readonly int BumpScaleProp = Shader.PropertyToID("_BumpScale");
        /// <summary>
        /// Texture2D = [0, 0, 1.0]
        /// </summary>
        private static readonly int BumpMapProp = Shader.PropertyToID("_BumpMap");
        /// <summary>
        /// Color = black
        /// </summary>
        private static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");
        /// <summary>
        /// Texture2D = black
        /// </summary>
        private static readonly int EmissionMapProp = Shader.PropertyToID("_EmissionMap");
        /// <summary>
        /// boolean keyword
        /// </summary>
        private static readonly int CutoffEnabledProp = Shader.PropertyToID("_CutoffEnabled");
        /// <summary>
        /// float = 0.5
        /// </summary>
        private static readonly int CutoffProp = Shader.PropertyToID("_Cutoff");

        public readonly Material Material;
        public TinyPbrMaterialContext(Material material)
        {
            Material = material;
        }

        public Color BaseColorSrgb
        {
            get => Material.GetColor(BaseColorProp);
            set => Material.SetColor(BaseColorProp, value);
        }
        public Texture BaseTexture
        {
            get => Material.GetTexture(BaseMapProp);
            set => Material.SetTexture(BaseMapProp, value);
        }
        public Vector2 BaseTextureOffset
        {
            get => Material.GetTextureOffset(BaseMapProp);
            set => Material.SetTextureOffset(BaseMapProp, value);
        }
        public Vector2 BaseTextureScale
        {
            get => Material.GetTextureScale(BaseMapProp);
            set => Material.SetTextureScale(BaseMapProp, value);
        }

        public float OcclusionStrength
        {
            get => Material.GetFloat(OcclusionStrengthProp);
            set => Material.SetFloat(OcclusionStrengthProp, value);
        }
        public Texture OcclusionTexture
        {
            get => Material.GetTexture(OcclusionMapProp);
            set
            {
                Material.SetTexture(OcclusionMapProp, value);
            }
        }

        public float Roughness
        {
            get => Material.GetFloat(RoughnessProp);
            set => Material.SetFloat(RoughnessProp, value);
        }
        public float Metallic
        {
            get => Material.GetFloat(MetallicProp);
            set => Material.SetFloat(MetallicProp, value);
        }
        public Texture MetallicRoughnessMap
        {
            get => Material.GetTexture(MetallicRoughnessMapProp);
            set => Material.SetTexture(MetallicRoughnessMapProp, value);
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
            }
        }

        public Color EmissionColorLinear
        {
            get => Material.GetColor(EmissionColorProp);
            set => Material.SetColor(EmissionColorProp, value);
        }
        public Texture EmissionTexture
        {
            get => Material.GetTexture(EmissionMapProp);
            set => Material.SetTexture(EmissionMapProp, value);
        }

        public bool CutoffEnabled
        {
            get => Material.GetInt(CutoffEnabledProp) != 0;
            set => Material.SetInt(CutoffEnabledProp, value ? 1 : 0);
        }

        public float Cutoff
        {
            get => Material.GetFloat(CutoffProp);
            set => Material.SetFloat(CutoffProp, value);
        }
    }
}
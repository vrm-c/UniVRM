using System.Dynamic;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class TinyMToonMaterialContext
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

        private static readonly int ShadingColorFactorProp = Shader.PropertyToID("_ShadingColor");
        private static readonly int ShadingMapProp = Shader.PropertyToID("_ShadingMap");
        private static readonly int ShadingToonyFactorProp = Shader.PropertyToID("_ShadingToonyFactor");
        private static readonly int ShadingShiftFactorProp = Shader.PropertyToID("_ShadingShiftFactor");

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
        public TinyMToonMaterialContext(Material material)
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

        public Color ShadingColorFactorSrgb
        {
            get => Material.GetColor(ShadingColorFactorProp);
            set => Material.SetColor(ShadingColorFactorProp, value);
        }
        public Texture ShadingTexture
        {
            get => Material.GetTexture(ShadingMapProp);
            set => Material.SetTexture(ShadingMapProp, value);
        }
        public float ShadingToonyFactor
        {
            get => Material.GetFloat(ShadingToonyFactorProp);
            set => Material.SetFloat(ShadingToonyFactorProp, value);
        }
        public float ShadingShiftFactor
        {
            get => Material.GetFloat(ShadingShiftFactorProp);
            set => Material.SetFloat(ShadingShiftFactorProp, value);
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
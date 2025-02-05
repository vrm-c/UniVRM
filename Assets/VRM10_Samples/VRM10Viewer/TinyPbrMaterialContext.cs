using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class TinyPbrMaterialContext
    {
        // When using shadergraph, you need to expose the following properties.
        /// <summary>
        /// Color = White
        /// </summary>
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        /// <summary>
        /// Texture2D = White
        /// When using shadergraph, require "Set as Main Texture"
        /// </summary>
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        /// <summary>
        /// float = 1
        /// </summary>
        private static readonly int OcclusionStrengthProp = Shader.PropertyToID("_OcclusionStrength");
        /// <summary>
        /// Texture2D = White
        /// </summary>
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");

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
            }
        }
    }
}
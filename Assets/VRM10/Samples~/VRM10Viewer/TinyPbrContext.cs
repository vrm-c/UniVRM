using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class TinyPbrContext
    {
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        public readonly Material Material;

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

        public TinyPbrContext(Material material)
        {
            Material = material;
        }
    }
}
using UnityEngine;


namespace UniGLTF
{
    public abstract class MaterialItemBase
    {
        protected int m_index;
        protected glTFMaterial m_src;

        public string Name { get; set; }

        public MaterialItemBase(int i, glTFMaterial src)
        {
            m_index = i;
            m_src = src;
            Name = src != null ? m_src.name : "";
        }

        public abstract Material GetOrCreate(GetTextureItemFunc getTexture);

        protected Material CreateMaterial(string shaderName)
        {
            var material = new Material(Shader.Find(shaderName));
#if UNITY_EDITOR
            // textureImporter.SaveAndReimport(); may destroy this material
            material.hideFlags = HideFlags.DontUnloadUnusedAsset;
#endif
            material.name = (m_src == null || string.IsNullOrEmpty(m_src.name))
                ? string.Format("material_{0:00}", m_index)
                : m_src.name
                ;

            return material;
        }

        protected static void SetTextureOffsetAndScale(Material material, glTFTextureInfo textureInfo, string propertyName)
        {
            if (glTF_KHR_texture_transform.TryGet(textureInfo, out glTF_KHR_texture_transform textureTransform))
            {
                Vector2 offset = new Vector2(0, 0);
                Vector2 scale = new Vector2(1, 1);
                if (textureTransform.offset != null && textureTransform.offset.Length == 2)
                {
                    offset = new Vector2(textureTransform.offset[0], textureTransform.offset[1]);
                }
                if (textureTransform.scale != null && textureTransform.scale.Length == 2)
                {
                    scale = new Vector2(textureTransform.scale[0], textureTransform.scale[1]);
                }

                offset.y = (offset.y + scale.y - 1.0f) * -1.0f;

                material.SetTextureOffset(propertyName, offset);
                material.SetTextureScale(propertyName, scale);
            }
        }
    }
}

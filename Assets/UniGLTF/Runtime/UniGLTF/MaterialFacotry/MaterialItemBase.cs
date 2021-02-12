using System.Threading.Tasks;
using UnityEngine;


namespace UniGLTF
{
    public static class MaterialItemBase
    {
        public static Material CreateMaterial(int index, glTFMaterial src, string shaderName)
        {
            var material = new Material(Shader.Find(shaderName));
#if UNITY_EDITOR
            // textureImporter.SaveAndReimport(); may destroy this material
            material.hideFlags = HideFlags.DontUnloadUnusedAsset;
#endif
            material.name = (src == null || string.IsNullOrEmpty(src.name))
                ? string.Format("material_{0:00}", index)
                : src.name
                ;

            return material;
        }

        public static void SetTextureOffsetAndScale(Material material, glTFTextureInfo textureInfo, string propertyName)
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

        public static Task<Material> DefaultCreateMaterialAsync(glTF gltf, int i, GetTextureAsyncFunc getTexture)
        {

            if (i < 0 || i >= gltf.materials.Count)
            {
                UnityEngine.Debug.LogWarning("glTFMaterial is empty");
                return PBRMaterialItem.CreateAsync(i, null, getTexture);
            }
            var x = gltf.materials[i];

            if (glTF_KHR_materials_unlit.IsEnable(x))
            {
                var hasVertexColor = gltf.MaterialHasVertexColor(i);
                return UnlitMaterialItem.CreateAsync(i, x, getTexture, hasVertexColor);
            }

            return PBRMaterialItem.CreateAsync(i, x, getTexture);
        }

        /// <summary>
        /// for unittest
        /// </summary>
        /// <param name="i"></param>
        /// <param name="material"></param>
        /// <param name="getTexture"></param>
        /// <returns></returns>
        public static Material CreateMaterialForTest(int i, glTFMaterial material)
        {
            throw new System.NotImplementedException();
        }
    }
}

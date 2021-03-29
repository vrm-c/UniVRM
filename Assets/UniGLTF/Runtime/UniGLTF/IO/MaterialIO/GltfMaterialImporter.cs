using System.Collections.Generic;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    public delegate bool TryCreateMaterialParamFromGltf(GltfParser parser, int i, out MaterialImportParam param);

    public class GltfMaterialImporter
    {
        /// <summary>
        /// gltfMaterialを解釈する関数。
        /// 拡張するには、先頭に挿入するべし。
        /// </summary>
        /// <typeparam name="TryCreateMaterialParamFromGltf"></typeparam>
        /// <returns></returns>
        public readonly List<TryCreateMaterialParamFromGltf> GltfMaterialParamProcessors = new List<TryCreateMaterialParamFromGltf>();

        public GltfMaterialImporter()
        {
            // unlit を試し
            GltfMaterialParamProcessors.Add(GltfUnlitMaterial.TryCreateParam);
            // PBR を作成する(失敗しない)
            GltfMaterialParamProcessors.Add(GltfPBRMaterial.TryCreateParam);
        }

        public static string MaterialName(int index, glTFMaterial src)
        {
            if (src != null && !string.IsNullOrEmpty(src.name))
            {
                return src.name;
            }
            return $"material_{index:00}";
        }

        public MaterialImportParam CreateParam(GltfParser parser, int i)
        {
            foreach (var tryCreate in GltfMaterialParamProcessors)
            {
                if (tryCreate(parser, i, out MaterialImportParam param))
                {
                    return param;
                }
            }

            // fallback
#if VRM_DEVELOP
            Debug.LogWarning($"material: {i} out of range. fallback");
#endif
            return new MaterialImportParam(MaterialName(i, null), GltfPBRMaterial.ShaderName);
        }

        public static (Vector2, Vector2) GetTextureOffsetAndScale(glTFTextureInfo textureInfo)
        {
            Vector2 offset = new Vector2(0, 0);
            Vector2 scale = new Vector2(1, 1);
            if (glTF_KHR_texture_transform.TryGet(textureInfo, out glTF_KHR_texture_transform textureTransform))
            {
                if (textureTransform.offset != null && textureTransform.offset.Length == 2)
                {
                    offset = new Vector2(textureTransform.offset[0], textureTransform.offset[1]);
                }
                if (textureTransform.scale != null && textureTransform.scale.Length == 2)
                {
                    scale = new Vector2(textureTransform.scale[0], textureTransform.scale[1]);
                }

                offset.y = (offset.y + scale.y - 1.0f) * -1.0f;
            }
            return (offset, scale);
        }


        /// <summary>
        /// for unittest
        /// </summary>
        public static glTF CreateMaterialForTest(glTFMaterial material)
        {
            return new glTF
            {
                materials = new System.Collections.Generic.List<glTFMaterial> {
                    material
                },
                textures = new List<glTFTexture>{
                    new glTFTexture{
                        name = "texture_0"
                    }
                },
                images = new List<glTFImage>{
                    new glTFImage{
                        name = "image_0",
                        mimeType = "image/png",
                    }
                },
            };
        }
    }
}

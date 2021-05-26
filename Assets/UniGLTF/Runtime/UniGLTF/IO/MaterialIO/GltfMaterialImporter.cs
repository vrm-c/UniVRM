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

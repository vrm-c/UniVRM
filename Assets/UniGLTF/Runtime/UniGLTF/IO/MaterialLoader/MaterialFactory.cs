using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF.AltTask;

namespace UniGLTF
{
    public class MaterialFactory : IDisposable
    {
        glTF m_gltf;
        IStorage m_storage;
        Dictionary<string, Material> m_externalMap;
        public bool TryGetExternal(int index, out Material external)
        {
            var gltfMaterial = m_gltf.materials[index];
            if (m_externalMap.TryGetValue(gltfMaterial.name, out external))
            {
                return true;
            }

            external = default;
            return false;
        }

        public MaterialFactory(glTF gltf, IStorage storage,
        IEnumerable<KeyValuePair<string, UnityEngine.Object>> externalMap)
        {
            m_gltf = gltf;
            m_storage = storage;
            if (externalMap != null)
            {
                m_externalMap = externalMap
                    .Select(kv => (kv.Key, kv.Value as Material))
                    .Where(kv => kv.Item2 != null)
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2)
                    ;
            }
        }

        public delegate Awaitable<Material> CreateMaterialAsyncFunc(glTF gltf, int i, GetTextureAsyncFunc getTexture);
        CreateMaterialAsyncFunc m_createMaterialAsync;
        public CreateMaterialAsyncFunc CreateMaterialAsync
        {
            set
            {
                m_createMaterialAsync = value;
            }
            get
            {
                if (m_createMaterialAsync == null)
                {
                    m_createMaterialAsync = MaterialFactory.DefaultCreateMaterialAsync;
                }
                return m_createMaterialAsync;
            }
        }

        public struct MaterialLoadInfo
        {
            public readonly Material Asset;
            public readonly bool UseExternal;

            public MaterialLoadInfo(Material asset, bool useExternal)
            {
                Asset = asset;
                UseExternal = useExternal;
            }
        }

        List<MaterialLoadInfo> m_materials = new List<MaterialLoadInfo>();
        public IReadOnlyList<MaterialLoadInfo> Materials => m_materials;
        public void Dispose()
        {
            foreach (var x in ObjectsForSubAsset())
            {
                UnityEngine.Object.DestroyImmediate(x, true);
            }
        }

        public IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        {
            foreach (var x in m_materials)
            {
                yield return x.Asset;
            }
        }

        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index].Asset;
        }

        public async Awaitable LoadMaterialsAsync(GetTextureAsyncFunc getTexture)
        {
            if (m_gltf.materials == null || m_gltf.materials.Count == 0)
            {
                // no material. work around.
                var material = await CreateMaterialAsync(m_gltf, 0, getTexture);
                m_materials.Add(new MaterialLoadInfo(material, false));
                return;
            }

            // 先に m_gltf.textures を作成
            for (int i = 0; i < m_gltf.textures.Count; ++i)
            {
                await getTexture(GetTextureParam.Create(i));
            }

            // 後に material を作成。
            // 必用に応じてテクスチャを変換。
            for (int i = 0; i < m_gltf.materials.Count; ++i)
            {
                if (TryGetExternal(i, out Material material))
                {
                    m_materials.Add(new MaterialLoadInfo(material, true));
                }
                else
                {
                    material = await CreateMaterialAsync(m_gltf, i, getTexture);
                    m_materials.Add(new MaterialLoadInfo(material, false));
                }
            }
        }

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

        public static Awaitable<Material> DefaultCreateMaterialAsync(glTF gltf, int i, GetTextureAsyncFunc getTexture)
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
            var gltf = new glTF
            {
                materials = new System.Collections.Generic.List<glTFMaterial> { material },
            };
            var task = DefaultCreateMaterialAsync(gltf, i, null);
            return task.Result;
        }
    }
}

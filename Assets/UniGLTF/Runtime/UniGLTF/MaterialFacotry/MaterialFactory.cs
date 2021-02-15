using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public class MaterialFactory : IDisposable
    {
        glTF m_gltf;
        IStorage m_storage;
        public MaterialFactory(glTF gltf, IStorage storage)
        {
            m_gltf = gltf;
            m_storage = storage;
        }

        public delegate Task<Material> CreateMaterialAsyncFunc(glTF glTF, int i, GetTextureAsyncFunc getTexture);
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
                    m_createMaterialAsync = MaterialItemBase.DefaultCreateMaterialAsync;
                }
                return m_createMaterialAsync;
            }
        }

        List<Material> m_materials = new List<Material>();
        public IReadOnlyList<Material> Materials => m_materials;
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
                yield return x;
            }
        }

        public void AddMaterial(Material material)
        {
            var originalName = material.name;
            int j = 2;
            while (m_materials.Any(x => x.name == material.name))
            {
                material.name = string.Format("{0}({1})", originalName, j++);
            }
            m_materials.Add(material);
        }
        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index];
        }

        public IEnumerator LoadMaterials(GetTextureAsyncFunc getTexture)
        {
            if (m_gltf.materials == null || m_gltf.materials.Count == 0)
            {
                var task = CreateMaterialAsync(m_gltf, 0, getTexture);

                foreach (var x in task.AsIEnumerator())
                {
                    yield return x;
                }

                AddMaterial(task.Result);
            }
            else
            {
                for (int i = 0; i < m_gltf.materials.Count; ++i)
                {
                    var task = CreateMaterialAsync(m_gltf, i, getTexture);
                    foreach (var x in task.AsIEnumerator())
                    {
                        yield return null;
                    }

                    AddMaterial(task.Result);
                }
            }
            yield return null;
        }
    }
}

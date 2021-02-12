using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public delegate TextureItem GetTextureItemFunc(int i);
    public delegate Shader GetShaderFunc();
    public delegate MaterialItemBase MaterialImporter(int i, glTFMaterial x, bool hasVertexColor);


    public class MaterialFactory : IDisposable
    {
        MaterialImporter m_materialImporter;
        public MaterialImporter MaterialImporter
        {
            set
            {
                m_materialImporter = value;
            }
            get
            {
                if (m_materialImporter == null)
                {
                    m_materialImporter = CreateMaterial;
                }
                return m_materialImporter;
            }
        }

        List<TextureItem> m_textures = new List<TextureItem>();
        public IList<TextureItem> GetTextures()
        {
            return m_textures;
        }
        public TextureItem GetTexture(int i)
        {
            if (i < 0 || i >= m_textures.Count)
            {
                return null;
            }
            return m_textures[i];
        }
        public void AddTexture(TextureItem item)
        {
            m_textures.Add(item);
        }

        List<MaterialItemBase> m_materials = new List<MaterialItemBase>();
        public void AddMaterial(MaterialItemBase material)
        {
            var originalName = material.Name;
            int j = 2;
            while (m_materials.Any(x => x.Name == material.Name))
            {
                material.Name = string.Format("{0}({1})", originalName, j++);
            }
            m_materials.Add(material);
        }
        public IList<MaterialItemBase> GetMaterials()
        {
            return m_materials;
        }
        public MaterialItemBase GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index];
        }

        public virtual ITextureLoader CreateTextureLoader(int index)
        {
#if UNIGLTF_USE_WEBREQUEST_TEXTURELOADER
            return new UnityWebRequestTextureLoader(index);
#else
            return new GltfTextureLoader(index);
#endif
        }

        public void Prepare(UniGLTF.glTF gltf, UnityPath imageBaseDir = default(UnityPath))
        {
            if (m_textures.Count == 0)
            {
                //
                // runtime
                //
                CreateTextureItems(gltf, imageBaseDir);
            }
            else
            {
                //
                // already CreateTextures(by assetPostProcessor or editor menu)
                //
            }

        }

        public void CreateTextureItems(UniGLTF.glTF gltf, UnityPath imageBaseDir)
        {
            if (m_textures.Any())
            {
                return;
            }

            for (int i = 0; i < gltf.textures.Count; ++i)
            {

                TextureItem item = null;
#if UNITY_EDITOR
                var image = gltf.GetImageFromTextureIndex(i);
                if (imageBaseDir.IsUnderAssetsFolder
                    && !string.IsNullOrEmpty(image.uri)
                    && !image.uri.FastStartsWith("data:")
                    )
                {
                    ///
                    /// required SaveTexturesAsPng or SetTextureBaseDir
                    ///
                    var assetPath = imageBaseDir.Child(image.uri);
                    var textureName = !string.IsNullOrEmpty(image.name) ? image.name : Path.GetFileNameWithoutExtension(image.uri);
                    item = new TextureItem(i, new AssetTextureLoader(assetPath, textureName))
                    {
                        IsAsset = true
                    };
                }
                else
#endif
                {
                    item = new TextureItem(i, CreateTextureLoader(i));
                }

                AddTexture(item);
            }
        }

        public IEnumerator TexturesProcessOnMainThread(glTF gltf, IStorage storage)
        {
            // using (MeasureTime("TexturesProcessOnMainThread"))
            {
                foreach (var x in GetTextures())
                {
                    yield return x.ProcessOnMainThreadCoroutine(gltf, storage);
                }
            }
        }

        public IEnumerator LoadMaterials(glTF gltf)
        {
            // using (MeasureTime("LoadMaterials"))
            {
                if (gltf.materials == null || !gltf.materials.Any())
                {
                    AddMaterial(MaterialImporter(0, null, false));
                }
                else
                {
                    for (int i = 0; i < gltf.materials.Count; ++i)
                    {
                        AddMaterial(MaterialImporter(i, gltf.materials[i], gltf.MaterialHasVertexColor(i)));
                    }
                }
            }
            yield return null;
        }

        public static MaterialItemBase CreateMaterial(int i, glTFMaterial x, bool hasVertexColor)
        {
            if (x == null)
            {
                UnityEngine.Debug.LogWarning("glTFMaterial is empty");
                return new PBRMaterialItem(i, x);
            }

            if (glTF_KHR_materials_unlit.IsEnable(x))
            {
                return new UnlitMaterialItem(i, x, hasVertexColor);
            }

            return new PBRMaterialItem(i, x);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

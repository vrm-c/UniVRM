using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public class MaterialFactory
    {
        public IShaderStore m_shaderStore;
        public IShaderStore ShaderStore
        {
            set
            {
                m_shaderStore = value;
            }
            get
            {
                if (m_shaderStore == null)
                {
                    m_shaderStore = new ShaderStore(this);
                }
                return m_shaderStore;
            }
        }

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
                    m_materialImporter = new MaterialImporter(ShaderStore);
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

        List<Material> m_materials = new List<Material>();
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
        public IList<Material> GetMaterials()
        {
            return m_materials;
        }
        public Material GetMaterial(int index)
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
            return new TextureLoader(index);
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
                    item = new TextureItem(i, assetPath, textureName);
                }
                else
#endif
                {
                    item = new TextureItem(i, CreateTextureLoader(i));
                }

                AddTexture(item);
            }
        }

        public IEnumerator TexturesProcessOnAnyThread(glTF gltf, IStorage storage)
        {
            // using (MeasureTime("TexturesProcessOnAnyThread"))
            {
                foreach (var x in GetTextures())
                {
                    x.ProcessOnAnyThread(gltf, storage);
                    yield return null;
                }
            }
        }

        public IEnumerator TexturesProcessOnMainThread(glTF gltf)
        {
            // using (MeasureTime("TexturesProcessOnMainThread"))
            {
                foreach (var x in GetTextures())
                {
                    yield return x.ProcessOnMainThreadCoroutine(gltf);
                }
            }
        }

        public IEnumerator LoadMaterials(glTF gltf)
        {
            // using (MeasureTime("LoadMaterials"))
            {
                if (gltf.materials == null || !gltf.materials.Any())
                {
                    AddMaterial(MaterialImporter.CreateMaterial(0, null, false, GetTexture));
                }
                else
                {
                    for (int i = 0; i < gltf.materials.Count; ++i)
                    {
                        AddMaterial(MaterialImporter.CreateMaterial(i, gltf.materials[i], gltf.MaterialHasVertexColor(i), GetTexture));
                    }
                }
            }
            yield return null;
        }
    }
}

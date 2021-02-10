using System.Collections;
using UnityEngine;

namespace UniGLTF
{
#if UNITY_EDITOR
    public class AssetTextureLoader : ITextureLoader
    {
        public Texture2D Texture
        {
            private set;
            get;
        }

        UnityPath m_assetPath;

        public AssetTextureLoader(UnityPath assetPath, string _)
        {
            m_assetPath = assetPath;
        }

        public void Dispose()
        {
        }

        public void ProcessOnAnyThread(glTF gltf, IStorage storage)
        {
        }

        public IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler)
        {
            //
            // texture from assets
            //
            m_assetPath.ImportAsset();
            var importer = m_assetPath.GetImporter<UnityEditor.TextureImporter>();
            if (importer == null)
            {
                Debug.LogWarningFormat("fail to get TextureImporter: {0}", m_assetPath);
            }
            importer.maxTextureSize = 8192;
            importer.sRGBTexture = !isLinear;

            importer.SaveAndReimport();

            Texture = m_assetPath.LoadAsset<Texture2D>();

            //Texture.name = m_textureName;
            if (Texture == null)
            {
                Debug.LogWarningFormat("fail to Load Texture2D: {0}", m_assetPath);
            }

            else
            {
                var maxSize = Mathf.Max(Texture.width, Texture.height);

                importer.maxTextureSize
                    = maxSize > 4096 ? 8192 :
                    maxSize > 2048 ? 4096 :
                    maxSize > 1024 ? 2048 :
                    maxSize > 512 ? 1024 :
                    512;

                importer.SaveAndReimport();
            }

            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(Texture, sampler);
            }

            yield break;
        }
    }
#endif
}

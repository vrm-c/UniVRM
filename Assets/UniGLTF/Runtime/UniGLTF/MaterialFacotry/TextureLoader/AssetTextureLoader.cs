using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
#if UNITY_EDITOR
    public static class AssetTextureLoader
    {
        public static Task<Texture2D> LoadTaskAsync(UnityPath m_assetPath,
            bool isLinear, glTFTextureSampler sampler)
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

            var Texture = m_assetPath.LoadAsset<Texture2D>();

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

            return Task.FromResult(Texture);
        }
    }
#endif
}

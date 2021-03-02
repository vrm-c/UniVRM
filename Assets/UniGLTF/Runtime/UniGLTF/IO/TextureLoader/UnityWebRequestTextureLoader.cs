using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UniGLTF
{
    public class UnityWebRequestTextureLoader
    {
        public Texture2D Texture
        {
            private set;
            get;
        }

        int m_textureIndex;

        public UnityWebRequestTextureLoader(int textureIndex)
        {
            m_textureIndex = textureIndex;
        }

        UnityWebRequest m_uwr;
        public void Dispose()
        {
            if (m_uwr != null)
            {
                m_uwr.Dispose();
                m_uwr = null;
            }
        }

        string m_textureName = default;
        public void ProcessOnAnyThread()
        {
        }

        class Deleter : IDisposable
        {
            string m_path;
            public Deleter(string path)
            {
                m_path = path;
            }
            public void Dispose()
            {
                if (File.Exists(m_path))
                {
                    File.Delete(m_path);
                }
            }
        }

        public IEnumerator ProcessOnMainThread(glTF gltf, IStorage storage, bool isLinear, glTFTextureSampler sampler)
        {
            var gltfTexture = gltf.textures[m_textureIndex];
            var bytes = gltf.GetImageBytes(storage, gltfTexture.source);

            // tmp file
            var tmp = Path.GetTempFileName();
            using (var f = new FileStream(tmp, FileMode.Create))
            {
                f.Write(bytes.Array, bytes.Offset, bytes.Count);
            }

            using (var d = new Deleter(tmp))
            {
                var url = "file:///" + tmp.Replace("\\", "/");
                Debug.LogFormat("UnityWebRequest: {0}", url);
#if UNITY_2017_1_OR_NEWER
                using (var m_uwr = UnityWebRequestTexture.GetTexture(url, true))
                {
                    yield return m_uwr.SendWebRequest();

                    if (m_uwr.isNetworkError || m_uwr.isHttpError)
                    {
                        Debug.LogWarning(m_uwr.error);
                    }
                    else
                    {
                        // Get downloaded asset bundle
                        Texture = ((DownloadHandlerTexture)m_uwr.downloadHandler).texture;
                        Texture.name = m_textureName;
                    }
                }
#elif UNITY_5
                using (var m_uwr = new WWW(url))
                {
                    yield return m_uwr;

                    // wait for request
                    while (!m_uwr.isDone)
                    {
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(m_uwr.error))
                    {
                        Debug.Log(m_uwr.error);
                        yield break;
                    }

                    // Get downloaded asset bundle
                    Texture = m_uwr.textureNonReadable;
                    Texture.name = m_textureName;
                }
#else
#error Unsupported Unity version
#endif
            }
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(Texture, sampler);
            }
        }
    }
}

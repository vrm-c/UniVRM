using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public interface ITextureLoader : IDisposable
    {
        Texture2D Texture { get; }

        /// <summary>
        /// Call from any thread
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="storage"></param>
        void ProcessOnAnyThread(glTF gltf, IStorage storage);

        /// <summary>
        /// Call from unity main thread
        /// </summary>
        /// <param name="isLinear"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler);
    }

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
            var importer = m_assetPath.GetImporter<TextureImporter>();
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

    public class TextureLoader : ITextureLoader
    {
        int m_textureIndex;
        public TextureLoader(int textureIndex)
        {
            m_textureIndex = textureIndex;
        }

        public Texture2D Texture
        {
            private set;
            get;
        }

        public void Dispose()
        {
        }

        static Byte[] ToArray(ArraySegment<byte> bytes)
        {
            if (bytes.Array == null)
            {
                return new byte[] { };
            }
            else if (bytes.Offset == 0 && bytes.Count == bytes.Array.Length)
            {
                return bytes.Array;
            }
            else
            {
                Byte[] result = new byte[bytes.Count];
                Buffer.BlockCopy(bytes.Array, bytes.Offset, result, 0, result.Length);
                return result;
            }
        }

        Byte[] m_imageBytes;
        string m_textureName;
        public void ProcessOnAnyThread(glTF gltf, IStorage storage)
        {
            var imageIndex = gltf.GetImageIndexFromTextureIndex(m_textureIndex);
            var segments = gltf.GetImageBytes(storage, imageIndex, out m_textureName);
            m_imageBytes = ToArray(segments);
        }

        public IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler)
        {
            //
            // texture from image(png etc) bytes
            //
            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, isLinear);
            Texture.name = m_textureName;
            if (m_imageBytes != null)
            {
                Texture.LoadImage(m_imageBytes);
            }
            if (sampler != null)
            {
                TextureSamplerUtil.SetSampler(Texture, sampler);
            }
            yield break;
        }
    }

    public class UnityWebRequestTextureLoader : ITextureLoader
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

        ArraySegment<Byte> m_segments;
        string m_textureName;
        public void ProcessOnAnyThread(glTF gltf, IStorage storage)
        {
            var imageIndex = gltf.GetImageIndexFromTextureIndex(m_textureIndex);
            m_segments = gltf.GetImageBytes(storage, imageIndex, out m_textureName);
        }

#if false
        HttpHost m_http;
        class HttpHost : IDisposable
        {
            TcpListener m_listener;
            Socket m_connection;

            public HttpHost(int port)
            {
                m_listener = new TcpListener(IPAddress.Loopback, port);
                m_listener.Start();
                m_listener.BeginAcceptSocket(OnAccepted, m_listener);
            }

            void OnAccepted(IAsyncResult ar)
            {
                var l = ar.AsyncState as TcpListener;
                if (l == null) return;
                m_connection = l.EndAcceptSocket(ar);
                // 次の接続受付はしない

                BeginRead(m_connection, new byte[8192]);
            }

            void BeginRead(Socket c, byte[] buffer)
            {
                AsyncCallback callback = ar =>
                {
                    var s = ar.AsyncState as Socket;
                    if (s == null) return;
                    var size = s.EndReceive(ar);
                    if (size > 0)
                    {
                        OnRead(buffer, size);
                    }
                    BeginRead(s, buffer);
                };
                m_connection.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, m_connection);
            }

            List<Byte> m_buffer = new List<byte>();
            void OnRead(byte[] buffer, int len)
            {
                m_buffer.AddRange(buffer.Take(len));
            }

            public string Url
            {
                get
                {

                }
            }

            public void Dispose()
            {
                if (m_connection != null)
                {
                    m_connection.Dispose();
                    m_connection = null;
                }
                if(m_listener != null)
                {
                    m_listener.Stop();
                    m_listener = null;
                }
            }
        }
#endif

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

        public IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler)
        {
            // tmp file
            var tmp = Path.GetTempFileName();
            using (var f = new FileStream(tmp, FileMode.Create))
            {
                f.Write(m_segments.Array, m_segments.Offset, m_segments.Count);
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

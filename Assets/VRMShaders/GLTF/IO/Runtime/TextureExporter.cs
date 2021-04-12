using System;
using System.Collections.Generic;
using UnityEngine;


namespace VRMShaders
{
    /// <summary>
    /// glTF にエクスポートする Texture2D を蓄えて index を確定させる。
    /// Exporter の最後でまとめて Texture2D から bytes 列を得て出力する。
    /// </summary>
    public class TextureExporter : IDisposable
    {
        Func<Texture, bool> m_useAsset;

        public TextureExporter(Func<Texture, bool> useAsset)
        {
            m_useAsset = useAsset;
        }

        public void Dispose()
        {
            // TODO: export 用にコピー・変換したテクスチャーをここで解放したい
        }

        public enum ConvertTypes
        {
            // 無変換
            None,
            // Unity Standard様式 から glTF PBR様式への変換
            OcclusionMetallicRoughness,
            // Assetを使うときはそのバイト列を無変換で、それ以外は DXT5nm 形式からのデコードを行う
            Normal,
        }

        struct ExportKey
        {
            public readonly Texture Src;
            public readonly ConvertTypes TextureType;

            public ExportKey(Texture src, ConvertTypes type)
            {
                if (src == null)
                {
                    throw new ArgumentNullException();
                }
                Src = src;
                TextureType = type;
            }
        }
        Dictionary<ExportKey, int> m_exportMap = new Dictionary<ExportKey, int>();

        /// <summary>
        /// Export する Texture2D のリスト。これが gltf.textures になる
        /// </summary>
        /// <typeparam name="Texture2D"></typeparam>
        /// <returns></returns>
        public readonly List<Texture2D> Exported = new List<Texture2D>();

        /// <summary>
        /// Texture の export index を得る
        /// </summary>
        /// <param name="src"></param>
        /// <param name="textureType"></param>
        /// <returns></returns>
        public int GetTextureIndex(Texture src, ConvertTypes textureType)
        {
            if (src == null)
            {
                return -1;
            }
            return m_exportMap[new ExportKey(src, textureType)];
        }

        /// <summary>
        /// sRGBなテクスチャーを処理し、index を確定させる
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int ExportSRGB(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, ConvertTypes.None), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (m_useAsset(texture2D))
            {
                // do nothing                
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, TextureImportTypes.sRGB, null);
            }
            Exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, ConvertTypes.None), index);

            return index;
        }

        /// <summary>
        /// Linearなテクスチャーを処理し、index を確定させる
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int ExportLinear(Texture src)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Standard の Metallic, Smoothness, Occlusion をまとめ、index を確定させる
        /// </summary>
        /// <param name="metallicSmoothTexture"></param>
        /// <param name="smoothness"></param>
        /// <param name="occlusionTexture"></param>
        /// <returns></returns>
        public int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture)
        {
            if (metallicSmoothTexture == null && occlusionTexture == null)
            {
                return -1;
            }

            // cache
            if (metallicSmoothTexture != null && m_exportMap.TryGetValue(new ExportKey(metallicSmoothTexture, ConvertTypes.OcclusionMetallicRoughness), out var index))
            {
                return index;
            }
            if (occlusionTexture != null && m_exportMap.TryGetValue(new ExportKey(occlusionTexture, ConvertTypes.OcclusionMetallicRoughness), out index))
            {
                return index;
            }

            //
            // Unity と glTF で互換性が無いので必ず変換が必用
            //
            index = Exported.Count;
            var texture2D = OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture);

            Exported.Add(texture2D);
            if (metallicSmoothTexture != null)
            {
                m_exportMap.Add(new ExportKey(metallicSmoothTexture, ConvertTypes.OcclusionMetallicRoughness), index);
            }
            if (occlusionTexture != null && occlusionTexture != metallicSmoothTexture)
            {
                m_exportMap.Add(new ExportKey(occlusionTexture, ConvertTypes.OcclusionMetallicRoughness), index);
            }

            return index;
        }

        /// <summary>
        /// Normal のテクスチャを変換し index を確定させる
        /// </summary>
        /// <param name="normalTexture"></param>
        /// <returns></returns>
        public int ExportNormal(Texture src)
        {
            if (src == null)
            {
                return -1;
            }

            // cache
            if (m_exportMap.TryGetValue(new ExportKey(src, ConvertTypes.Normal), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (m_useAsset(texture2D))
            {
                // EditorAsset を使うので変換不要
            }
            else
            {
                // 後で Bitmap を使うために変換する
                texture2D = NormalConverter.Export(src);
            }

            Exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, ConvertTypes.Normal), index);

            return index;
        }

        /// <summary>
        /// 画像のバイト列を得る
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static (byte[] bytes, string mime) GetTextureBytesWithMime(Texture2D texture)
        {
            try
            {
                var png = texture.EncodeToPNG();
                if (png != null)
                {
                    return (png, "image/png");
                }
            }
            catch (Exception ex)
            {
                // fail to EncodeToPng
                // System.ArgumentException: not readable, the texture memory can not be accessed from scripts. You can make the texture readable in the Texture Import Settings.
                Debug.LogWarning(ex);
            }

            {
                // try copy and EncodeToPng
                var copy = TextureConverter.CopyTexture(texture, TextureImportTypes.sRGB, null);
                var png = copy.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(copy);
                return (png, "image/png");
            }
        }
    }
}

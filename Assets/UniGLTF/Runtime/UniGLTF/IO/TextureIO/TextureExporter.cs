using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniGLTF
{
    /// <summary>
    /// glTF にエクスポートする Texture2D を蓄えて index を確定させる
    /// </summary>
    public class TextureExporter
    {
        struct ExportKey
        {
            public readonly Texture Src;
            public readonly glTFTextureTypes TextureType;

            public ExportKey(Texture src, glTFTextureTypes type)
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
        public int GetTextureIndex(Texture src, glTFTextureTypes textureType)
        {
            return m_exportMap[new ExportKey(src, textureType)];
        }

        /// <summary>
        /// TextureImporter.maxTextureSize が元のテクスチャーより小さいか否かの判定
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        static bool CopyIfMaxTextureSizeIsSmaller(Texture src)
        {
#if UNITY_EDITOR            
            var textureImporter = AssetImporter.GetAtPath(UnityPath.FromAsset(src).Value) as TextureImporter;
            var getSizeMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            if (textureImporter != null && getSizeMethod != null)
            {
                var args = new object[2] { 0, 0 };
                getSizeMethod.Invoke(textureImporter, args);
                var originalWidth = (int)args[0];
                var originalHeight = (int)args[1];
                var originalSize = Mathf.Max(originalWidth, originalHeight);
                if (textureImporter.maxTextureSize < originalSize)
                {
                    return true;
                }
            }
#endif
            return false;
        }

        /// <summary>
        /// 元の Asset が存在して、 TextureImporter に設定された画像サイズが小さくない
        /// </summary>
        /// <param name="src"></param>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        static bool UseAsset(Texture2D texture2D)
        {
#if UNITY_EDITOR
            if (texture2D != null && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(texture2D)))
            {
                if (CopyIfMaxTextureSizeIsSmaller(texture2D))
                {
                    return false;
                }
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// sRGBなテクスチャーを処理する
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
            if (m_exportMap.TryGetValue(new ExportKey(src, glTFTextureTypes.SRGB), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (UseAsset(texture2D))
            {
                // do nothing                
            }
            else
            {
                texture2D = TextureConverter.CopyTexture(src, glTFTextureTypes.SRGB, null);
            }
            Exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, glTFTextureTypes.SRGB), index);

            return index;
        }

        /// <summary>
        /// Standard の Metallic, Smoothness, Occlusion をまとめる
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
            if (m_exportMap.TryGetValue(new ExportKey(metallicSmoothTexture, glTFTextureTypes.OcclusionMetallicRoughness), out var index))
            {
                return index;
            }
            if (m_exportMap.TryGetValue(new ExportKey(occlusionTexture, glTFTextureTypes.OcclusionMetallicRoughness), out index))
            {
                return index;
            }

            //
            // Unity と glTF で互換性が無いので必ず変換が必用
            //
            index = Exported.Count;
            var texture2D = OcclusionMetallicRoughnessConverter.Export(metallicSmoothTexture, smoothness, occlusionTexture);

            Exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(metallicSmoothTexture, glTFTextureTypes.OcclusionMetallicRoughness), index);
            if (occlusionTexture != metallicSmoothTexture && occlusionTexture != null)
            {
                m_exportMap.Add(new ExportKey(occlusionTexture, glTFTextureTypes.OcclusionMetallicRoughness), index);
            }

            return index;
        }

        /// <summary>
        /// Normal のテクスチャを変換する
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
            if (m_exportMap.TryGetValue(new ExportKey(src, glTFTextureTypes.Normal), out var index))
            {
                return index;
            }

            // get Texture2D
            index = Exported.Count;
            var texture2D = src as Texture2D;
            if (UseAsset(texture2D))
            {
                // EditorAsset を使うので変換不要
            }
            else
            {
                // 後で Bitmap を使うために変換する
                texture2D = NormalConverter.Export(src);
            }

            Exported.Add(texture2D);
            m_exportMap.Add(new ExportKey(src, glTFTextureTypes.Normal), index);

            return index;
        }

        /// <summary>
        /// 画像のバイト列を得る
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        static (Byte[] bytes, string mine) GetBytesWithMime(Texture2D texture)
        {
#if UNITY_EDITOR
            var path = UnityPath.FromAsset(texture);
            if (path.IsUnderAssetsFolder)
            {
                if (path.Extension == ".png")
                {
                    return
                    (
                        System.IO.File.ReadAllBytes(path.FullPath),
                        "image/png"
                    );
                }
                if (path.Extension == ".jpg")
                {
                    return
                    (
                        System.IO.File.ReadAllBytes(path.FullPath),
                        "image/jpeg"
                    );
                }
            }
#endif

            return
            (
                texture.EncodeToPNG(),
                "image/png"
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="bufferIndex"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        static public int ExportTexture(glTF gltf, int bufferIndex, Texture2D texture)
        {
            var bytesWithMime = GetBytesWithMime(texture);

            // add view
            var view = gltf.buffers[bufferIndex].Append(bytesWithMime.bytes, glBufferTarget.NONE);
            var viewIndex = gltf.AddBufferView(view);

            // add image
            var imageIndex = gltf.images.Count;
            gltf.images.Add(new glTFImage
            {
                name = GetTextureParam.RemoveSuffix(texture.name),
                bufferView = viewIndex,
                mimeType = bytesWithMime.mine,
            });

            // add sampler
            var samplerIndex = gltf.samplers.Count;
            var sampler = TextureSamplerUtil.Export(texture);
            gltf.samplers.Add(sampler);

            // add texture
            var textureIndex = gltf.textures.Count;
            gltf.textures.Add(new glTFTexture
            {
                sampler = samplerIndex,
                source = imageIndex,
            });

            return textureIndex;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UniGLTF;
using System.Linq;
using VRMShaders;

namespace UniGLTF
{
    public class TextureExtractor
    {
        const string TextureDirName = "Textures";

        GltfParser m_parser;
        public GltfParser Parser => m_parser;

        public glTF GLTF => m_parser.GLTF;
        public IStorage Storage => m_parser.Storage;

        public readonly Dictionary<SubAssetKey, UnityPath> Textures = new Dictionary<SubAssetKey, UnityPath>();
        (SubAssetKey Key, UnityEngine.Texture2D Value)[] m_subAssets;
        UnityPath m_textureDirectory;

        public TextureExtractor(GltfParser parser, UnityPath textureDirectory, (SubAssetKey, UnityEngine.Texture2D)[] subAssets)
        {
            m_parser = parser;
            m_textureDirectory = textureDirectory;
            m_textureDirectory.EnsureFolder();
            m_subAssets = subAssets;
        }

        public static string GetExt(string mime, string uri)
        {
            switch (mime)
            {
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
            }

            return Path.GetExtension(uri).ToLower();
        }

        public void Extract(SubAssetKey key, TextureImportParam param)
        {
            if (Textures.ContainsKey(key))
            {
                return;
            }

            if (!string.IsNullOrEmpty(param.Uri) && !param.ExtractConverted)
            {
                // use GLTF external
                // targetPath = m_textureDirectory.Child(key.Name);
            }
            else
            {
                UnityPath targetPath = default;

                switch (param.TextureType)
                {
                    case TextureImportTypes.StandardMap:
                        {
                            // write converted texture
                            var (_, subAsset) = m_subAssets.FirstOrDefault(kv => kv.Key == key);
                            if (subAsset == null)
                            {
                                throw new KeyNotFoundException();
                            }
                            targetPath = m_textureDirectory.Child($"{key.Name}.png");
                            File.WriteAllBytes(targetPath.FullPath, subAsset.EncodeToPNG().ToArray());
                            targetPath.ImportAsset();
                            break;
                        }

                    default:
                        {
                            // write original bytes
                            targetPath = m_textureDirectory.Child($"{key.Name}{param.Ext}");
                            File.WriteAllBytes(targetPath.FullPath, param.Index0().Result.ToArray());
                            targetPath.ImportAsset();
                            break;
                        }
                }
                Textures.Add(key, targetPath);
            }
        }

        /// <summary>
        /// 
        /// * Texture(.png etc...)をディスクに書き出す
        /// * EditorApplication.delayCall で処理を進めて 書き出した画像が Asset として成立するのを待つ
        /// * 書き出した Asset から TextureImporter を取得して設定する
        /// 
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="dirName"></param>
        /// <param name="onCompleted"></param>
        public static void ExtractTextures(GltfParser parser, UnityPath textureDirectory,
            EnumerateAllTexturesDistinctFunc textureEnumerator, (SubAssetKey, Texture2D)[] subAssets,
            Action<SubAssetKey, Texture2D> addRemap,
            Action<IEnumerable<UnityPath>> onCompleted = null)
        {
            var extractor = new TextureExtractor(parser, textureDirectory, subAssets);
            foreach (var (key, x) in textureEnumerator(parser))
            {
                extractor.Extract(key, x);
            }

            EditorApplication.delayCall += () =>
            {
                // Wait for the texture assets to be imported

                foreach (var (key, targetPath) in extractor.Textures)
                {
                    // remap
                    var externalObject = targetPath.LoadAsset<Texture2D>();
                    if (externalObject != null)
                    {
                        addRemap(key, externalObject);
                    }
                }

                if (onCompleted != null)
                {
                    onCompleted(extractor.Textures.Values);
                }
            };
        }
    }

    public static class KeyValuePariExtensions
    {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> pair, out T key, out U value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}

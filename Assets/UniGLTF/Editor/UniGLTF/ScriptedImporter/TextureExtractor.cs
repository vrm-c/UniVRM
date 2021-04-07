using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
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

        public readonly Dictionary<UnityPath, TextureImportParam> Textures = new Dictionary<UnityPath, TextureImportParam>();
        UnityEngine.Texture2D[] m_subAssets;
        UnityPath m_textureDirectory;

        public TextureExtractor(GltfParser parser, UnityPath textureDirectory, UnityEngine.Texture2D[] subAssets)
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

        public void Extract(TextureImportParam param)
        {
            if (Textures.Values.Contains(param))
            {
                return;
            }

            UnityPath targetPath = default;
            if (!string.IsNullOrEmpty(param.Uri) && !param.ExtractConverted)
            {
                targetPath = m_textureDirectory.Child(param.GltfFileName);
            }
            else
            {

                switch (param.TextureType)
                {
                    case TextureImportTypes.StandardMap:
                        {
                            // write converted texture
                            var subAsset = m_subAssets.FirstOrDefault(x => x.name == param.ConvertedName);
                            targetPath = m_textureDirectory.Child(param.ConvertedFileName);
                            File.WriteAllBytes(targetPath.FullPath, subAsset.EncodeToPNG().ToArray());
                            targetPath.ImportAsset();
                            break;
                        }

                    default:
                        {
                            // write original bytes
                            targetPath = m_textureDirectory.Child(param.GltfFileName);
                            File.WriteAllBytes(targetPath.FullPath, param.Index0().Result.ToArray());
                            targetPath.ImportAsset();
                            break;
                        }
                }
            }

            Textures.Add(targetPath, param);
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
            EnumerateAllTexturesDistinctFunc textureEnumerator, Texture2D[] subAssets, Action<Texture2D> addRemap,
            Action<IEnumerable<UnityPath>> onCompleted = null)
        {
            var extractor = new TextureExtractor(parser, textureDirectory, subAssets);
            foreach (var x in textureEnumerator(parser))
            {
                extractor.Extract(x);
            }

            EditorApplication.delayCall += () =>
            {
                // Wait for the texture assets to be imported

                foreach (var kv in extractor.Textures)
                {
                    var targetPath = kv.Key;
                    var param = kv.Value;

                    // remap
                    var externalObject = targetPath.LoadAsset<Texture2D>();
                    if (externalObject != null)
                    {
                        addRemap(externalObject);
                    }
                }

                if (onCompleted != null)
                {
                    onCompleted(extractor.Textures.Keys);
                }
            };
        }
    }
}

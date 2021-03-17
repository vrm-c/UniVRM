using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UniGLTF
{
    public class TextureExtractor
    {
        const string TextureDirName = "Textures";

        GltfParser m_parser;
        public GltfParser Parser => m_parser;

        public glTF GLTF => m_parser.GLTF;
        public IStorage Storage => m_parser.Storage;

        public readonly Dictionary<string, GetTextureParam> Textures = new Dictionary<string, GetTextureParam>();
        UnityEngine.Texture2D[] m_subAssets;
        string m_path;

        public TextureExtractor(string assetPath, UnityEngine.Texture2D[] subAssets)
        {
            // parse GLTF
            m_parser = new GltfParser();
            m_parser.ParsePath(assetPath);

            m_path = $"{Path.GetDirectoryName(assetPath)}/{Path.GetFileNameWithoutExtension(assetPath)}.Textures";
            SafeCreateDirectory(m_path);

            if (assetPath == null)
            {
                throw new ArgumentNullException();
            }
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

        public void Extract(GetTextureParam param, bool hasUri)
        {
            if (Textures.Values.Contains(param))
            {
                return;
            }

            var subAsset = m_subAssets.FirstOrDefault(x => x.name == param.ConvertedName);
            string targetPath = "";

            if (hasUri && !param.ExtractConverted)
            {
                var gltfTexture = GLTF.textures[param.Index0.Value];
                var gltfImage = GLTF.images[gltfTexture.source];
                var ext = GetExt(gltfImage.mimeType, gltfImage.uri);
                targetPath = $"{Path.GetDirectoryName(m_path)}/{param.GltflName}{ext}";
            }
            else
            {

                switch (param.TextureType)
                {
                    case GetTextureParam.METALLIC_GLOSS_PROP:
                    case GetTextureParam.OCCLUSION_PROP:
                        {
                            // write converted texture
                            targetPath = $"{m_path}/{param.ConvertedName}.png";
                            File.WriteAllBytes(targetPath, subAsset.EncodeToPNG().ToArray());
                            AssetDatabase.ImportAsset(targetPath);
                            break;
                        }

                    default:
                        {
                            // write original bytes
                            var gltfTexture = GLTF.textures[param.Index0.Value];
                            var gltfImage = GLTF.images[gltfTexture.source];
                            var ext = GetExt(gltfImage.mimeType, gltfImage.uri);
                            targetPath = $"{m_path}/{param.GltflName}{ext}";
                            File.WriteAllBytes(targetPath, GLTF.GetImageBytes(Storage, gltfTexture.source).ToArray());
                            AssetDatabase.ImportAsset(targetPath);
                            break;
                        }
                }
            }

            Textures.Add(targetPath, param);
        }

        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
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
        public static void ExtractTextures(string assetPath, TextureEnumerator textureEnumerator, Texture2D[] subAssets, Action<Texture2D> addRemap, Action<IEnumerable<string>> onCompleted = null)
        {
            var extractor = new TextureExtractor(assetPath, subAssets);
            var normalMaps = new List<string>();

            foreach (var x in textureEnumerator(extractor.GLTF))
            {
                var gltfTexture = extractor.GLTF.textures[x.Index0.Value];
                var gltfImage = extractor.GLTF.images[gltfTexture.source];
                extractor.Extract(x, !string.IsNullOrEmpty(gltfImage.uri));
            }

            EditorApplication.delayCall += () =>
            {
                foreach (var kv in extractor.Textures)
                {
                    var targetPath = kv.Key;
                    var param = kv.Value;

                    // TextureImporter                   
                    var targetTextureImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                    if (targetTextureImporter != null)
                    {
                        switch (param.TextureType)
                        {
                            case GetTextureParam.OCCLUSION_PROP:
                            case GetTextureParam.METALLIC_GLOSS_PROP:
#if VRM_DEVELOP
                                Debug.Log($"{targetPath} => linear");
#endif
                                targetTextureImporter.sRGBTexture = false;
                                targetTextureImporter.SaveAndReimport();
                                break;

                            case GetTextureParam.NORMAL_PROP:
#if VRM_DEVELOP
                                Debug.Log($"{targetPath} => normalmap");
#endif
                                targetTextureImporter.textureType = TextureImporterType.NormalMap;
                                targetTextureImporter.SaveAndReimport();
                                break;
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException(targetPath);
                    }

                    // remap
                    var externalObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(targetPath);
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

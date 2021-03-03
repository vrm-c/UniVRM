using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UniGLTF
{
    class TextureExtractor
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

            m_subAssets = subAssets;
        }

        static string GetExt(string mime)
        {
            switch (mime)
            {
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
            }
            throw new NotImplementedException();
        }

        public void Extract(GetTextureParam param)
        {
            var subAsset = m_subAssets.FirstOrDefault(x => x.name == param.Name);
            string targetPath = "";

            switch (param.TextureType)
            {
                case GetTextureParam.METALLIC_GLOSS_PROP:
                case GetTextureParam.OCCLUSION_PROP:
                    {
                        // write converted texture
                        targetPath = $"{m_path}/{param.Name}.png";
                        File.WriteAllBytes(targetPath, subAsset.EncodeToPNG().ToArray());
                        break;
                    }

                default:
                    {
                        // write original bytes
                        var gltfTexture = GLTF.textures[param.Index0.Value];
                        var gltfImage = GLTF.images[gltfTexture.source];
                        var ext = GetExt(gltfImage.mimeType);
                        targetPath = $"{m_path}/{param.Name}{ext}";
                        File.WriteAllBytes(targetPath, GLTF.GetImageBytes(Storage, gltfTexture.source).ToArray());
                        break;
                    }
            }
            AssetDatabase.ImportAsset(targetPath);
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
        public static void ExtractTextures(string assetPath, Texture2D[] subAssets, Action<Texture2D> addRemap, Action onCompleted = null)
        {
            var extractor = new TextureExtractor(assetPath, subAssets);
            foreach (var material in extractor.GLTF.materials)
            {
                foreach (var x in extractor.Parser.EnumerateTextures(material))
                {
                    extractor.Extract(x);
                }
            }

            EditorApplication.delayCall += () =>
            {
                foreach (var kv in extractor.Textures)
                {
                    var targetPath = kv.Key;
                    var param = kv.Value;

                    // TextureImporter                   
                    var targetTextureImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;

                    switch (param.TextureType)
                    {
                        case GetTextureParam.OCCLUSION_PROP:
                        case GetTextureParam.METALLIC_GLOSS_PROP:
                            targetTextureImporter.sRGBTexture = false;
                            break;

                        case GetTextureParam.NORMAL_PROP:
                            targetTextureImporter.textureType = TextureImporterType.NormalMap;
                            break;
                    }

                    targetTextureImporter.SaveAndReimport();

                    // remap
                    var externalObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(targetPath);
                    addRemap(externalObject);
                }

                if (onCompleted != null)
                {
                    onCompleted();
                }
            };
        }
    }
}

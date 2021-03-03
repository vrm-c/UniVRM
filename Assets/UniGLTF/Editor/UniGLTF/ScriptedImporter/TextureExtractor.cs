using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Linq;

namespace UniGLTF
{
    class TextureExtractor
    {
        GltfParser m_parser;
        public GltfParser Parser => m_parser;

        public glTF GLTF => m_parser.GLTF;
        public IStorage Storage => m_parser.Storage;

        public readonly Dictionary<string, GetTextureParam> Textures = new Dictionary<string, GetTextureParam>();
        UnityEngine.Texture2D[] m_subAssets;
        string m_path;

        public TextureExtractor(ScriptedImporter importer)
        {
            // parse GLTF
            m_parser = new GltfParser();
            m_parser.ParsePath(importer.assetPath);

            m_path = $"{Path.GetDirectoryName(importer.assetPath)}/{Path.GetFileNameWithoutExtension(importer.assetPath)}.Textures";
            m_subAssets = importer.GetSubAssets<UnityEngine.Texture2D>(importer.assetPath).ToArray();
        }

        static Regex s_mimeTypeReg = new Regex("image/(?<mime>.*)$");

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
                        targetPath = string.Format("{0}/{1}{2}",
                           m_path,
                           param.Name,
                           ".png"
                           );
                        File.WriteAllBytes(targetPath, subAsset.EncodeToPNG().ToArray());
                        break;
                    }

                default:
                    {
                        // write original bytes
                        targetPath = string.Format("{0}/{1}{2}",
                           m_path,
                           param.Name,
                           ".png"
                           );
                        var gltfTexture = GLTF.textures[param.Index0.Value];
                        File.WriteAllBytes(targetPath, GLTF.GetImageBytes(Storage, gltfTexture.source).ToArray());
                        break;
                    }
            }
            AssetDatabase.ImportAsset(targetPath);
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
        public static void ExtractTextures(ScriptedImporter importer, string dirName, Action onCompleted = null)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }

            var path = string.Format("{0}/{1}.{2}",
                Path.GetDirectoryName(importer.assetPath),
                Path.GetFileNameWithoutExtension(importer.assetPath),
                dirName
                );
            importer.SafeCreateDirectory(path);

            // Reload Model
            var extractor = new TextureExtractor(importer);

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
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(UnityEngine.Texture2D), externalObject.name), externalObject);
                }

                AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);

                if (onCompleted != null)
                {
                    onCompleted();
                }
            };
        }
    }
}

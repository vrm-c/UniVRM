using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace UniGLTF
{
    public static class ScriptedImporterExtension
    {
        public static void ClearExternalObjects<T>(this ScriptedImporter importer) where T : UnityEngine.Object
        {
            foreach (var extarnalObject in importer.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)))
            {
                importer.RemoveRemap(extarnalObject.Key);
            }

            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }

        public static void ClearExtarnalObjects(this ScriptedImporter importer)
        {
            foreach (var extarnalObject in importer.GetExternalObjectMap())
            {
                importer.RemoveRemap(extarnalObject.Key);
            }

            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static T GetSubAsset<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        {
            return importer.GetSubAssets<T>(assetPath)
                .FirstOrDefault();
        }

        public static IEnumerable<T> GetSubAssets<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase
                .LoadAllAssetsAtPath(assetPath)
                .Where(x => AssetDatabase.IsSubAsset(x))
                .Where(x => x is T)
                .Select(x => x as T);
        }

        private static void ExtractFromAsset(UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        {
            string assetPath = AssetDatabase.GetAssetPath(subAsset);

            var clone = UnityEngine.Object.Instantiate(subAsset);
            AssetDatabase.CreateAsset(clone, destinationPath);

            var assetImporter = AssetImporter.GetAtPath(assetPath);
            assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(subAsset), clone);

            if (isForceUpdate)
            {
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }

        public static void ExtractAssets<T>(this ScriptedImporter importer, string dirName, string extension) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(importer.assetPath))
                return;

            var subAssets = importer.GetSubAssets<T>(importer.assetPath);

            var path = string.Format("{0}/{1}.{2}",
                Path.GetDirectoryName(importer.assetPath),
                Path.GetFileNameWithoutExtension(importer.assetPath),
                dirName
                );

            var info = importer.SafeCreateDirectory(path);

            foreach (var asset in subAssets)
            {
                ExtractFromAsset(asset, string.Format("{0}/{1}{2}", path, asset.name, extension), false);
            }
        }

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
        }

        public static void ExtractTextures(this ScriptedImporter importer, string dirName, Action onCompleted = null)
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

        public static DirectoryInfo SafeCreateDirectory(this ScriptedImporter importer, string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }
    }
}

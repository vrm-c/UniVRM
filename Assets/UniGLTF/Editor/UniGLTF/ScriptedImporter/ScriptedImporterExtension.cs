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

        struct TextureInfo
        {
            public string Path;
            public bool sRGB;
            public bool IsNormalMap;
        }

        class TextureExtractor
        {
            GltfParser m_parser;
            public GltfParser Parser => m_parser;

            public glTF GLTF => m_parser.GLTF;

            public readonly List<TextureInfo> Textures = new List<TextureInfo>();
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
                var targetPath = string.Format("{0}/{1}{2}",
                    m_path,
                    param.Name,
                    ".png"
                    );
                File.WriteAllBytes(targetPath, subAsset.EncodeToPNG().ToArray());
                AssetDatabase.ImportAsset(targetPath);

                Textures.Add(new TextureInfo
                {
                    Path = targetPath,
                    sRGB = true,
                    IsNormalMap = param.TextureType == GetTextureParam.NORMAL_PROP,
                });
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
                foreach (var extracted in extractor.Textures)
                {
                    // TextureImporter
                    var targetTextureImporter = AssetImporter.GetAtPath(extracted.Path) as TextureImporter;
                    targetTextureImporter.sRGBTexture = extracted.sRGB;
                    if (extracted.IsNormalMap)
                    {
                        targetTextureImporter.textureType = TextureImporterType.NormalMap;
                    }
                    targetTextureImporter.SaveAndReimport();

                    // remap
                    var externalObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(extracted.Path);
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

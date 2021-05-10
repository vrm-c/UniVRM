using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRMShaders;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    public class TmpGuiEnable : IDisposable
    {
        bool m_backup;
        public TmpGuiEnable(bool enable)
        {
            m_backup = GUI.enabled;
            GUI.enabled = enable;
        }

        public void Dispose()
        {
            GUI.enabled = m_backup;
        }
    }
    public static class EditorMaterial
    {
        static bool s_foldMaterials = true;
        static bool s_foldTextures = true;

        public static void OnGUI(ScriptedImporter importer, GltfParser parser, EnumerateAllTexturesDistinctFunc enumTextures, Func<string, string> textureDir, Func<string, string> materialDir)
        {
            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is Material || x.Value is Texture2D);
            using (new TmpGuiEnable(!hasExternal))
            {
                if (GUILayout.Button("Extract Materials And Textures ..."))
                {
                    ExtractMaterialsAndTextures(importer, parser, enumTextures, textureDir, materialDir);
                }
            }

            //
            // Draw ExternalObjectMap
            //
            s_foldMaterials = EditorGUILayout.Foldout(s_foldMaterials, "Remapped Materials");
            if (s_foldMaterials)
            {
                importer.DrawRemapGUI<UnityEngine.Material>(parser.GLTF.materials.Select(x => new SubAssetKey(typeof(Material), x.name)));
            }

            s_foldTextures = EditorGUILayout.Foldout(s_foldTextures, "Remapped Textures");
            if (s_foldTextures)
            {
                importer.DrawRemapGUI<UnityEngine.Texture2D>(enumTextures(parser)
                    .Where(x =>
                    {
                        var (key, param) = x;
                        if ((param.TextureType == TextureImportTypes.sRGB || param.TextureType == TextureImportTypes.NormalMap) && !string.IsNullOrEmpty(param.Uri))
                        {
                            // GLTF の 無変換テクスチャーをスキップする
                            return false;
                        }
                        return true;
                    })
                    .Select(x => x.Key)
                    );
            }

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<UnityEngine.Material>();
                importer.ClearExternalObjects<UnityEngine.Texture2D>();
            }
        }

        public static void SetExternalUnityObject<T>(this ScriptedImporter self, UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            self.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(self.assetPath);
            AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
        }

        static void ExtractMaterialsAndTextures(ScriptedImporter self, GltfParser parser, EnumerateAllTexturesDistinctFunc enumTextures, Func<string, string> textureDir, Func<string, string> materialDir)
        {
            if (string.IsNullOrEmpty(self.assetPath))
            {
                return;
            }

            Action<SubAssetKey, Texture2D> addRemap = (key, externalObject) =>
                {
                    self.AddRemap(new AssetImporter.SourceAssetIdentifier(key.Type, key.Name), externalObject);
                };
            Action<IEnumerable<UnityPath>> onCompleted = _ =>
                {
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
                    self.ExtractMaterials(materialDir);
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
                };

            var assetPath = UnityPath.FromFullpath(parser.TargetPath);
            var dirName = textureDir(assetPath.Value); // $"{assetPath.FileNameWithoutExtension}.Textures";
            TextureExtractor.ExtractTextures(parser, assetPath.Parent.Child(dirName),
                enumTextures,
                self.GetSubAssets<UnityEngine.Texture2D>(self.assetPath).ToArray(),
                addRemap,
                onCompleted
                );
        }

        public static void ExtractMaterials(this ScriptedImporter importer, Func<string, string> materialDir)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }
            var path = $"{Path.GetDirectoryName(importer.assetPath)}/{materialDir(importer.assetPath)}"; //  Path.GetFileNameWithoutExtension(importer.assetPath)}.Materials
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var (key, asset) in importer.GetSubAssets<Material>(importer.assetPath))
            {
                asset.ExtractSubAsset($"{path}/{key.Name}.mat", false);
            }
        }
    }
}

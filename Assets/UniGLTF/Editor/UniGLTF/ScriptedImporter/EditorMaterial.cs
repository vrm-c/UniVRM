using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UniGLTF
{
    public static class EditorMaterial
    {
        class TmpGuiEnable : IDisposable
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

        static bool s_foldMaterials;
        static bool s_foldTextures;

        public static void OnGUIMaterial(ScriptedImporter importer, GltfParser parser)
        {
            var canExtract = !importer.GetExternalObjectMap().Any(x => x.Value is Material || x.Value is Texture2D);
            using (new TmpGuiEnable(canExtract))
            {
                if (GUILayout.Button("Extract Materials And Textures ..."))
                {
                    ExtractMaterialsAndTextures(importer);
                }
            }

            //
            // Draw ExternalObjectMap
            //
            s_foldMaterials = EditorGUILayout.Foldout(s_foldMaterials, "Remapped Materials");
            if (s_foldMaterials)
            {
                DrawRemapGUI<UnityEngine.Material>(importer, parser.GLTF.materials.Select(x => x.name));
            }

            s_foldTextures = EditorGUILayout.Foldout(s_foldTextures, "Remapped Textures");
            if (s_foldTextures)
            {
                DrawRemapGUI<UnityEngine.Texture2D>(importer, GltfTextureEnumerator.Enumerate(parser.GLTF).Select(x => x.ConvertedName));
            }

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<UnityEngine.Material>();
                importer.ClearExternalObjects<UnityEngine.Texture2D>();
            }
        }

        static void DrawRemapGUI<T>(ScriptedImporter importer, IEnumerable<string> names) where T : UnityEngine.Object
        {
            EditorGUI.indentLevel++;
            var map = importer.GetExternalObjectMap()
                .Select(x => (x.Key.name, x.Value as T))
                .Where(x => x.Item2 != null)
                .ToDictionary(x => x.Item1, x => x.Item2)
                ;
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new System.ArgumentNullException();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(name);
                map.TryGetValue(name, out T value);
                var asset = EditorGUILayout.ObjectField(value, typeof(T), true) as T;
                if (asset != value)
                {
                    importer.SetExternalUnityObject(new AssetImporter.SourceAssetIdentifier(value), asset);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        public static void SetExternalUnityObject<T>(this ScriptedImporter self, UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            self.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(self.assetPath);
            AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
        }

        static void ExtractMaterialsAndTextures(ScriptedImporter self)
        {
            if (string.IsNullOrEmpty(self.assetPath))
            {
                return;
            }

            Action<Texture2D> addRemap = externalObject =>
                {
                    self.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(UnityEngine.Texture2D), externalObject.name), externalObject);
                };
            Action<IEnumerable<string>> onCompleted = _ =>
                {
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
                    self.ExtractMaterials();
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
                };

            TextureExtractor.ExtractTextures(self.assetPath,
                GltfTextureEnumerator.Enumerate,
                self.GetSubAssets<UnityEngine.Texture2D>(self.assetPath).ToArray(),
                addRemap,
                onCompleted
                );
        }

        public static void ExtractMaterials(this ScriptedImporter importer)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }
            var path = $"{Path.GetDirectoryName(importer.assetPath)}/{Path.GetFileNameWithoutExtension(importer.assetPath)}.Materials";
            var info = TextureExtractor.SafeCreateDirectory(path);

            foreach (var asset in importer.GetSubAssets<Material>(importer.assetPath))
            {
                ExtractSubAsset(asset, $"{path}/{asset.name}.mat", false);
            }
        }

        private static void ExtractSubAsset(UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        {
            string assetPath = AssetDatabase.GetAssetPath(subAsset);

            var clone = UnityEngine.Object.Instantiate(subAsset);
            AssetDatabase.CreateAsset(clone, destinationPath);

            var assetImporter = AssetImporter.GetAtPath(assetPath);
            assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(clone), clone);

            if (isForceUpdate)
            {
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}

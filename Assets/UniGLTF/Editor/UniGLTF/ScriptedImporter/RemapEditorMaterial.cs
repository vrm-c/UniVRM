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
    /// <summary>
    /// Material, Texture の Remap, Extract
    /// </summary>
    public class RemapEditorMaterial : RemapEditorBase
    {
        static bool s_foldMaterials = true;
        static bool s_foldTextures = true;

        public RemapEditorMaterial(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter) : base(keys, getter, setter)
        { }

        public void OnGUI(ScriptedImporter importer, GltfData data,
            ITextureDescriptorGenerator textureDescriptorGenerator,
            Func<string, string> textureDir,
            Func<string, string> materialDir)
        {
            if (CanExtract(importer))
            {
                if (GUILayout.Button("Extract Materials And Textures ..."))
                {
                    ExtractMaterialsAndTextures(importer, data, textureDescriptorGenerator, textureDir, materialDir);
                }
                EditorGUILayout.HelpBox("Extract subasset to external object and overwrite remap", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Clear extraction"))
                {
                    ClearExternalObjects(importer, typeof(Texture), typeof(Material));
                }
                EditorGUILayout.HelpBox("Clear remap. All remap use subAsset", MessageType.Info);
            }

            //
            // Draw ExternalObjectMap
            //
            s_foldMaterials = EditorGUILayout.Foldout(s_foldMaterials, "Remapped Materials");
            if (s_foldMaterials)
            {
                DrawRemapGUI<UnityEngine.Material>(importer.GetExternalObjectMap());
            }

            s_foldTextures = EditorGUILayout.Foldout(s_foldTextures, "Remapped Textures");
            if (s_foldTextures)
            {
                DrawRemapGUI<UnityEngine.Texture>(importer.GetExternalObjectMap());
            }
        }

        void ExtractMaterialsAndTextures(ScriptedImporter self, GltfData data, ITextureDescriptorGenerator textureDescriptorGenerator, Func<string, string> textureDir, Func<string, string> materialDir)
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
                    // texture extract 後に importer 発動
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);

                    ExtractMaterials(self, materialDir);
                };

            // subAsset を ExternalObject として投入する
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(self.assetPath)
                    .Select(x => x as Texture)
                    .Where(x => x != null)
                    .Select(x => (new SubAssetKey(x), x))
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2)
                    ;

            var assetPath = UnityPath.FromUnityPath(self.assetPath);
            var dirName = textureDir(assetPath.Value); // $"{assetPath.FileNameWithoutExtension}.Textures";
            TextureExtractor.ExtractTextures(
                data,
                assetPath.Parent.Child(dirName),
                textureDescriptorGenerator,
                subAssets,
                addRemap,
                onCompleted
            );
        }

        public static void ExtractMaterials(ScriptedImporter importer, Func<string, string> materialDir)
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

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(importer.assetPath))
            {
                if (asset is Material)
                {
                    ExtractSubAsset(asset, $"{path}/{asset.name}.mat", false);
                }
            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}

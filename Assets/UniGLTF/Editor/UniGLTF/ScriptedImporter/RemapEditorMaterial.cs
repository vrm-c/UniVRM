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

        public void OnGUI(ScriptedImporter importer, GltfParser parser,
            ITextureDescriptorGenerator textureDescriptorGenerator,
            Func<string, string> textureDir,
            Func<string, string> materialDir)
        {
            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is Material || x.Value is Texture2D);
            using (new EditorGUI.DisabledScope(hasExternal))
            {
                if (GUILayout.Button("Extract Materials And Textures ..."))
                {
                    ExtractMaterialsAndTextures(importer, parser, textureDescriptorGenerator, textureDir, materialDir);
                }
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

        void ExtractMaterialsAndTextures(ScriptedImporter self, GltfParser parser, ITextureDescriptorGenerator textureDescriptorGenerator, Func<string, string> textureDir, Func<string, string> materialDir)
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
                    // material extract 後に importer 発動
                    AssetDatabase.ImportAsset(self.assetPath, ImportAssetOptions.ForceUpdate);
                };

            var assetPath = UnityPath.FromFullpath(parser.TargetPath);
            var dirName = textureDir(assetPath.Value); // $"{assetPath.FileNameWithoutExtension}.Textures";
            TextureExtractor.ExtractTextures(
                parser,
                assetPath.Parent.Child(dirName),
                textureDescriptorGenerator,
                self.GetSubAssets<Texture>(self.assetPath).ToDictionary(kv => kv.Item1, kv => kv.Item2),
                addRemap,
                onCompleted
            );
        }

        public void ExtractMaterials(ScriptedImporter importer, Func<string, string> materialDir)
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

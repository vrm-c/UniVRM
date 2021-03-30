using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class VrmScriptedImporterImpl
    {
        /// <summary>
        /// VRM1 で　パースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="migrateToVrm1"></param>
        /// <returns></returns>
        public static GltfParser Parse(string path, bool migrateToVrm1)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(path);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                return parser;
            }

            if (migrateToVrm1)
            {
                // try migrateion
                var migrated = MigrationVrm.Migrate(File.ReadAllBytes(path));
                parser = new GltfParser();
                parser.Parse(path, migrated);
                return parser;
            }

            return null;
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP            
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            var parser = Parse(scriptedImporter.assetPath, migrateToVrm1);
            if (parser == null)
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var externalObjectMap = scriptedImporter.GetExternalObjectMap().Select(kv => (kv.Value.name, kv.Value)).ToArray();

            using (var loader = new RuntimeUnityBuilder(parser, externalObjectMap))
            {
                // settings TextureImporters
                foreach (var textureInfo in Vrm10MToonMaterialImporter.EnumerateAllTexturesDistinct(parser))
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalMap);
                }

                loader.Load();
                loader.ShowMeshes();

                loader.TransferOwnership(o =>
                {
                    context.AddObjectToAsset(o.name, o);
                    if (o is GameObject)
                    {
                        // Root GameObject is main object
                        context.SetMainObject(loader.Root);
                    }

                    return true;
                });
            }
        }

        // public void ExtractMeta()
        // {
        //     this.ExtractAssets<UniVRM10.VRM10MetaObject>(MetaDirName, ".asset");
        //     var metaObject = this.GetExternalUnityObjects<UniVRM10.VRM10MetaObject>().FirstOrDefault();
        //     var metaObjectPath = AssetDatabase.GetAssetPath(metaObject.Value);
        //     if (!string.IsNullOrEmpty(metaObjectPath))
        //     {
        //         EditorUtility.SetDirty(metaObject.Value);
        //         AssetDatabase.WriteImportSettingsIfDirty(metaObjectPath);
        //     }
        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public void ExtractExpressions()
        // {
        //     this.ExtractAssets<UniVRM10.VRM10ExpressionAvatar>(ExpressionDirName, ".asset");
        //     this.ExtractAssets<UniVRM10.VRM10Expression>(ExpressionDirName, ".asset");

        //     var expressionAvatar = this.GetExternalUnityObjects<UniVRM10.VRM10ExpressionAvatar>().FirstOrDefault();
        //     var expressions = this.GetExternalUnityObjects<UniVRM10.VRM10Expression>();

        //     expressionAvatar.Value.Clips = expressions.Select(x => x.Value).ToList();
        //     var avatarPath = AssetDatabase.GetAssetPath(expressionAvatar.Value);
        //     if (!string.IsNullOrEmpty(avatarPath))
        //     {
        //         EditorUtility.SetDirty(expressionAvatar.Value);
        //         AssetDatabase.WriteImportSettingsIfDirty(avatarPath);
        //     }

        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public Dictionary<string, T> GetExternalUnityObjects<T>() where T : UnityEngine.Object
        // {
        //     return this.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)).ToDictionary(x => x.Key.name, x => (T)x.Value);
        // }

        // public void SetExternalUnityObject<T>(UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        // {
        //     this.AddRemap(sourceAssetIdentifier, obj);
        //     AssetDatabase.WriteImportSettingsIfDirty(this.assetPath);
        //     AssetDatabase.ImportAsset(this.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public static void ClearExternalObjects<T>(this ScriptedImporter importer) where T : UnityEngine.Object
        // {
        //     foreach (var extarnalObject in importer.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)))
        //     {
        //         importer.RemoveRemap(extarnalObject.Key);
        //     }

        //     AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        //     AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public static void ClearExtarnalObjects(this ScriptedImporter importer)
        // {
        //     foreach (var extarnalObject in importer.GetExternalObjectMap())
        //     {
        //         importer.RemoveRemap(extarnalObject.Key);
        //     }

        //     AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        //     AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // private static T GetSubAsset<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        // {
        //     return importer.GetSubAssets<T>(assetPath)
        //         .FirstOrDefault();
        // }

        // public static IEnumerable<T> GetSubAssets<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        // {
        //     return AssetDatabase
        //         .LoadAllAssetsAtPath(assetPath)
        //         .Where(x => AssetDatabase.IsSubAsset(x))
        //         .Where(x => x is T)
        //         .Select(x => x as T);
        // }

        // private static void ExtractFromAsset(UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        // {
        //     string assetPath = AssetDatabase.GetAssetPath(subAsset);

        //     var clone = UnityEngine.Object.Instantiate(subAsset);
        //     AssetDatabase.CreateAsset(clone, destinationPath);

        //     var assetImporter = AssetImporter.GetAtPath(assetPath);
        //     assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(subAsset), clone);

        //     if (isForceUpdate)
        //     {
        //         AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        //         AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        //     }
        // }

        // public static void ExtractAssets<T>(this ScriptedImporter importer, string dirName, string extension) where T : UnityEngine.Object
        // {
        //     if (string.IsNullOrEmpty(importer.assetPath))
        //         return;

        //     var subAssets = importer.GetSubAssets<T>(importer.assetPath);

        //     var path = string.Format("{0}/{1}.{2}",
        //         Path.GetDirectoryName(importer.assetPath),
        //         Path.GetFileNameWithoutExtension(importer.assetPath),
        //         dirName
        //         );

        //     var info = importer.SafeCreateDirectory(path);

        //     foreach (var asset in subAssets)
        //     {
        //         ExtractFromAsset(asset, string.Format("{0}/{1}{2}", path, asset.name, extension), false);
        //     }
        // }
    }
}

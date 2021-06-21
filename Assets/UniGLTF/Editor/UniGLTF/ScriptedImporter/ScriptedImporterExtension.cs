using System;
using System.Collections.Generic;
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
    public static class ScriptedImporterExtension
    {
        // public static void ClearExternalObjects(this ScriptedImporter importer, params Type[] targetTypes)
        // {
        //     foreach (var targetType in targetTypes)
        //     {
        //         if (!typeof(UnityEngine.Object).IsAssignableFrom(targetType))
        //         {
        //             throw new NotImplementedException();
        //         }

        //         foreach (var (key, obj) in importer.GetExternalObjectMap())
        //         {
        //             if (targetType.IsAssignableFrom(key.type))
        //             {
        //                 importer.RemoveRemap(key);
        //             }
        //         }
        //     }

        //     AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        //     AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        public static IEnumerable<(SubAssetKey, T)> GetSubAssets<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase
                .LoadAllAssetsAtPath(assetPath)
                .Where(x => AssetDatabase.IsSubAsset(x))
                .Where(x => x is T)
                .Select(x => (new SubAssetKey(typeof(T), x.name), x as T));
        }

        /// <summary>
        /// subAsset を 指定された path に extract する
        /// </summary>
        /// <param name="subAsset"></param>
        /// <param name="destinationPath"></param>
        /// <param name="isForceUpdate"></param>
        public static UnityEngine.Object ExtractSubAsset(this UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        {
            string assetPath = AssetDatabase.GetAssetPath(subAsset);

            // clone を path に出力(subAsset を出力するため)
            var clone = UnityEngine.Object.Instantiate(subAsset);
            AssetDatabase.CreateAsset(clone, destinationPath);

            // subAsset を clone に対して remap する
            var assetImporter = AssetImporter.GetAtPath(assetPath);
            assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(clone), clone);

            if (isForceUpdate)
            {
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return clone;
        }
    }
}

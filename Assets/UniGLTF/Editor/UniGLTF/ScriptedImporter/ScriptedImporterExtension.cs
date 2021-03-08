using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;

namespace UniGLTF
{
    public static class ScriptedImporterExtension
    {
        public static void ClearExternalObjects<T>(this ScriptedImporter importer) where T : UnityEngine.Object
        {
            foreach (var externalObject in importer.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)))
            {
                importer.RemoveRemap(externalObject.Key);
            }

            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }

        public static IEnumerable<T> GetSubAssets<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase
                .LoadAllAssetsAtPath(assetPath)
                .Where(x => AssetDatabase.IsSubAsset(x))
                .Where(x => x is T)
                .Select(x => x as T);
        }
    }
}

using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using UniGLTF.MeshUtility;

namespace VRM
{
    public static class VrmPrefabUtility
    {
        const string BACKUP_DIR = "MeshIntegratorBackup";

        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            if (!IsPrefab(prefab)) return null;

            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        static bool IsPrefab(Object instance)
        {
            if (instance == null)
            {
                return false;
            }
            if (PrefabUtility.GetPrefabAssetType(instance) != PrefabAssetType.Regular)
            {
                return false;
            }
            return true;
        }

        public static void ApplyChangesToPrefab(GameObject instance)
        {
            var prefab = instance.GetPrefab();
            if (prefab == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
        }
    }
}

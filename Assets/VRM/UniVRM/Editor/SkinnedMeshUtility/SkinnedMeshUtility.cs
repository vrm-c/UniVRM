using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRM
{
    public static class SkinnedMeshUtility
    {
        public const string MENU_KEY = "GameObject/UnityEditorScripts/";
        public const int MENU_PRIORITY = 11;

        public static Object GetPrefab(GameObject instance)
        {
#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
        }

        public static bool IsPrefab(Object instance)
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
            var prefab = GetPrefab(instance);
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

        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            if (!IsPrefab(prefab)) return null;

            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
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
            return instance != null && PrefabUtility.GetPrefabType(instance) == PrefabType.Prefab;
        }

        public static void ApplyChangesToPrefab(GameObject instance)
        {
            var prefab = GetPrefab(instance);
            if (prefab == null) return;

            PrefabUtility.ReplacePrefab(instance, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
        
        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            if (!IsPrefab(prefab)) return null;

            return (GameObject) PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
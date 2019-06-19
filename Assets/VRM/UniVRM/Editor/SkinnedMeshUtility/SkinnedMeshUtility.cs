using UnityEditor;
using UnityEngine;

namespace VRM
{
    public static class SkinnedMeshUtility
    {
        public const string MENU_KEY = "GameObject/UnityEditorScripts/";
        public const int MENU_PRIORITY = 11;
        
        public static Object GetPrefab(GameObject go)
        {
#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
        }

        public static bool IsPrefab(GameObject go)
        {
            return go != null && PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
        }
    }
}
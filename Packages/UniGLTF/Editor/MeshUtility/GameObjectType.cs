using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public enum GameObjectType
    {
        Null,
        SceneInstance,
        PrefabInstance,
        AssetPrefab,
    }

    public static class GameObjectTypeUtility
    {
        public static Object GetPrefab(this GameObject instance)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(instance)))
            {
                return instance;
            }

#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
        }

        public static GameObjectType GetGameObjectType(this GameObject go)
        {
            if (go == null)
            {
                return GameObjectType.Null;
            }

            if (!go.scene.IsValid())
            {
                if (PrefabUtility.IsPartOfAnyPrefab(go))
                {
                    return GameObjectType.AssetPrefab;
                }
                else
                {
                    throw new System.NotSupportedException("Unknown prefab status. The target does not exist in a valid scene and is not a prefab.");
                }
            }

            if (PrefabUtility.IsPartOfAnyPrefab(go))
            {
                return GameObjectType.PrefabInstance;
            }

            return GameObjectType.SceneInstance;
        }
    }
}

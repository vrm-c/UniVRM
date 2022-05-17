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
                    throw new System.Exception("unknown");
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

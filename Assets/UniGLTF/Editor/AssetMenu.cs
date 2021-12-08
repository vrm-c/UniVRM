using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class AssetMenu
    {
        [MenuItem("Assets/SaveAsPng", true)]
        [MenuItem("Assets/SaveAsPngLinear", true)]
        static bool IsTextureAsset()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/SaveAsPng")]
        static void SaveAsPng()
        {
            MeshUtility.EditorChangeTextureType.SaveAsPng(true);
        }

        [MenuItem("Assets/SaveAsPngLinear")]
        static void SaveAsPngLinear()
        {
            MeshUtility.EditorChangeTextureType.SaveAsPng(false);
        }
    }
}
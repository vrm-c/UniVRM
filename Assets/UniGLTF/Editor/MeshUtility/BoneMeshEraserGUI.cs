using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [CustomEditor(typeof(MeshProcessDialog), true)]
    public class BoneMeshEraserGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var skinnedMesh = serializedObject.FindProperty("_cSkinnedMesh");
            EditorGUILayout.PropertyField(skinnedMesh, new GUIContent("Skinned Mesh"), true);
            var list = serializedObject.FindProperty("_eraseBones");
            EditorGUILayout.PropertyField(list, new GUIContent("Erase Bones"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

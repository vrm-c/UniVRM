using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [CustomEditor(typeof(MeshProcessDialog), true)]
    class MeshProcessDialogEditor : Editor
    {
        public Tabs Tabs;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            switch (Tabs)
            {
                case Tabs.MeshIntegrator:
                    {
                        var skinnedMesh = serializedObject.FindProperty("_separateByBlendShape");
                        EditorGUILayout.PropertyField(skinnedMesh, new GUIContent(MeshProcessingMessages.MESH_SEPARATOR_BY_BLENDSHAPE.Msg()));
                        break;
                    }

                case Tabs.BoneMeshEraser:
                    {
                        var skinnedMesh = serializedObject.FindProperty("_cSkinnedMesh");
                        EditorGUILayout.PropertyField(skinnedMesh, new GUIContent("Skinned Mesh"), true);
                        var list = serializedObject.FindProperty("_eraseBones");
                        EditorGUILayout.PropertyField(list, new GUIContent("Erase Bones"), true);
                        break;
                    }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

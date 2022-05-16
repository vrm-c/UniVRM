using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    [CustomEditor(typeof(MeshProcessDialog), true)]
    class MeshProcessDialogEditor : Editor
    {
        MeshProcessDialog _targetDialog;
        SerializedProperty _separateByBlendShape;
        SerializedProperty _skinnedMesh;
        SerializedProperty _eraseBones;

        void OnEnable()
        {
            _targetDialog = (MeshProcessDialog)target;
            _separateByBlendShape = serializedObject.FindProperty(nameof(MeshProcessDialog._separateByBlendShape));
            _skinnedMesh = serializedObject.FindProperty(nameof(MeshProcessDialog._skinnedMesh));
            _eraseBones = serializedObject.FindProperty(nameof(MeshProcessDialog._eraseBones));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            switch (_targetDialog.Tab)
            {
                case MeshProcessDialogTabs.MeshSeparator:
                    {
                        // no properties
                        break;
                    }

                case MeshProcessDialogTabs.MeshIntegrator:
                    {
                        EditorGUILayout.PropertyField(_separateByBlendShape, new GUIContent(MeshProcessingMessages.MESH_SEPARATOR_BY_BLENDSHAPE.Msg()));
                        break;
                    }

                case MeshProcessDialogTabs.BoneMeshEraser:
                    {
                        EditorGUILayout.PropertyField(_skinnedMesh);
                        EditorGUILayout.PropertyField(_eraseBones);
                        break;
                    }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

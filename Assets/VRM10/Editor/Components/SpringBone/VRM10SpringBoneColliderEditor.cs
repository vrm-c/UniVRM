using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneCollider))]
    class VRM10SpringBoneColliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (VRM10Window.Active == target)
            {
                GUI.backgroundColor = Color.cyan;
                Repaint();
            }
            var property = serializedObject.FindProperty("m_Script");
            var script = (VRM10SpringBoneCollider)target;
            EditorGUILayout.PropertyField(property, new GUIContent("Script (" + script.GetIdentificationName() + ")"));
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}

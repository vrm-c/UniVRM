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
            var component = (VRM10SpringBoneCollider)target;
            EditorGUILayout.PropertyField(property, new GUIContent("Script (" + component.GetIdentificationName() + ")"));
            switch (component.ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Plane:
                    // radius: x
                    // tail: x
                    // normal: o
                    DrawPropertiesExcluding(serializedObject, "m_Script", nameof(component.Tail), nameof(component.Radius));
                    break;

                case VRM10SpringBoneColliderTypes.Sphere:
                case VRM10SpringBoneColliderTypes.SphereInside:
                    // radius: o
                    // tail: x
                    // normal: x
                    DrawPropertiesExcluding(serializedObject, nameof(component.Tail), nameof(component.Normal), "m_Script");
                    break;

                case VRM10SpringBoneColliderTypes.Capsule:
                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    // radius: o
                    // tail: o
                    // normal: x
                    DrawPropertiesExcluding(serializedObject, nameof(component.Normal), "m_Script");
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
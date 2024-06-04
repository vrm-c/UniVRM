using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(VRM10SpringBoneCollider))]
    public class VRM10SpringBoneColliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            try
            {
                EditorGUI.ObjectField(rect, property, new GUIContent(((VRM10SpringBoneCollider)property.objectReferenceValue).GetIdentificationName()));
            }
            catch
            {
                EditorGUI.ObjectField(rect, property, new GUIContent());
            }
        }
    }
}
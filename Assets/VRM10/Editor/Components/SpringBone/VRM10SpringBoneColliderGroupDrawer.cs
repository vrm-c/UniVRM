
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(VRM10SpringBoneColliderGroup))]
    public class VRM10SpringBoneColliderGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            try
            {
                EditorGUI.ObjectField(rect, property, new GUIContent(((VRM10SpringBoneColliderGroup)property.objectReferenceValue).Name));
            }
            catch
            {
                EditorGUI.ObjectField(rect, property, new GUIContent("fallback"));
            }
        }
    }
}
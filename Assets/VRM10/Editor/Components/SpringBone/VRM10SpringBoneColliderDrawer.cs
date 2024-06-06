using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(VRM10SpringBoneCollider))]
    public class VRM10SpringBoneColliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            const string ButtonLabel = "Reselect";
            var ButtonSize = GUI.skin.button.CalcSize(new GUIContent(ButtonLabel));
            try
            {
                var collider = property.objectReferenceValue as VRM10SpringBoneCollider;
                EditorGUI.ObjectField(rect, property, new GUIContent(collider.GetIdentificationName()));

                var colliders = collider.transform.GetComponents<VRM10SpringBoneCollider>();
                if (colliders.Length > 1)
                {
                    if (GUI.Button(new Rect(rect.x + EditorGUIUtility.labelWidth - ButtonSize.x, rect.y, ButtonSize.x, rect.height), ButtonLabel))
                    {
                        VRM10SpringBoneColliderEditorWindow.Show(property);
                    }
                }
            }
            catch
            {
                EditorGUI.ObjectField(rect, property, new GUIContent());
            }
        }
    }

    public class VRM10SpringBoneColliderEditorWindow : EditorWindow
    {
        private static SerializedProperty targetProperty;
        private Vector2 scrollPosition;

        public static void Show(SerializedProperty property)
        {
            targetProperty = property;
            var window = CreateInstance<VRM10SpringBoneColliderEditorWindow>();
            window.titleContent = new GUIContent("Reselect Collider from \"" + (property.objectReferenceValue as VRM10SpringBoneCollider).transform.name + "\"");
            window.ShowAuxWindow();
        }

        private void OnGUI()
        {
            if (targetProperty is null)
            {
                Close();
                return;
            }

            var transform = (targetProperty.objectReferenceValue as VRM10SpringBoneCollider).transform;
            var colliders = transform.GetComponents<VRM10SpringBoneCollider>();
            if (colliders.Length == 0)
            {
                Close();
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var collider in colliders)
            {
                if (GUILayout.Button(collider.GetIdentificationName()))
                {
                    targetProperty.objectReferenceValue = collider;
                    targetProperty.serializedObject.ApplyModifiedProperties();
                    Close();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
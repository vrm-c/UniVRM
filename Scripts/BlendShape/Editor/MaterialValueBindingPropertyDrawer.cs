using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomPropertyDrawer(typeof(MaterialValueBinding))]
    public class MaterialValueBindingPropertyDrawer : PropertyDrawer
    {
        public static int GUIElementHeight = 90;

        public override void OnGUI(Rect position,
           SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var y = position.y;
                for (var depth = property.depth;
                    property.NextVisible(true) && property.depth >= depth;
                    )
                {
                    {
                        var height = EditorGUI.GetPropertyHeight(property);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), property, false);
                        y += height;
                    }
                }
            }
        }
    }
}

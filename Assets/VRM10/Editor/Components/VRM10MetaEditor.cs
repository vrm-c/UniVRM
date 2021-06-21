using UniGLTF;
using UnityEditor;

namespace UniVRM10
{
    public class VRM10MetaEditor : SerializedPropertyEditor
    {
        public VRM10MetaEditor(SerializedObject serializedObject, SerializedProperty property) : base(serializedObject, property)
        {
        }

        public static VRM10MetaEditor Create(SerializedObject serializedObject)
        {
            return new VRM10MetaEditor(serializedObject, serializedObject.FindProperty(nameof(VRM10Object.Meta)));
        }

        protected override void RecursiveProperty(SerializedProperty root)
        {
            EditorGUILayout.LabelField("meta");
        }
    }
}

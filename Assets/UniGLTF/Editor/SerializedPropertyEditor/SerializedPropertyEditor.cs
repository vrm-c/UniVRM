using System;
using UnityEditor;

namespace UniGLTF
{
    /// <summary>
    /// ScriptableObject や MonoBehaviour の部分の Editor を表示する。
    /// </summary>
    public class SerializedPropertyEditor
    {
        protected SerializedObject m_serializedObject;
        protected SerializedProperty m_rootProperty;

        public SerializedPropertyEditor(SerializedObject serializedObject, SerializedProperty property)
        {
            m_serializedObject = serializedObject;
            m_rootProperty = property;
        }

        public static SerializedPropertyEditor Create(SerializedObject serializedObject, string name)
        {
            var prop = serializedObject.FindProperty(name);
            if (prop == null)
            {
                throw new ArgumentNullException();
            }
            return new SerializedPropertyEditor(serializedObject, prop);
        }

        public void OnInspectorGUI()
        {
            RecursiveProperty(m_rootProperty);
        }

        protected virtual void RecursiveProperty(SerializedProperty root)
        {
            var depth = root.depth;
            var iterator = root.Copy();
            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.depth < depth)
                {
                    // 前の要素よりも浅くなった。脱出
                    return;
                }
                depth = iterator.depth;

                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }
    }
}

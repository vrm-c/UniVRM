using UnityEditor;

namespace UniVRM10
{
    /// <summary>
    /// 指定した SerializedProperty を起点に再帰的に SerializedProperty を表示する。
    /// </summary>
    class PropGui
    {
        SerializedProperty m_root;

        public PropGui(SerializedProperty property)
        {
            m_root = property;
        }

        // public static PropGui FromSerializedObject(SerializedObject serializedObject, string name)
        // {
        //     var prop = serializedObject.FindProperty(name);
        //     return new PropGui(prop);
        // }

        public void RecursiveProperty()
        {
            var depth = m_root.depth;
            var iterator = m_root.Copy();
            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.depth < depth)
                    return;

                depth = iterator.depth;

                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }
    }
}

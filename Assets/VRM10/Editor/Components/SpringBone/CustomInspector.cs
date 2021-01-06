using System;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    struct CustomInspector : IDisposable
    {
        SerializedObject serializedObject;
        int m_depth;

        Func<SerializedProperty, bool> m_callback;

        public CustomInspector(SerializedObject so, int depth = 0, Func<SerializedProperty, bool> callback = null)
        {
            m_depth = depth;
            m_callback = callback;
            serializedObject = so;
            serializedObject.Update();
        }

        public void OnInspectorGUI()
        {
            int currentDepth = 0;
            for (var iterator = serializedObject.GetIterator(); iterator.NextVisible(true);)
            {
                var isCollapsed = currentDepth < iterator.depth;
                if (isCollapsed)
                {
                    continue;
                }

#if DEBUG
                // Debug.Log($"{iterator.propertyPath}({iterator.propertyType})");
#endif

                if (m_callback is null || !m_callback(iterator))
                {
                    EditorGUI.indentLevel = iterator.depth + m_depth;
                    EditorGUILayout.PropertyField(iterator, false);
                }

                if (iterator.isExpanded)
                {
                    currentDepth = iterator.depth + 1;
                }
                else
                {
                    currentDepth = iterator.depth;
                }
            }
        }

        public void Dispose()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}

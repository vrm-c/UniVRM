using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace UniVRM10
{
    public class ReorderableMaterialUVBindingList
    {
        ReorderableList m_list;
        SerializedProperty m_serializedProperty;
        bool m_changed;

        public ReorderableMaterialUVBindingList(SerializedObject serializedObject, string[] materialNames, int height)
        {
            m_serializedProperty = serializedObject.FindProperty(nameof(VRM10Expression.MaterialUVBindings));
            m_list = new ReorderableList(serializedObject, m_serializedProperty);
            m_list.elementHeight = height * 3;
            m_list.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_serializedProperty.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (DrawMaterialUVBinding(rect, element, materialNames, height))
                  {
                      m_changed = true;
                  }
              };
            m_list.onAddCallback = (m_list) =>
            {
                // first add one element                
                m_serializedProperty.arraySize++;

                // then get that element
                var newIndex = m_serializedProperty.arraySize - 1;
                var newElement = m_serializedProperty.GetArrayElementAtIndex(newIndex);

                // now reset all properties like
                var scaling = newElement.FindPropertyRelative(nameof(MaterialUVBinding.Scaling));
                scaling.vector2Value = Vector2.one;
            };
        }

        ///
        /// Material List のElement描画
        ///
        static bool DrawMaterialUVBinding(Rect position, SerializedProperty property,
            string[] materialNames, int height)
        {
            bool changed = false;
            if (materialNames != null)
            {
                // Materialを選択する
                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int materialIndex;
                if (ExpressionEditorHelper.StringPopup(rect, property.FindPropertyRelative(nameof(MaterialUVBinding.MaterialName)), materialNames, out materialIndex))
                {
                    changed = true;
                }

                // offset
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.UVProp(rect, property.FindPropertyRelative(nameof(MaterialUVBinding.Offset))))
                {
                    changed = true;
                }

                // scale
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.UVProp(rect, property.FindPropertyRelative(nameof(MaterialUVBinding.Scaling))))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public bool Draw(string label)
        {
            m_changed = false;
            m_list.DoLayoutList();
            if (GUILayout.Button($"Clear {label}"))
            {
                m_changed = true;
                m_serializedProperty.arraySize = 0;
            }
            return m_changed;
        }
    }
}

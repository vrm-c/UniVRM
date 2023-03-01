using System;
using UniGLTF.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace UniVRM10
{
    public class ReorderableMaterialColorBindingList
    {
        ReorderableList m_list;
        SerializedProperty m_serializedProperty;
        bool m_changed;
        public ReorderableMaterialColorBindingList(SerializedObject serializedObject, string[] materialNames, int height)
        {
            m_serializedProperty = serializedObject.FindProperty(nameof(VRM10Expression.MaterialColorBindings));
            m_list = new ReorderableList(serializedObject, m_serializedProperty);
            m_list.elementHeight = height * 3;
            m_list.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_serializedProperty.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (DrawMaterialValueBinding(rect, element, materialNames, height))
                  {
                      m_changed = true;
                  }
              };
        }

        ///
        /// Material List のElement描画
        ///
        static bool DrawMaterialValueBinding(Rect position, SerializedProperty property,
            string[] materialNames, int height)
        {
            bool changed = false;
            if (materialNames != null)
            {
                // Material を選択する
                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int materialIndex;
                if (ExpressionEditorHelper.StringPopup(rect, property.FindPropertyRelative(nameof(MaterialColorBinding.MaterialName)), materialNames, out materialIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);

                // 対象のプロパティを enum から選択する
                var bindTypeProp = property.FindPropertyRelative("BindType");
                var bindTypes = CachedEnum.GetValues<UniGLTF.Extensions.VRMC_vrm.MaterialColorType>();
                var bindType = bindTypes[bindTypeProp.enumValueIndex];
                var newBindType = ExpressionEditorHelper.EnumPopup(rect, bindType);
                if (newBindType != bindType)
                {
                    bindTypeProp.enumValueIndex = Array.IndexOf(bindTypes, newBindType);
                    changed = true;
                }

                // 目標の色
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.ColorProp(rect, property.FindPropertyRelative(nameof(MaterialColorBinding.TargetValue))))
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

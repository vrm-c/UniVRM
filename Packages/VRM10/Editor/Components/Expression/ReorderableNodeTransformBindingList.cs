using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace UniVRM10
{
    public class ReorderableNodeTransformBindingList
    {
        ReorderableList m_ValuesList;
        SerializedProperty m_valuesProp;
        bool m_changed;

        public ReorderableNodeTransformBindingList(SerializedObject serializedObject, PreviewSceneManager previewSceneManager, int height)
        {
            m_valuesProp = serializedObject.FindProperty(nameof(VRM10Expression.NodeTransformBindings));
            m_ValuesList = new ReorderableList(serializedObject, m_valuesProp);
            m_ValuesList.elementHeight = height * 4;
            m_ValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_valuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (DrawNodeTransformBinding(rect, element, previewSceneManager, height))
                  {
                      m_changed = true;
                  }
              };
        }

        ///
        /// NodeTransform List のElement描画
        ///
        static bool DrawNodeTransformBinding(Rect position, SerializedProperty property,
            PreviewSceneManager scene, int height)
        {
            bool changed = false;
            if (scene != null)
            {
                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int pathIndex;
                if (ExpressionEditorHelper.StringPopup(rect, property.FindPropertyRelative(nameof(NodeTransformBinding.RelativePath)), scene.NodeTransformPathList, out pathIndex))
                {
                    changed = true;
                }

                // T`
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.Vec3Prop(rect, property.FindPropertyRelative(nameof(NodeTransformBinding.OffsetTranslation))))
                {
                    changed = true;
                }

                // R
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(NodeTransformBinding.OffsetRotation)));
                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                }

                // S
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.Vec3Prop(rect, property.FindPropertyRelative(nameof(NodeTransformBinding.TargetScale))))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public void SetValues(NodeTransformBinding[] bindings)
        {
            m_valuesProp.ClearArray();
            m_valuesProp.arraySize = bindings.Length;
            for (int i = 0; i < bindings.Length; ++i)
            {
                var item = m_valuesProp.GetArrayElementAtIndex(i);

                var endProperty = item.GetEndProperty();
                while (item.NextVisible(true))
                {
                    if (SerializedProperty.EqualContents(item, endProperty))
                    {
                        break;
                    }

                    switch (item.name)
                    {
                        case nameof(NodeTransformBinding.RelativePath):
                            item.stringValue = bindings[i].RelativePath;
                            break;

                        case nameof(NodeTransformBinding.OffsetTranslation):
                            item.vector3Value = bindings[i].OffsetTranslation;
                            break;

                        case nameof(NodeTransformBinding.OffsetRotation):
                            item.quaternionValue = bindings[i].OffsetRotation;
                            break;

                        case nameof(NodeTransformBinding.TargetScale):
                            item.vector3Value = bindings[i].TargetScale;
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }

        }

        public bool Draw(string label)
        {
            m_changed = false;
            m_ValuesList.DoLayoutList();
            if (GUILayout.Button($"Clear {label}"))
            {
                m_changed = true;
                m_valuesProp.arraySize = 0;
            }
            return m_changed;
        }
    }
}

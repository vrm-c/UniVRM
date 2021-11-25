using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace UniVRM10
{
    public class ReorderableMorphTargetBindingList
    {
        ReorderableList m_ValuesList;
        SerializedProperty m_valuesProp;
        bool m_changed;

        public ReorderableMorphTargetBindingList(SerializedObject serializedObject, PreviewSceneManager previewSceneManager, int height)
        {
            m_valuesProp = serializedObject.FindProperty(nameof(VRM10Expression.MorphTargetBindings));
            m_ValuesList = new ReorderableList(serializedObject, m_valuesProp);
            m_ValuesList.elementHeight = height * 3;
            m_ValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_valuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (DrawMorphTargetBinding(rect, element, previewSceneManager, height))
                  {
                      m_changed = true;
                  }
              };
        }

        ///
        /// MorphTarget List のElement描画
        ///
        static bool DrawMorphTargetBinding(Rect position, SerializedProperty property,
            PreviewSceneManager scene, int height)
        {
            bool changed = false;
            if (scene != null)
            {
                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int pathIndex;
                if (ExpressionEditorHelper.StringPopup(rect, property.FindPropertyRelative(nameof(MorphTargetBinding.RelativePath)), scene.SkinnedMeshRendererPathList, out pathIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                int morphTargetIndex;
                if (ExpressionEditorHelper.IntPopup(rect, property.FindPropertyRelative(nameof(MorphTargetBinding.Index)), scene.GetBlendShapeNames(pathIndex), out morphTargetIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (ExpressionEditorHelper.FloatSlider(rect, property.FindPropertyRelative(nameof(MorphTargetBinding.Weight)), MorphTargetBinding.MAX_WEIGHT))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public void SetValues(MorphTargetBinding[] bindings)
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
                        case nameof(MorphTargetBinding.RelativePath):
                            item.stringValue = bindings[i].RelativePath;
                            break;

                        case nameof(MorphTargetBinding.Index):
                            item.intValue = bindings[i].Index;
                            break;

                        case nameof(MorphTargetBinding.Weight):
                            item.floatValue = bindings[i].Weight;
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

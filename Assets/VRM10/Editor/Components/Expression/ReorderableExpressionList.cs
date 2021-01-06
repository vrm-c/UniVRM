using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;

namespace UniVRM10
{
    class ReorderableExpressionList
    {
        ReorderableList m_list;

        public event Action<VRM10Expression> Selected;
        void RaiseSelected(VRM10Expression selected)
        {
            var handler = Selected;
            if (handler == null)
            {
                return;
            }
            handler(selected);
        }

        public void Select(int index)
        {
            m_list.index = index;
        }

        public ReorderableExpressionList(SerializedObject serializedObject, SerializedProperty prop, UnityEngine.Object target)
        {
            m_list = new ReorderableList(serializedObject, prop);

            m_list.drawHeaderCallback = (rect) =>
                                 EditorGUI.LabelField(rect, "Expressions");

            m_list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = prop.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };

            m_list.onAddCallback += (list) =>
            {
                // Add slot
                prop.arraySize++;
                // select last item
                list.index = prop.arraySize - 1;
                // get last item
                var element = prop.GetArrayElementAtIndex(list.index);
                element.objectReferenceValue = null;

                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(target));
                var path = EditorUtility.SaveFilePanel(
                               "Create Expression",
                               dir,
                               string.Format("Expression#{0}.asset", list.count),
                               "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var clip = VRM10ExpressionAvatar.CreateExpression(path.ToUnityRelativePath());
                    //clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(target));

                    element.objectReferenceValue = clip;
                }
            };

            m_list.onSelectCallback += (list) =>
            {
                var a = list.serializedProperty;
                var selected = a.GetArrayElementAtIndex(list.index);
                RaiseSelected((VRM10Expression)selected.objectReferenceValue);
            };

            //m_clipList.onCanRemoveCallback += list => true;
        }

        public void GUI()
        {
            m_list.DoLayoutList();
        }
    }
}

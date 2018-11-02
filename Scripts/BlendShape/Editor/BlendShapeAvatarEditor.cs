using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeAvatar))]
    public class BlendShapeAvatarEditor : Editor
    {
        ReorderableList m_clipList;

        void OnEnable()
        {
            var prop = serializedObject.FindProperty("Clips");
            m_clipList = new ReorderableList(serializedObject, prop);

            m_clipList.drawHeaderCallback = (rect) =>
                                 EditorGUI.LabelField(rect, "BlendShapeClips");

            m_clipList.elementHeight = BlendShapeClipDrawer.Height;
            m_clipList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = prop.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };

            m_clipList.onAddCallback += (list) =>
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
                               "Create BlendShapeClip",
                               dir,
                               string.Format("BlendShapeClip#{0}.asset", list.count),
                               "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var clip = BlendShapeAvatar.CreateBlendShapeClip(path.ToUnityRelativePath());
                    //clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(target));

                    element.objectReferenceValue = clip;
                }
            };

            m_clipList.onCanRemoveCallback += list => true;
        }

        void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_clipList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

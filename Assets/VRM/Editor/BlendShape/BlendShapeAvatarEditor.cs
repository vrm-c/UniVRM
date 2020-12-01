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
    public class BlendShapeAvatarEditor : PreviewEditor
    {
        ReorderableList m_clipList;

        BlendShapeClipSelector m_selector;

        SerializedBlendShapeEditor m_clipEditor;

        protected override PreviewSceneManager.BakeValue GetBakeValue()
        {
            var clip = m_selector.Selected;
            var value = new PreviewSceneManager.BakeValue();
            if (clip != null)
            {
                value.BlendShapeBindings = clip.Values;
                value.MaterialValueBindings = clip.MaterialValues;
                value.Weight = 1.0f;
            }
            return value;
        }

        void OnSelected(BlendShapeClip clip)
        {
            if (PreviewSceneManager == null)
            {
                m_clipEditor = null;
            }
            else if (clip != null)
            {
                m_clipEditor = new SerializedBlendShapeEditor(clip, PreviewSceneManager);
                PreviewSceneManager.Bake(new PreviewSceneManager.BakeValue
                {
                    BlendShapeBindings = clip.Values,
                    MaterialValueBindings = clip.MaterialValues,
                    Weight = 1.0f
                });
            }
            else
            {
                m_clipEditor = null;
                PreviewSceneManager.Bake(new PreviewSceneManager.BakeValue());
            }
        }

        protected override void OnEnable()
        {
            m_selector = new BlendShapeClipSelector((BlendShapeAvatar)target, OnSelected);

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

            m_clipList.onSelectCallback += (list) =>
            {
                var a = list.serializedProperty;
                var selected = a.GetArrayElementAtIndex(list.index);
                OnSelected((BlendShapeClip)selected.objectReferenceValue);
            };

            //m_clipList.onCanRemoveCallback += list => true;
            base.OnEnable();

            OnSelected(m_selector.Selected);
        }

        int m_mode;
        static readonly string[] MODES = new string[]{
            "Editor",
            "List"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            m_mode = GUILayout.Toolbar(m_mode, MODES);
            switch (m_mode)
            {
                case 0:
                    m_selector.SelectGUI();
                    if (m_clipEditor != null)
                    {
                        Separator();
                        var result = m_clipEditor.Draw();
                        if (result.Changed)
                        {
                            PreviewSceneManager.Bake(new PreviewSceneManager.BakeValue
                            {
                                BlendShapeBindings = result.BlendShapeBindings,
                                MaterialValueBindings = result.MaterialValueBindings,
                                Weight = 1.0f,
                            });
                        }
                    }
                    break;

                case 1:
                    m_clipList.DoLayoutList();
                    break;

                default:
                    throw new NotImplementedException();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

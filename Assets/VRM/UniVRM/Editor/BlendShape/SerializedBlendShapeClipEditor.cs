using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VRM
{
    public class SerializedBlendShapeEditor
    {
        BlendShapeClip m_targetObject;

        SerializedObject m_serializedObject;

        #region  Properties
        SerializedProperty m_thumbnail;
        SerializedProperty m_blendShapeNameProp;
        SerializedProperty m_presetProp;

        SerializedProperty m_isBinaryProp;
        #endregion

        #region BlendShapeBind
        public static int BlendShapeBindingHeight = 60;
        ReorderableList m_ValuesList;

        SerializedProperty m_valuesProp;
        #endregion

        #region  MaterialValueBind
        const int MaterialValueBindingHeight = 90;
        ReorderableList m_MaterialValuesList;

        SerializedProperty m_materialsProp;
        #endregion

        #region  Editor values
        //float m_previewSlider = 1.0f;

        bool m_changed;

        int m_mode;
        static string[] MODES = new[]{
            "BlendShape",
            "BlendShape List",
            "Material List"
        };

        MeshPreviewItem[] m_items;
        #endregion

        public SerializedBlendShapeEditor(SerializedObject serializedObject,
            PreviewSceneManager previewSceneManager) : this(
                serializedObject, (BlendShapeClip)serializedObject.targetObject, previewSceneManager)
        { }

        public SerializedBlendShapeEditor(BlendShapeClip blendShapeClip,
            PreviewSceneManager previewSceneManager) : this(
                new SerializedObject(blendShapeClip), blendShapeClip, previewSceneManager)
        { }

        public SerializedBlendShapeEditor(SerializedObject serializedObject, BlendShapeClip targetObject,
            PreviewSceneManager previewSceneManager)
        {
            this.m_serializedObject = serializedObject;
            this.m_targetObject = targetObject;

            //m_thumbnail = serializedObject.FindProperty("Thumbnail");
            m_blendShapeNameProp = serializedObject.FindProperty("BlendShapeName");
            m_presetProp = serializedObject.FindProperty("Preset");
            m_isBinaryProp = serializedObject.FindProperty("IsBinary");

            m_valuesProp = serializedObject.FindProperty("Values");

            m_ValuesList = new ReorderableList(serializedObject, m_valuesProp);
            m_ValuesList.elementHeight = BlendShapeBindingHeight;
            m_ValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_valuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (BlendShapeClipEditorHelper.DrawBlendShapeBinding(rect, element, previewSceneManager))
                  {
                      m_changed = true;
                  }
              };

            m_materialsProp = serializedObject.FindProperty("MaterialValues");
            m_MaterialValuesList = new ReorderableList(serializedObject, m_materialsProp);
            m_MaterialValuesList.elementHeight = MaterialValueBindingHeight;
            m_MaterialValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) =>
              {
                  var element = m_materialsProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if (BlendShapeClipEditorHelper.DrawMaterialValueBinding(rect, element, previewSceneManager))
                  {
                      m_changed = true;
                  }
              };

            m_items = previewSceneManager.EnumRenderItems
            .Where(x => x.SkinnedMeshRenderer != null)
            .ToArray();
        }

        public struct DrawResult
        {
            public bool Changed;

            public BlendShapeBinding[] BlendShapeBindings;

            public MaterialValueBinding[] MaterialValueBindings;
        }

        public DrawResult Draw()
        {
            m_changed = false;

            m_serializedObject.Update();

            // Readonly のBlendShapeClip参照
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Current clip",
                m_targetObject, typeof(BlendShapeClip), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(m_blendShapeNameProp, true);
            EditorGUILayout.PropertyField(m_presetProp, true);

            // v0.45 Added. Binary flag
            EditorGUILayout.PropertyField(m_isBinaryProp, true);

            EditorGUILayout.Space();
            //m_mode = EditorGUILayout.Popup("SourceType", m_mode, MODES);
            m_mode = GUILayout.Toolbar(m_mode, MODES);
            switch (m_mode)
            {
                case 0:
                    {
                        ClipGUI();
                    }
                    break;

                case 1:
                    {
                        if (GUILayout.Button("Clear"))
                        {
                            m_changed = true;
                            m_valuesProp.arraySize = 0;
                        }
                        m_ValuesList.DoLayoutList();
                    }
                    break;

                case 2:
                    {
                        if (GUILayout.Button("Clear"))
                        {
                            m_changed = true;
                            m_materialsProp.arraySize = 0;
                        }
                        m_MaterialValuesList.DoLayoutList();
                    }
                    break;
            }

            m_serializedObject.ApplyModifiedProperties();

            return new DrawResult
            {
                Changed = m_changed,
                BlendShapeBindings = m_targetObject.Values,
                MaterialValueBindings = m_targetObject.MaterialValues
            };
        }

        void ClipGUI()
        {
            var changed = BlendShapeBindsGUI();
            if (changed)
            {
                string maxWeightName;
                var bindings = GetBindings(out maxWeightName);
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
                            case "RelativePath":
                                item.stringValue = bindings[i].RelativePath;
                                break;

                            case "Index":
                                item.intValue = bindings[i].Index;
                                break;

                            case "Weight":
                                item.floatValue = bindings[i].Weight;
                                break;

                            default:
                                throw new Exception();
                        }
                    }
                }

                m_changed = true;
            }
        }

        List<bool> m_meshFolds = new List<bool>();
        bool BlendShapeBindsGUI()
        {
            bool changed = false;
            int foldIndex = 0;
            // すべてのSkinnedMeshRendererを列挙する
            foreach (var renderer in m_items.Select(x => x.SkinnedMeshRenderer))
            {
                var mesh = renderer.sharedMesh;
                if (mesh != null && mesh.blendShapeCount > 0)
                {
                    //var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, m_target.transform);
                    //EditorGUILayout.LabelField(m_target.name + "/" + item.Path);

                    if (foldIndex >= m_meshFolds.Count)
                    {
                        m_meshFolds.Add(false);
                    }
                    m_meshFolds[foldIndex] = EditorGUILayout.Foldout(m_meshFolds[foldIndex], renderer.name);
                    if (m_meshFolds[foldIndex])
                    {
                        //EditorGUI.indentLevel += 1;
                        for (int i = 0; i < mesh.blendShapeCount; ++i)
                        {
                            var src = renderer.GetBlendShapeWeight(i);
                            var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, 100.0f);
                            if (dst != src)
                            {
                                renderer.SetBlendShapeWeight(i, dst);
                                changed = true;
                            }
                        }
                        //EditorGUI.indentLevel -= 1;
                    }
                    ++foldIndex;
                }
            }
            return changed;
        }

        BlendShapeBinding[] GetBindings(out string _maxWeightName)
        {
            var maxWeight = 0.0f;
            var maxWeightName = "";
            // weightのついたblendShapeを集める
            var values = m_items
                .SelectMany(x =>
            {
                var mesh = x.SkinnedMeshRenderer.sharedMesh;

                var relativePath = x.Path;

                var list = new List<BlendShapeBinding>();
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        var weight = x.SkinnedMeshRenderer.GetBlendShapeWeight(i);
                        if (weight == 0)
                        {
                            continue;
                        }
                        var name = mesh.GetBlendShapeName(i);
                        if (weight > maxWeight)
                        {
                            maxWeightName = name;
                            maxWeight = weight;
                        }
                        list.Add(new BlendShapeBinding
                        {
                            Index = i,
                            RelativePath = relativePath,
                            Weight = weight
                        });
                    }
                }
                return list;
            }).ToArray()
            ;
            _maxWeightName = maxWeightName;
            return values;
        }
    }
}

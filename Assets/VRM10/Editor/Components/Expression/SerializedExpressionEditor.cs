using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Expression カスタムエディタの実装
    /// </summary>
    public class SerializedExpressionEditor
    {
        VRM10Expression m_targetObject;

        SerializedObject m_serializedObject;

        #region  Properties
        SerializedProperty m_thumbnail;
        SerializedProperty m_isBinaryProp;

        public bool IsBinary => m_isBinaryProp.boolValue;

        SerializedProperty m_ignoreBlinkProp;
        SerializedProperty m_ignoreLookAtProp;
        SerializedProperty m_ignoreMouthProp;
        #endregion

        ReorderableMorphTargetBindingList m_morphTargetBindings;
        ReorderableMaterialColorBindingList m_materialColorBindings;
        ReorderableMaterialUVBindingList m_materialUVBindings;

        #region  Editor values

        bool m_changed;

        static int s_Mode;
        static bool s_MorphTargetFoldout = true;
        static bool s_OptionFoldout;
        static bool s_ListFoldout;

        static string[] MODES = new[]{
            "MorphTarget",
            "Material Color",
            "Texture Transform"
        };

        PreviewMeshItem[] m_items;
        #endregion

        public SerializedExpressionEditor(SerializedObject serializedObject,
            PreviewSceneManager previewSceneManager) : this(
                serializedObject, (VRM10Expression)serializedObject.targetObject, previewSceneManager)
        { }

        public SerializedExpressionEditor(VRM10Expression expression,
            PreviewSceneManager previewSceneManager) : this(
                new SerializedObject(expression), expression, previewSceneManager)
        { }

        public SerializedExpressionEditor(SerializedObject serializedObject, VRM10Expression targetObject,
            PreviewSceneManager previewSceneManager)
        {
            this.m_serializedObject = serializedObject;
            this.m_targetObject = targetObject;

            m_isBinaryProp = serializedObject.FindProperty(nameof(targetObject.IsBinary));
            m_ignoreBlinkProp = serializedObject.FindProperty(nameof(targetObject.OverrideBlink));
            m_ignoreLookAtProp = serializedObject.FindProperty(nameof(targetObject.OverrideLookAt));
            m_ignoreMouthProp = serializedObject.FindProperty(nameof(targetObject.OverrideMouth));

            m_morphTargetBindings = new ReorderableMorphTargetBindingList(serializedObject, previewSceneManager, 20);
            m_materialColorBindings = new ReorderableMaterialColorBindingList(serializedObject, previewSceneManager?.MaterialNames, 20);
            m_materialUVBindings = new ReorderableMaterialUVBindingList(serializedObject, previewSceneManager?.MaterialNames, 20);

            m_items = previewSceneManager.EnumRenderItems
            .Where(x => x.SkinnedMeshRenderer != null)
            .ToArray();
        }

        public bool Draw(out VRM10Expression bakeValue)
        {
            m_changed = false;

            m_serializedObject.Update();

            // Readonly の Expression 参照
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Current clip",
                m_targetObject, typeof(VRM10Expression), false);
            GUI.enabled = true;

            s_MorphTargetFoldout = CustomUI.Foldout(s_MorphTargetFoldout, "MorphTarget");
            if (s_MorphTargetFoldout)
            {
                EditorGUI.indentLevel++;
                var changed = MorphTargetBindsGUI();
                if (changed)
                {
                    string maxWeightName;
                    var bindings = GetBindings(out maxWeightName);
                    m_morphTargetBindings.SetValues(bindings);

                    m_changed = true;
                }
                EditorGUI.indentLevel--;
            }

            s_ListFoldout = CustomUI.Foldout(s_ListFoldout, "List");
            if (s_ListFoldout)
            {
                EditorGUI.indentLevel++;
                s_Mode = GUILayout.Toolbar(s_Mode, MODES);
                switch (s_Mode)
                {
                    case 0:
                        // MorphTarget
                        {
                            if (m_morphTargetBindings.Draw("MorphTarget"))
                            {
                                m_changed = true;
                            }
                        }
                        break;

                    case 1:
                        // Material
                        {
                            if (m_materialColorBindings.Draw("MaterialColor"))
                            {
                                m_changed = true;
                            }
                        }
                        break;

                    case 2:
                        // TextureTransform
                        {
                            if (m_materialUVBindings.Draw("TextureTransform"))
                            {
                                m_changed = true;
                            }
                        }
                        break;
                }
                EditorGUI.indentLevel--;
            }

            s_OptionFoldout = CustomUI.Foldout(s_OptionFoldout, "Option");
            if (s_OptionFoldout)
            {
                EditorGUI.indentLevel++;

                // v0.45 Added. Binary flag
                EditorGUILayout.PropertyField(m_isBinaryProp, true);

                // v1.0 Ignore State
                EditorGUILayout.PropertyField(m_ignoreBlinkProp, true);
                EditorGUILayout.PropertyField(m_ignoreLookAtProp, true);
                EditorGUILayout.PropertyField(m_ignoreMouthProp, true);


                EditorGUI.indentLevel--;
            }

            m_serializedObject.ApplyModifiedProperties();

            bakeValue = m_targetObject;
            return m_changed;
        }

        static List<bool> m_meshFolds = new List<bool>();
        bool MorphTargetBindsGUI()
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
                            var src = renderer.GetBlendShapeWeight(i) * MorphTargetBinding.UNITY_TO_VRM;
                            var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, MorphTargetBinding.MAX_WEIGHT);
                            if (dst != src)
                            {
                                renderer.SetBlendShapeWeight(i, dst * MorphTargetBinding.VRM_TO_UNITY);
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

        MorphTargetBinding[] GetBindings(out string _maxWeightName)
        {
            var maxUnityWeight = 0.0f;
            var maxWeightName = "";
            // weightのついたblendShapeを集める
            var values = m_items
                .SelectMany(x =>
            {
                var mesh = x.SkinnedMeshRenderer.sharedMesh;

                var relativePath = x.Path;

                var list = new List<MorphTargetBinding>();
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        var unityWeight = x.SkinnedMeshRenderer.GetBlendShapeWeight(i);
                        if (unityWeight == 0)
                        {
                            continue;
                        }
                        var name = mesh.GetBlendShapeName(i);
                        if (unityWeight > maxUnityWeight)
                        {
                            maxWeightName = name;
                            maxUnityWeight = unityWeight;
                        }
                        list.Add(new MorphTargetBinding(relativePath, i, unityWeight * MorphTargetBinding.UNITY_TO_VRM));
                    }
                }
                return list;
            }).ToArray()
            ;
            _maxWeightName = maxWeightName;
            return values;
        }
    }

    /// http://tips.hecomi.com/entry/2016/10/15/004144
    public static class CustomUI
    {
        public static bool Foldout(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeAvatar))]
    public class BlendShapeAvatarEditor : PreviewEditor
    {
        static String[] Presets = ((BlendShapePreset[])Enum.GetValues(typeof(BlendShapePreset)))
            .Select(x => x.ToString()).ToArray();

        BlendShapeClipSelect m_selector;

        void OnPrefabChanged()
        {
            if (m_selector != null)
            {
                Bake(m_selector.Selected, 1.0f);
            }
        }

        protected override void OnEnable()
        {
            PrefabChanged += OnPrefabChanged;
            base.OnEnable();

            m_selector = new BlendShapeClipSelect((BlendShapeAvatar)target, clip => Bake(clip, 1.0f));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PrefabChanged -= OnPrefabChanged;
        }

        List<bool> m_meshFolds = new List<bool>();
        int m_preset;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_selector.SelectGUI();

            ClipGUI(m_selector.Selected);
        }

        void ClipGUI(BlendShapeClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("no clip");
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CurrentClip", EditorStyles.boldLabel);

            // ReadonlyのBlendShapeClip参照
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Current clip",
                clip, typeof(BlendShapeClip), false);
            GUI.enabled = true;

            // Preset選択
            clip.Preset = (BlendShapePreset)EditorGUILayout.Popup("Preset", (int)clip.Preset, Presets);

            // Readonlyの名前入力
            GUI.enabled = false;
            EditorGUILayout.TextField("BlendShapeName", clip.BlendShapeName);
            GUI.enabled = true;

            // Key重複の警告
            m_selector.DuplicateWarn();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("BlendShapeValues", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear"))
            {
                ClearBlendShape();
            }

            if (clip != null && GUILayout.Button("Apply"))
            {
                string maxWeightString;
                clip.Values = GetBindings(out maxWeightString);
                EditorUtility.SetDirty(clip);
            }
            EditorGUILayout.EndHorizontal();

            if (PreviewSceneManager != null)
            {
                if (BlendShapeBindsGUI(clip))
                {
                    PreviewSceneManager.Bake();
                }
            }
        }

        bool BlendShapeBindsGUI(BlendShapeClip clip)
        {
            bool changed = false;
            int foldIndex = 0;
            // すべてのSkinnedMeshRendererを列挙する
            foreach (var item in PreviewSceneManager.EnumRenderItems.Where(x => x.SkinnedMeshRenderer != null))
            {
                var mesh = item.SkinnedMeshRenderer.sharedMesh;
                if (mesh != null && mesh.blendShapeCount > 0)
                {
                    //var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, m_target.transform);
                    //EditorGUILayout.LabelField(m_target.name + "/" + item.Path);

                    if (foldIndex >= m_meshFolds.Count)
                    {
                        m_meshFolds.Add(false);
                    }
                    m_meshFolds[foldIndex] = EditorGUILayout.Foldout(m_meshFolds[foldIndex], item.SkinnedMeshRenderer.name);
                    if (m_meshFolds[foldIndex])
                    {
                        //EditorGUI.indentLevel += 1;
                        for (int i = 0; i < mesh.blendShapeCount; ++i)
                        {
                            var src = item.SkinnedMeshRenderer.GetBlendShapeWeight(i);
                            var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, 100.0f);
                            if (dst != src)
                            {
                                item.SkinnedMeshRenderer.SetBlendShapeWeight(i, dst);
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
            var values = PreviewSceneManager.EnumRenderItems
                .Where(x => x.SkinnedMeshRenderer != null)
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

        private void ClearBlendShape()
        {
            foreach (var item in PreviewSceneManager.EnumRenderItems.Where(x => x.SkinnedMeshRenderer != null))
            {
                var renderer = item.SkinnedMeshRenderer;
                var mesh = renderer.sharedMesh;
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        renderer.SetBlendShapeWeight(i, 0);
                    }
                }
            }
        }
    }
}

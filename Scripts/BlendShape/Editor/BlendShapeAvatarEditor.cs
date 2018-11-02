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

        BlendShapeClipSelector m_selector;

        SerializedBlendShapeEditor m_clipEditor;

        void OnPrefabChanged()
        {
            if (m_selector != null)
            {
                Bake(m_selector.Selected, 1.0f);
            }
        }

        void OnSelected(BlendShapeClip clip)
        {
            Bake(clip, 1.0f);
            if (clip != null)
            {
                m_clipEditor = new SerializedBlendShapeEditor(clip, PreviewSceneManager);
            }
            else
            {
                m_clipEditor = null;
            }
        }

        protected override void OnEnable()
        {
            PrefabChanged += OnPrefabChanged;
            base.OnEnable();

            m_selector = new BlendShapeClipSelector((BlendShapeAvatar)target, OnSelected);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PrefabChanged -= OnPrefabChanged;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_selector.SelectGUI();

            if (m_clipEditor != null)
            {
                Separator();

                var result = m_clipEditor.Draw();
                if (result.Changed)
                {
                    Bake(result.BlendShapeBindings, result.MaterialValueBindings, result.Weight);
                }
            }
        }

        /*
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
                */
    }
}

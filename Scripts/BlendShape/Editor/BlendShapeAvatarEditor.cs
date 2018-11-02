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

                m_selector.DuplicateWarn();

                var result = m_clipEditor.Draw();
                if (result.Changed)
                {
                    Bake(result.BlendShapeBindings, result.MaterialValueBindings, result.Weight);
                }
            }
        }
    }
}

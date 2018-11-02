using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : PreviewEditor
    {
        SerializedBlendShapeEditor m_serializedEditor;

        BlendShapeClip m_target;

        protected override GameObject GetPrefab()
        {
            return m_target.Prefab;
        }

        void OnPrefabChanged()
        {
            m_target.Prefab = Prefab;
            Bake(m_target.Values, m_target.MaterialValues, 1.0f);
        }

        protected override void OnEnable()
        {
            m_target = (BlendShapeClip)target;
            PrefabChanged += OnPrefabChanged;

            base.OnEnable();

            Bake(m_target.Values, m_target.MaterialValues, 1.0f);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PrefabChanged -= OnPrefabChanged;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_serializedEditor == null)
            {
                m_serializedEditor = new SerializedBlendShapeEditor(serializedObject, PreviewSceneManager);
            }

            var result = m_serializedEditor.Draw();
            if (result.Changed && PreviewSceneManager != null)
            {
                PreviewSceneManager.Bake(result.BlendShapeBindings, result.MaterialValueBindings, result.Weight);
            }
        }

        public override string GetInfoString()
        {
            return BlendShapeKey.CreateFrom((BlendShapeClip)target).ToString();
        }
    }
}

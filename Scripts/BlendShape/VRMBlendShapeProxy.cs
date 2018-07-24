using System;
using System.Collections.Generic;
using UnityEngine;


namespace VRM
{
    [DisallowMultipleComponent]
    public class VRMBlendShapeProxy : MonoBehaviour, IVRMComponent
    {
        [SerializeField]
        public BlendShapeAvatar BlendShapeAvatar;

        public void OnImported(VRMImporterContext context)
        {
            throw new NotImplementedException();
        }

        BlendShapeMerger m_merger;

        private void OnDestroy()
        {
            if (m_merger != null)
            {
                m_merger.RestoreMaterialInitialValues(BlendShapeAvatar.Clips);
            }
        }

        private void Start()
        {
            if (BlendShapeAvatar != null)
            {
                if (m_merger == null)
                {
                    m_merger = new BlendShapeMerger(BlendShapeAvatar.Clips, transform);
                }
            }
        }

        public void SetValue(BlendShapePreset key, float value, bool apply = true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(BlendShapePreset key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(String key, float value, bool apply = true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(String key)
        {
            return GetValue(new BlendShapeKey(key));
        }

        [Obsolete("Use SetValues")]
        public void SetValue(BlendShapeKey key, float value, bool apply)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, apply);
            }
        }

        public void SetValue(BlendShapeKey key, float value)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, true);
            }
        }

        public void SetValues(IEnumerable<KeyValuePair<BlendShapeKey, float>> values)
        {
            if (m_merger != null)
            {
                m_merger.SetValues(values);
            }
        }

        public float GetValue(BlendShapeKey key)
        {
            if (m_merger == null)
            {
                return 0;
            }
            return m_merger.GetValue(key);
        }

        public void ClearKeys()
        {
            if (m_merger != null)
            {
                m_merger.Clear();
            }
        }

        public void Apply()
        {
            if (m_merger != null)
            {
                m_merger.Apply();
            }
        }
    }
}

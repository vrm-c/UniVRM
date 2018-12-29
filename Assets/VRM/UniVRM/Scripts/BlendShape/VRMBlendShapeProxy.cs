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

        /// <summary>
        /// Immediately SetValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ImmediatelySetValue(BlendShapeKey key, float value)
        {
            if (m_merger != null)
            {
                m_merger.ImmediatelySetValue(key, value);
            }
        }

        /// <summary>
        /// AccumulateValue. After, Should call Apply
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AccumulateValue(BlendShapeKey key, float value)
        {
            if (m_merger != null)
            {
                m_merger.AccumulateValue(key, value);
            }
        }

        /// <summary>
        /// Get a blendShape value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetValue(BlendShapeKey key)
        {
            if (m_merger == null)
            {
                return 0;
            }
            return m_merger.GetValue(key);
        }

        public IEnumerable<KeyValuePair<BlendShapeKey, float>> GetValues()
        {
            if (m_merger != null && BlendShapeAvatar != null)
            {
                foreach (var clip in BlendShapeAvatar.Clips)
                {
                    var key = BlendShapeKey.CreateFrom(clip);
                    yield return new KeyValuePair<BlendShapeKey, float>(key, m_merger.GetValue(key));
                }
            }
        }

        /// <summary>
        /// Set blendShape values immediate.
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(IEnumerable<KeyValuePair<BlendShapeKey, float>> values)
        {
            if (m_merger != null)
            {
                m_merger.SetValues(values);
            }
        }

        /// <summary>
        /// Apply blendShape values that use SetValue apply=false
        /// </summary>
        public void Apply()
        {
            if (m_merger != null)
            {
                m_merger.Apply();
            }
        }
    }

    public static class VRMBlendShapeProxyExtensions
    {
        public static float GetValue(this VRMBlendShapeProxy proxy, BlendShapePreset key)
        {
            return proxy.GetValue(new BlendShapeKey(key));
        }

        public static float GetValue(this VRMBlendShapeProxy proxy, String key)
        {
            return proxy.GetValue(new BlendShapeKey(key));
        }

        [Obsolete("Use ImmediatelySetValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, BlendShapePreset key, float value)
        {
            proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
        }
        public static void ImmediatelySetValue(this VRMBlendShapeProxy proxy, BlendShapePreset key, float value)
        {
            proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
        }
        public static void AccumulateValue(this VRMBlendShapeProxy proxy, BlendShapePreset key, float value)
        {
            proxy.AccumulateValue(new BlendShapeKey(key), value);
        }

        [Obsolete("Use ImmediatelySetValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, String key, float value)
        {
            proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
        }
        public static void ImmediatelySetValue(this VRMBlendShapeProxy proxy, String key, float value)
        {
            proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
        }
        public static void AccumulateValue(this VRMBlendShapeProxy proxy, String key, float value)
        {
            proxy.AccumulateValue(new BlendShapeKey(key), value);
        }

        [Obsolete("Use ImmediatelySetValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, BlendShapeKey key, float value)
        {
            proxy.ImmediatelySetValue(key, value);
        }

        [Obsolete("Use ImmediatelySetValue or AccumulateValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, BlendShapePreset key, float value, bool apply)
        {
            if (apply)
            {
                proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
            }
            else
            {
                proxy.AccumulateValue(new BlendShapeKey(key), value);
            }
        }

        [Obsolete("Use ImmediatelySetValue or AccumulateValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, String key, float value, bool apply)
        {
            if (apply)
            {
                proxy.ImmediatelySetValue(new BlendShapeKey(key), value);
            }
            else
            {
                proxy.AccumulateValue(new BlendShapeKey(key), value);
            }
        }

        [Obsolete("Use ImmediatelySetValue or AccumulateValue")]
        public static void SetValue(this VRMBlendShapeProxy proxy, BlendShapeKey key, float value, bool apply)
        {
            if (apply)
            {
                proxy.ImmediatelySetValue(key, value);
            }
            else
            {
                proxy.AccumulateValue(key, value);
            }
        }
    }
}

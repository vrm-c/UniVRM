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
        /// Set a blendShape value immediate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(BlendShapePreset key, float value)
        {
#pragma warning disable 0618
            SetValue(new BlendShapeKey(key), value, true);
#pragma warning restore 0618
        }

        /// <summary>
        /// Set a blendShape value immediate or delayed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="apply">immediate if true</param>
        [Obsolete("Use SetValues")]
        public void SetValue(BlendShapePreset key, float value, bool apply)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }

        /// <summary>
        /// Get a blendShape value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetValue(BlendShapePreset key)
        {
            return GetValue(new BlendShapeKey(key));
        }

        /// <summary>
        /// Set a blendShape value immediate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(String key, float value)
        {
#pragma warning disable 0618
            SetValue(new BlendShapeKey(key), value, true);
#pragma warning restore 0618
        }

        /// <summary>
        /// Set a blendShape value immediate or delayed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="apply">immediate if true</param>
        [Obsolete("Use SetValues")]
        public void SetValue(String key, float value, bool apply)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }

        /// <summary>
        /// Get a blendShape value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetValue(String key)
        {
            return GetValue(new BlendShapeKey(key));
        }

        /// <summary>
        /// Set a blendShape value immediate or delayed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="apply">immediate if true</param>
        [Obsolete("Use SetValues")]
        public void SetValue(BlendShapeKey key, float value, bool apply)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, apply);
            }
        }

        /// <summary>
        /// Set a blendShape value immediate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(BlendShapeKey key, float value)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, true);
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

        /*
        /// <summary>
        /// Clear all blendShape values
        /// </summary>
        public void ClearKeys()
        {
            if (m_merger != null)
            {
                m_merger.Clear();
            }
        }
        */

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
}

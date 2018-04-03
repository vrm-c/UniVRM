using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace VRM
{
    [DisallowMultipleComponent]
    public class VRMBlendShapeProxy : MonoBehaviour
    {
        [SerializeField]
        public BlendShapeAvatar BlendShapeAvatar;

        struct BlendShapePath
        {
            public String RelativePath;
            public int Index;
        }

        delegate void BlendShapeSetter(float value);

        class BlendShapePathHandler
        {
            public BlendShapeSetter Setter;
            float m_value;

            /*
            public void ReplaceValue(float value)
            {
                m_value = value;
            }
            */

            public void AddValue(float value)
            {
                m_value += value;
            }

            public void Apply(float value)
            {
                Setter(value);
                m_value = 0;
            }

            public void Apply()
            {
                Setter(m_value);
                m_value = 0;
            }

            public void Clear()
            {
                m_value = 0;
                Apply();
            }
        }

        class BlendShapeMerger
        {
            Dictionary<BlendShapeKey, BlendShapeClip> m_clipMap;
            Dictionary<BlendShapeKey, float> m_valueMap;
            Dictionary<BlendShapeBinding, BlendShapePathHandler> m_setterMap = new Dictionary<BlendShapeBinding, BlendShapePathHandler>();

            public BlendShapeMerger(IEnumerable<BlendShapeClip> clips, Transform root)
            {
                m_clipMap = clips.ToDictionary(x => BlendShapeKey.CreateFrom(x), x => x);
                m_valueMap = new Dictionary<BlendShapeKey, float>();
                foreach (var kv in m_clipMap)
                {
                    foreach (var binding in kv.Value.Values) {

                        if (!m_setterMap.ContainsKey(binding))
                        {
                            var _target = root.Find(binding.RelativePath);
                            SkinnedMeshRenderer target = null;
                            if (_target != null)
                            {
                                target = _target.GetComponent<SkinnedMeshRenderer>();
                            }                           
                            if (target != null)
                            {
                                m_setterMap.Add(binding, new BlendShapePathHandler
                                {
                                    Setter = x =>
                                    {
                                        target.SetBlendShapeWeight(binding.Index, x);
                                    }
                                });
                            }
                            else
                            {
                                Debug.LogWarningFormat("{0} not found", binding.RelativePath);
                            }
                        }
                    }
                }
            }

            public void Clear()
            {
                foreach(var kv in m_setterMap)
                {
                    kv.Value.Clear();
                }
            }

            public void Restore()
            {
                foreach (var kv in m_valueMap.ToArray())
                {
                    SetValue(kv.Key, kv.Value, false);
                }
            }

            public void Apply()
            {
                foreach(var kv in m_setterMap)
                {
                    kv.Value.Apply();
                }
            }

            public void SetValue(BlendShapeKey key, float value, bool replace)
            {
                m_valueMap[key] = value;

                BlendShapeClip clip;
                if(!m_clipMap.TryGetValue(key, out clip))
                {
                    return;
                }

                foreach(var binding in clip.Values)
                {
                    BlendShapePathHandler handler;
                    if(m_setterMap.TryGetValue(binding, out handler))
                    {
                        if (replace)
                        {
                            // 値置き換え
                            //handler.ReplaceValue();
                            handler.Apply(binding.Weight * value);
                        }
                        else
                        {
                            // 積算
                            handler.AddValue(binding.Weight * value);
                        }                       
                    }
                    else
                    {
                        Debug.LogWarningFormat("'{0}' not found", binding);
                    }
                }
            }

            public float GetValue(BlendShapeKey key)
            {
                float value;
                if (!m_valueMap.TryGetValue(key, out value))
                {
                    return 0;
                }
                return value;
            }
        }
        BlendShapeMerger m_merger;

        private void Update()
        {
            if (BlendShapeAvatar != null)
            {
                if (m_merger == null)
                {
                    m_merger = new BlendShapeMerger(BlendShapeAvatar.Clips, transform);
                }
            }
        }

        public void SetValue(BlendShapePreset key, float value, bool apply=true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(BlendShapePreset key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(String key, float value, bool apply=true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(String key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(BlendShapeKey key, float value, bool apply=true)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, apply);
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

        public void Restore()
        {
            if (m_merger != null)
            {
                m_merger.Restore();
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

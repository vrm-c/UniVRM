using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniGLTF;


namespace VRM
{
    class BlendShapeMerger
    {
        delegate void BlendShapeSetter(float value);

        Dictionary<BlendShapeKey, BlendShapeClip> m_clipMap;
        Dictionary<BlendShapeKey, float> m_valueMap;

        class BlendShapePathHandler
        {
            public BlendShapeSetter Setter;
            float m_value;

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
        Dictionary<BlendShapeBinding, BlendShapePathHandler> m_setterMap = new Dictionary<BlendShapeBinding, BlendShapePathHandler>();
        Dictionary<string, Material> m_materialMap;

        public BlendShapeMerger(IEnumerable<BlendShapeClip> clips, Transform root)
        {
            m_materialMap = new Dictionary<string, Material>();
            foreach (var x in root.Traverse())
            {
                var renderer = x.GetComponent<Renderer>();
                if (renderer != null)
                {
                    foreach (var y in renderer.sharedMaterials.Where(y => y != null))
                    {
                        if (!string.IsNullOrEmpty(y.name))
                        {
                            if (!m_materialMap.ContainsKey(y.name))
                            {
                                m_materialMap.Add(y.name, y);
                            }
                        }
                    }
                }
            }

            m_clipMap = clips.ToDictionary(x => BlendShapeKey.CreateFrom(x), x => x);
            m_valueMap = new Dictionary<BlendShapeKey, float>();
            foreach (var kv in m_clipMap)
            {
                foreach (var binding in kv.Value.Values)
                {

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
            foreach (var kv in m_setterMap)
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
            foreach (var kv in m_setterMap)
            {
                kv.Value.Apply();
            }
        }

        public void SetValue(BlendShapeKey key, float value, bool replace)
        {
            m_valueMap[key] = value;

            BlendShapeClip clip;
            if (!m_clipMap.TryGetValue(key, out clip))
            {
                return;
            }

            foreach (var binding in clip.Values)
            {
                BlendShapePathHandler handler;
                if (m_setterMap.TryGetValue(binding, out handler))
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

            // materialの更新
            foreach (var binding in clip.MaterialValues)
            {
                var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value;
                m_materialMap[binding.MaterialName].SetColor(binding.ValueName, propValue);
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

        public void RestoreMaterialValues(IEnumerable<BlendShapeClip> clips)
        {
            if (m_materialMap != null)
            {
                foreach (var x in clips)
                {
                    foreach (var y in x.MaterialValues)
                    {
                        // restore values
                        m_materialMap[y.MaterialName].SetColor(y.ValueName, y.BaseValue);
                    }
                }
            }
        }
    }
}

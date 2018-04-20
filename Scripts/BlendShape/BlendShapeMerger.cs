using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// ブレンドシェイプを蓄えてまとめて適用するクラス
    /// </summary>
    class BlendShapeMerger
    {
        Dictionary<BlendShapeKey, BlendShapeClip> m_clipMap;
        Dictionary<BlendShapeKey, float> m_valueMap;

        class AccumulatingSetter
        {
            public Action<float> Setter;
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
        Dictionary<BlendShapeBinding, AccumulatingSetter> m_setterMap;
        Dictionary<string, Material> m_materialMap;
        Dictionary<MaterialValueBinding, AccumulatingSetter> m_materialSetterMap;

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
            m_setterMap = new Dictionary<BlendShapeBinding, AccumulatingSetter>();
            m_materialSetterMap = new Dictionary<MaterialValueBinding, AccumulatingSetter>();
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
                            m_setterMap.Add(binding, new AccumulatingSetter
                            {
                                Setter = x =>
                                {
                                    target.SetBlendShapeWeight(binding.Index, x);
                                }
                            });
                        }
                        else
                        {
                            Debug.LogWarningFormat("SkinnedMeshRenderer: {0} not found", binding.RelativePath);
                        }
                    }
                }

                foreach(var binding in kv.Value.MaterialValues)
                {
                    if (!m_materialSetterMap.ContainsKey(binding))
                    {
                        Material target;
                        if(m_materialMap.TryGetValue(binding.MaterialName, out target))
                        {
                            m_materialSetterMap.Add(binding, new AccumulatingSetter
                            {
                                Setter = x =>
                                {
                                    //target.SetBlendShapeWeight(binding.Index, x);
                                    var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * x;
                                    target.SetColor(binding.ValueName, propValue);
                                }
                            });
                        }
                        else
                        {
                            Debug.LogWarningFormat("material: {0} not found", binding.MaterialName);
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
                AccumulatingSetter handler;
                if (m_setterMap.TryGetValue(binding, out handler))
                {
                    if (replace)
                    {
                        // 値置き換え
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
                AccumulatingSetter handler;
                if(m_materialSetterMap.TryGetValue(binding, out handler))
                {
                    if (replace)
                    {
                        // 値置き換え
                        handler.Apply(value);
                    }
                    else
                    {
                        // 積算
                        handler.AddValue(value);
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

        public void RestoreMaterialInitialValues(IEnumerable<BlendShapeClip> clips)
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

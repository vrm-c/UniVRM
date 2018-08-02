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

        Dictionary<string, Material> m_materialMap;

        Dictionary<BlendShapeBinding, float> m_blendShapeValueMap = new Dictionary<BlendShapeBinding, float>();
        Dictionary<BlendShapeBinding, Action<float>> m_blendShapeSetterMap = new Dictionary<BlendShapeBinding, Action<float>>();

        Dictionary<MaterialValueBinding, float> m_materialValueMap = new Dictionary<MaterialValueBinding, float>();
        Dictionary<MaterialValueBinding, Action<float>> m_materialSetterMap = new Dictionary<MaterialValueBinding, Action<float>>();

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
                    if (!m_blendShapeSetterMap.ContainsKey(binding))
                    {
                        var _target = root.Find(binding.RelativePath);
                        SkinnedMeshRenderer target = null;
                        if (_target != null)
                        {
                            target = _target.GetComponent<SkinnedMeshRenderer>();
                        }
                        if (target != null)
                        {
                            if (binding.Index >= 0 && binding.Index < target.sharedMesh.blendShapeCount)
                            {
                                m_blendShapeSetterMap.Add(binding, x =>
                                {
                                    target.SetBlendShapeWeight(binding.Index, x);
                                });
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid blendshape binding: {0}: {1}", target.name, binding);
                            }

                        }
                        else
                        {
                            Debug.LogWarningFormat("SkinnedMeshRenderer: {0} not found", binding.RelativePath);
                        }
                    }
                }

                foreach (var binding in kv.Value.MaterialValues)
                {
                    if (!m_materialSetterMap.ContainsKey(binding))
                    {
                        Material target;
                        if (m_materialMap.TryGetValue(binding.MaterialName, out target))
                        {
                            if (binding.ValueName.EndsWith("_ST_S"))
                            {
                                var valueName = binding.ValueName.Substring(0, binding.ValueName.Length - 2);
                                Action<float> setter = value =>
                                {
                                    var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value;
                                    var src = target.GetVector(valueName);
                                    src.x = propValue.x; // horizontal only
                                    src.z = propValue.z; // horizontal only
                                    target.SetVector(valueName, src);
                                };
                                m_materialSetterMap.Add(binding, setter);
                            }
                            else if(binding.ValueName.EndsWith("_ST_T"))
                            {
                                var valueName = binding.ValueName.Substring(0, binding.ValueName.Length - 2);
                                Action<float> setter = value =>
                                {
                                    var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value;
                                    var src = target.GetVector(valueName);
                                    src.y = propValue.y; // vertical only
                                    src.w = propValue.w; // vertical only
                                    target.SetVector(valueName, src);
                                };
                                m_materialSetterMap.Add(binding, setter);
                            }
                            else
                            {
                                Action<float> vec4Setter = x =>
                                {
                                    var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * x;
                                    target.SetColor(binding.ValueName, propValue);
                                };
                                m_materialSetterMap.Add(binding, vec4Setter);
                            }
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
            foreach (var kv in m_valueMap.ToArray())
            {
                SetValue(kv.Key, kv.Value, false);
            }
            Apply();
        }

        public void Apply()
        {
            foreach (var kv in m_blendShapeValueMap)
            {
                Action<float> setter;
                if (m_blendShapeSetterMap.TryGetValue(kv.Key, out setter))
                {
                    setter(kv.Value);
                }
            }
            m_blendShapeValueMap.Clear();

            foreach (var kv in m_materialValueMap)
            {
                Action<float> setter;
                if (m_materialSetterMap.TryGetValue(kv.Key, out setter))
                {
                    setter(kv.Value);
                }
            }
            m_materialValueMap.Clear();
        }

        public void SetValues(IEnumerable<KeyValuePair<BlendShapeKey, float>> values)
        {
            foreach (var kv in values)
            {
                SetValue(kv.Key, kv.Value, false);
            }
            Apply();
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
                if (replace)
                {
                    // 値置き換え
                    Action<float> setter;
                    if (m_blendShapeSetterMap.TryGetValue(binding, out setter))
                    {
                        setter(binding.Weight * value);
                    }
                }
                else
                {
                    // 積算
                    float acc;
                    if (m_blendShapeValueMap.TryGetValue(binding, out acc))
                    {
                        m_blendShapeValueMap[binding] = acc + binding.Weight * value;
                    }
                    else
                    {
                        m_blendShapeValueMap[binding] = binding.Weight * value;
                    }
                }
            }

            // materialの更新
            foreach (var binding in clip.MaterialValues)
            {
                if (replace)
                {
                    // 値置き換え
                    Action<float> setter;
                    if (m_materialSetterMap.TryGetValue(binding, out setter))
                    {
                        setter(value);
                    }
                }
                else
                {
                    // 積算
                    float acc;
                    if (m_materialValueMap.TryGetValue(binding, out acc))
                    {
                        m_materialValueMap[binding] = acc + value;
                    }
                    else
                    {
                        m_materialValueMap[binding] = value;
                    }
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

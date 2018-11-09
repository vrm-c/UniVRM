using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace VRM
{
    ///
    /// Base + (A.Target - Base) * A.Weight + (B.Target - Base) * B.Weight ...
    ///
    class MaterialValueBindingMerger
    {
        /// <summary>
        /// 名前とmaterialのマッピング
        /// </summary>
        Dictionary<string, Material> m_materialMap = new Dictionary<string, Material>();

        delegate void Setter(float value, bool firstValue);

        /// <summary>
        /// MaterialValueの適用値を蓄積する
        /// </summary>
        /// <typeparam name="MaterialValueBinding"></typeparam>
        /// <typeparam name="float"></typeparam>
        /// <returns></returns>
        Dictionary<MaterialValueBinding, float> m_materialValueMap = new Dictionary<MaterialValueBinding, float>();

        Dictionary<MaterialValueBinding, Setter> m_materialSetterMap = new Dictionary<MaterialValueBinding, Setter>();

        BlendShapeClip[] m_clips;

        public MaterialValueBindingMerger(Dictionary<BlendShapeKey, BlendShapeClip> clipMap, Transform root)
        {
            m_clips = clipMap.Values.ToArray();

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

            foreach (var kv in clipMap)
            {
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
                                Setter setter = (value, firstValue) =>
                                {
                                    var propValue = firstValue
                                        ? (binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value)
                                        : (target.GetVector(valueName) + (binding.TargetValue - binding.BaseValue) * value)
                                        ;
                                    var src = target.GetVector(valueName);
                                    src.x = propValue.x; // horizontal only
                                    src.z = propValue.z; // horizontal only
                                    target.SetVector(valueName, src);
                                };
                                m_materialSetterMap.Add(binding, setter);
                            }
                            else if (binding.ValueName.EndsWith("_ST_T"))
                            {
                                var valueName = binding.ValueName.Substring(0, binding.ValueName.Length - 2);
                                Setter setter = (value, firstValue) =>
                                {
                                    var propValue = firstValue
                                        ? (binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value)
                                        : (target.GetVector(valueName) + (binding.TargetValue - binding.BaseValue) * value)
                                        ;
                                    var src = target.GetVector(valueName);
                                    src.y = propValue.y; // vertical only
                                    src.w = propValue.w; // vertical only
                                    target.SetVector(valueName, src);
                                };
                                m_materialSetterMap.Add(binding, setter);
                            }
                            else
                            {
                                Setter vec4Setter = (value, firstValue) =>
                                {
                                    var propValue = firstValue
                                        ? (binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value)
                                        : (target.GetVector(binding.ValueName) + (binding.TargetValue - binding.BaseValue) * value)
                                        ;
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

        public void RestoreMaterialInitialValues(IEnumerable<BlendShapeClip> clips)
        {
            if (m_materialMap != null)
            {
                foreach (var x in clips)
                {
                    foreach (var y in x.MaterialValues)
                    {
                        // restore values
                        Material material;
                        if (m_materialMap.TryGetValue(y.MaterialName, out material))
                        {
                            var valueName = y.ValueName;
                            if (valueName.EndsWith("_ST_S")
                            || valueName.EndsWith("_ST_T"))
                            {
                                valueName = valueName.Substring(0, valueName.Length - 2);
                            }
                            material.SetColor(valueName, y.BaseValue);
                        }
                        else
                        {
                            Debug.LogWarningFormat("{0} not found", y.MaterialName);
                        }
                    }
                }
            }
        }

        public void ImmediatelySetValue(BlendShapeClip clip, float value)
        {
            foreach (var binding in clip.MaterialValues)
            {
                Setter setter;
                if (m_materialSetterMap.TryGetValue(binding, out setter))
                {
                    setter(value, true);
                }
            }
        }

        public void AccumulateValue(BlendShapeClip clip, float value)
        {
            foreach (var binding in clip.MaterialValues)
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

        public void Apply()
        {
            // clear
            RestoreMaterialInitialValues(m_clips);

            // (binding.Value-Base) * weight を足す
            foreach (var kv in m_materialValueMap)
            {
                Setter setter;
                if (m_materialSetterMap.TryGetValue(kv.Key, out setter))
                {
                    setter(kv.Value, false);
                }
            }
            m_materialValueMap.Clear();
        }
    }
}

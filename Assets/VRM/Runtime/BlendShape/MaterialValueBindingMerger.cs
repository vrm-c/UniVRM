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

        struct DictionaryKeyMaterialValueBindingComparer : IEqualityComparer<MaterialValueBinding>
        {
            public bool Equals(MaterialValueBinding x, MaterialValueBinding y)
            {
                return x.TargetValue == y.TargetValue && x.BaseValue == y.BaseValue && x.MaterialName == y.MaterialName && x.ValueName == y.ValueName;
            }

            public int GetHashCode(MaterialValueBinding obj)
            {
                return obj.GetHashCode();
            }
        }

        static DictionaryKeyMaterialValueBindingComparer comparer = new DictionaryKeyMaterialValueBindingComparer();

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
        Dictionary<MaterialValueBinding, float> m_materialValueMap = new Dictionary<MaterialValueBinding, float>(comparer);

        Dictionary<MaterialValueBinding, Setter> m_materialSetterMap = new Dictionary<MaterialValueBinding, Setter>(comparer);

        //BlendShapeClip[] m_clips;

        public MaterialValueBindingMerger(Dictionary<BlendShapeKey, BlendShapeClip> clipMap, Transform root)
        {
            //m_clips = clipMap.Values.ToArray();

            foreach (var x in root.Traverse())
            {
                if (x.TryGetComponent<Renderer>(out var renderer))
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
#if UNITY_EDITOR
                            // restore only material with asset
                            if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(material)))
                            {
                                material.SetColor(valueName, y.BaseValue);
                            }
#endif
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

        struct MaterialTarget : IEquatable<MaterialTarget>
        {
            public string MaterialName;
            public string ValueName;

            public bool Equals(MaterialTarget other)
            {
                return MaterialName == other.MaterialName
                    && ValueName == other.ValueName;
            }

            public override bool Equals(object obj)
            {
                if (obj is MaterialTarget)
                {
                    return Equals((MaterialTarget)obj);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                if (MaterialName == null || ValueName == null)
                {
                    return 0;
                }
                return MaterialName.GetHashCode() + ValueName.GetHashCode();
            }

            public static MaterialTarget Create(MaterialValueBinding binding)
            {
                return new MaterialTarget
                {
                    MaterialName = binding.MaterialName,
                    ValueName = binding.ValueName
                };
            }
        }

        HashSet<MaterialTarget> m_used = new HashSet<MaterialTarget>();

        public void Apply()
        {
            // clear
            //RestoreMaterialInitialValues(m_clips);
            m_used.Clear();

            // (binding.Value-Base) * weight を足す
            foreach (var kv in m_materialValueMap)
            {
                var key = MaterialTarget.Create(kv.Key);
                if (!m_used.Contains(key))
                {
                    // restore value
                    Material material;
                    if (m_materialMap.TryGetValue(key.MaterialName, out material))
                    {
                        var value = kv.Key.BaseValue;
                        var valueName = key.ValueName;
                        if (valueName.EndsWith("_ST_S"))
                        {
                            valueName = valueName.Substring(0, valueName.Length - 2);
                            var v = material.GetVector(valueName);
                            value.y = v.y;
                            value.w = v.w;
                        }
                        else if (valueName.EndsWith("_ST_T"))
                        {
                            valueName = valueName.Substring(0, valueName.Length - 2);
                            var v = material.GetVector(valueName);
                            value.x = v.x;
                            value.z = v.z;
                        }
                        material.SetColor(valueName, value);
                    }
                    m_used.Add(key);
                }

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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    ///
    /// A.Value * A.Weight + B.Value * B.Weight ...
    ///
    internal sealed class MorphTargetBindingMerger
    {
        class DictionaryKeyMorphTargetBindingComparer : IEqualityComparer<MorphTargetBinding>
        {
            public bool Equals(MorphTargetBinding x, MorphTargetBinding y)
            {
                return x.RelativePath == y.RelativePath
                && x.Index == y.Index;
            }

            public int GetHashCode(MorphTargetBinding obj)
            {
                return obj.RelativePath.GetHashCode() + obj.Index;
            }
        }

        private static DictionaryKeyMorphTargetBindingComparer comparer = new DictionaryKeyMorphTargetBindingComparer();

        /// <summary>
        /// MorphTargetBinding の適用値を蓄積する
        /// </summary>
        /// <typeparam name="MorphTargetBinding"></typeparam>
        /// <typeparam name="float"></typeparam>
        /// <returns></returns>
        Dictionary<MorphTargetBinding, float> m_morphTargetValueMap = new Dictionary<MorphTargetBinding, float>(comparer);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Dictionary<MorphTargetBinding, Action<float>> m_morphTargetSetterMap = new Dictionary<MorphTargetBinding, Action<float>>(comparer);

        public MorphTargetBindingMerger(Dictionary<ExpressionKey, VRM10Expression> clipMap, Transform root)
        {
            foreach (var kv in clipMap)
            {
                foreach (var binding in kv.Value.MorphTargetBindings)
                {
                    if (!m_morphTargetSetterMap.ContainsKey(binding))
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
                                m_morphTargetSetterMap.Add(binding, x =>
                                {
                                    if (target == null)
                                    {
                                        // recompile in editor ?
                                        return;
                                    }
                                    // VRM-1.0 weight is 0-1
                                    target.SetBlendShapeWeight(binding.Index, x * MorphTargetBinding.VRM_TO_UNITY);
                                });
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid morphTarget binding: {0}: {1}", target.name, binding.Index);
                            }

                        }
                        else
                        {
                            Debug.LogWarningFormat("SkinnedMeshRenderer: {0} not found", binding.RelativePath);
                        }
                    }
                }
            }
        }

        public void AccumulateValue(VRM10Expression clip, float value)
        {
            foreach (var binding in clip.MorphTargetBindings)
            {
                float acc;
                if (m_morphTargetValueMap.TryGetValue(binding, out acc))
                {
                    m_morphTargetValueMap[binding] = acc + binding.Weight * value;
                }
                else
                {
                    m_morphTargetValueMap[binding] = binding.Weight * value;
                }
            }
        }

        public void Apply()
        {
            foreach (var kv in m_morphTargetValueMap)
            {
                Action<float> setter;
                if (m_morphTargetSetterMap.TryGetValue(kv.Key, out setter))
                {
                    setter(kv.Value);
                }
            }
            m_morphTargetValueMap.Clear();
        }
    }
}

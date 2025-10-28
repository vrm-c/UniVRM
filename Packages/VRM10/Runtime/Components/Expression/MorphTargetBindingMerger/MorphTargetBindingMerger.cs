using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Optimized implementation of MorphTarget binding merger.
    ///
    /// Dictionary を使用すると、その GetEnumerator()(foreach) や get/set を 100,1000 オーダーで呼び出すことになる。
    /// するとモバイル環境ではかなりの定常負荷になってしまうため、その使用を避けて実装する。
    /// </summary>
    internal sealed class MorphTargetBindingMerger
    {
        private readonly ExpressionKey[] _keyOrder;
        private readonly Dictionary<ExpressionKey, int> _keyIndexReverseMap = new(ExpressionKey.Comparer);

        /// <summary>
        /// index access with [_keyOrder][*]
        /// </summary>
        private readonly RuntimeMorphTargetBinding[][] _bindings;
        private readonly MorphTargetIdentifier[] _morphTargetOrder;

        /// <summary>
        /// index access with [_morphTargetOrder]
        /// </summary>
        private readonly float?[] _accumulatedWeights;

        public MorphTargetBindingMerger(Dictionary<ExpressionKey, VRM10Expression> expressions, Transform modelRoot)
        {
            _keyOrder = expressions.Keys.ToArray();
            for (var keyIdx = 0; keyIdx < _keyOrder.Length; ++keyIdx)
            {
                _keyIndexReverseMap.Add(_keyOrder[keyIdx], keyIdx);
            }

            var morphTargetList = new List<MorphTargetIdentifier>();
            _bindings = new RuntimeMorphTargetBinding[_keyOrder.Length][];
            for (var keyIdx = 0; keyIdx < _keyOrder.Length; ++keyIdx)
            {
                var key = _keyOrder[keyIdx];
                var expression = expressions[key];

                var bindingsPerKey = new List<RuntimeMorphTargetBinding>();
                foreach (var rawBinding in expression.MorphTargetBindings)
                {
                    var morphTarget = MorphTargetIdentifier.Create(rawBinding, modelRoot);
                    if (!morphTarget.HasValue)
                    {
                        UniGLTFLogger.Warning($"Invalid {nameof(MorphTargetBinding)} found: {rawBinding}");
                        continue;
                    }
                    if (bindingsPerKey.FindIndex(x => morphTarget.Value.Equals(x.TargetIdentifier)) >= 0)
                    {
                        UniGLTFLogger.Warning($"Duplicate MorphTargetBinding found: {rawBinding}");
                        continue;
                    }

                    var morphTargetIdx = morphTargetList.FindIndex(x => morphTarget.Value.Equals(x));
                    if (morphTargetIdx < 0)
                    {
                        morphTargetIdx = morphTargetList.Count;
                        morphTargetList.Add(morphTarget.Value);
                    }

                    var bindingWeight = rawBinding.Weight * MorphTargetBinding.VRM_TO_UNITY;
                    bindingsPerKey.Add(new RuntimeMorphTargetBinding(morphTarget.Value, inputWeight =>
                    {
                        _accumulatedWeights[morphTargetIdx] = (_accumulatedWeights[morphTargetIdx] ?? 0f) + bindingWeight * inputWeight;
                    }));
                }
                _bindings[keyIdx] = bindingsPerKey.ToArray();
            }
            _morphTargetOrder = morphTargetList.ToArray();
            _accumulatedWeights = new float?[_morphTargetOrder.Length];
        }

        public void AccumulateValue(ExpressionKey key, float value)
        {
            if (!_keyIndexReverseMap.TryGetValue(key, out var idx)) return;

            foreach (var binding in _bindings[idx])
            {
                binding.WeightApplier(value);
            }
        }

        public void Apply()
        {
            for (var idx = 0; idx < _morphTargetOrder.Length; ++idx)
            {
                var morphTarget = _morphTargetOrder[idx];
                var weight = _accumulatedWeights[idx];
                if (!weight.HasValue) continue;
                if (morphTarget.TargetRenderer)
                {
                    morphTarget.TargetRenderer.SetBlendShapeWeight(morphTarget.TargetBlendShapeIndex, weight.Value);
                }
                _accumulatedWeights[idx] = null;
            }
        }
    }
}
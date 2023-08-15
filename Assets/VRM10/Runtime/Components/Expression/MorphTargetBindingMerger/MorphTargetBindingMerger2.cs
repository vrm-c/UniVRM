using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    internal sealed class MorphTargetBindingMerger2
    {
        private readonly Dictionary<ExpressionKey, Dictionary<RuntimeMorphTargetBinding, float>> _weights = new(ExpressionKey.Comparer);
        private readonly Dictionary<RuntimeMorphTargetBinding, (bool, float)> _accumulatedWeights = new(RuntimeMorphTargetBinding.Comparer);
        private readonly RuntimeMorphTargetBinding[] _targetIds;

        public MorphTargetBindingMerger2(Dictionary<ExpressionKey, VRM10Expression> expressions, Transform modelRoot)
        {
            foreach (var (expressionKey, expression) in expressions)
            {
                _weights.Add(expressionKey, new Dictionary<RuntimeMorphTargetBinding, float>(RuntimeMorphTargetBinding.Comparer));
                foreach (var binding in expression.MorphTargetBindings)
                {
                    var id = RuntimeMorphTargetBinding.Create(binding, modelRoot);
                    if (id == null)
                    {
                        Debug.LogWarning($"Invalid MorphTargetBinding found: {binding}");
                        continue;
                    }
                    if (_weights[expressionKey].ContainsKey(id.Value))
                    {
                        Debug.LogWarning($"Duplicate MorphTargetBinding found: {binding}");
                        continue;
                    }
                    _weights[expressionKey].Add(id.Value, binding.Weight * MorphTargetBinding.VRM_TO_UNITY);
                    _accumulatedWeights.TryAdd(id.Value, (false, 0f));
                }
            }
            _targetIds = _accumulatedWeights.Keys.ToArray();
        }

        public void AccumulateValue(ExpressionKey key, float value)
        {
            if (_weights.TryGetValue(key, out var weightsPerBinding))
            {
                foreach (var (id, weight) in weightsPerBinding)
                {
                    var (_, currentWeight) = _accumulatedWeights[id];
                    _accumulatedWeights[id] = (true, currentWeight + weight * value);
                }
            }
        }

        public void Apply()
        {
            foreach (var (id, (isAccumulated, totalWeight)) in _accumulatedWeights)
            {
                if (isAccumulated)
                {
                    if (id.TargetRenderer == null) continue;

                    id.TargetRenderer.SetBlendShapeWeight(id.TargetBlendShapeIndex, totalWeight);
                }
            }

            // NOTE: Reset
            foreach (var id in _targetIds)
            {
                _accumulatedWeights[id] = (false, 0f);
            }
        }
    }
}
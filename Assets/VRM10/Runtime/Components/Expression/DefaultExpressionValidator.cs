using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public sealed class DefaultExpressionValidator : IExpressionValidator
    {
        private readonly ExpressionKey[] _keys;
        private readonly Dictionary<ExpressionKey, VRM10Expression> _expressions;

        private DefaultExpressionValidator(VRM10ObjectExpression expressionAvatar)
        {
            _keys = expressionAvatar.Clips
                .Select(x => expressionAvatar.CreateKey(x.Clip))
                .ToArray();
            _expressions = expressionAvatar.Clips.ToDictionary(
                x => expressionAvatar.CreateKey(x.Clip),
                x => x.Clip,
                ExpressionKey.Comparer
            );
        }

        public void Validate(IReadOnlyDictionary<ExpressionKey, float> inputWeights, IDictionary<ExpressionKey, float> actualWeights,
            LookAtEyeDirection inputEyeDirection, out LookAtEyeDirection actualEyeDirection,
            out float blinkOverrideRate, out float lookAtOverrideRate, out float mouthOverrideRate)
        {
            // override rate
            blinkOverrideRate = 0f;
            lookAtOverrideRate = 0f;
            mouthOverrideRate = 0f;

            // 1. Set weights and Accumulate override rates.
            foreach (var key in _keys)
            {
                // Get expression.
                if (!_expressions.ContainsKey(key)) continue;
                var expression = _expressions[key];

                // Get weight with evaluation binary flag.
                var weight = expression.IsBinary ? Mathf.Round(inputWeights[key]) : inputWeights[key];

                // Set weight.
                actualWeights[key] = weight;

                // Override rate without targeting myself.
                if (!key.IsBlink)
                {
                    blinkOverrideRate += GetOverrideRate(expression.OverrideBlink, weight);
                }
                if (!key.IsLookAt)
                {
                    lookAtOverrideRate += GetOverrideRate(expression.OverrideLookAt, weight);
                }
                if (!key.IsMouth)
                {
                    mouthOverrideRate += GetOverrideRate(expression.OverrideMouth, weight);
                }
            }

            // 2. Saturate rate.
            blinkOverrideRate = Mathf.Clamp01(blinkOverrideRate);
            lookAtOverrideRate = Mathf.Clamp01(lookAtOverrideRate);
            mouthOverrideRate = Mathf.Clamp01(mouthOverrideRate);

            var blinkMultiplier = 1f - blinkOverrideRate;
            var lookAtMultiplier = 1f - lookAtOverrideRate;
            var mouthMultiplier = 1f - mouthOverrideRate;

            // 3. Set procedural key's weights.
            foreach (var key in _keys)
            {
                if (key.IsBlink)
                {
                    actualWeights[key] = actualWeights[key] * blinkMultiplier;
                }
                else if (key.IsLookAt)
                {
                    actualWeights[key] = actualWeights[key] * lookAtMultiplier;
                }
                else if (key.IsMouth)
                {
                    actualWeights[key] = actualWeights[key] * mouthMultiplier;
                }
            }

            // 4. eye direction
            actualEyeDirection = LookAtEyeDirection.Multiply(inputEyeDirection, 1f - lookAtOverrideRate);
        }

        private float GetOverrideRate(ExpressionOverrideType type, float weight)
        {
            switch (type)
            {
                case ExpressionOverrideType.none:
                    return 0f;
                case ExpressionOverrideType.block:
                    return weight > 0f ? 1f : 0f;
                case ExpressionOverrideType.blend:
                    return weight;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public sealed class Factory : IExpressionValidatorFactory
        {
            public IExpressionValidator Create(VRM10ObjectExpression expressionAvatar)
            {
                return new DefaultExpressionValidator(expressionAvatar);
            }
        }
    }
}
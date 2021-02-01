using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public sealed class DefaultExpressionValidator : IExpressionValidator
    {
        private readonly Dictionary<ExpressionKey, VRM10Expression> _expressions;
        
        private DefaultExpressionValidator(VRM10ExpressionAvatar expressionAvatar)
        {
            _expressions = expressionAvatar.Clips.ToDictionary(ExpressionKey.CreateFromClip, x => x);
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
            foreach (var (key, weight) in inputWeights)
            {
                if (!actualWeights.ContainsKey(key))
                {
                    actualWeights.Add(key, weight);
                }
                // Set weight.
                actualWeights[key] = weight;
                
                // Get expression.
                if (!_expressions.ContainsKey(key)) continue;
                var expression = _expressions[key];

                // Override rate without targeting myself.
                if (!key.IsBlink)
                {
                    blinkOverrideRate = Mathf.Max(blinkOverrideRate, GetOverrideRate(expression.OverrideBlink, weight));
                }
                if (!key.IsLookAt)
                {
                    lookAtOverrideRate = Mathf.Max(lookAtOverrideRate, GetOverrideRate(expression.OverrideLookAt, weight));
                }
                if (!key.IsMouth)
                {
                    mouthOverrideRate = Mathf.Max(mouthOverrideRate, GetOverrideRate(expression.OverrideMouth, weight));
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
            foreach (var (key, weight) in inputWeights)
            {
                if (key.IsBlink)
                {
                    actualWeights[key] = weight * blinkMultiplier;
                }
                else if (key.IsLookAt)
                {
                    actualWeights[key] = weight * lookAtMultiplier;
                }
                else if (key.IsMouth)
                {
                    actualWeights[key] = weight * mouthMultiplier;
                }
            }
            
            // 4. eye direction
            actualEyeDirection = LookAtEyeDirection.Multiply(inputEyeDirection, 1f - lookAtOverrideRate);
        }

        private float GetOverrideRate(ExpressionOverrideType type, float weight)
        {
            switch (type)
            {
                case ExpressionOverrideType.None:
                    return 0f;
                case ExpressionOverrideType.Block:
                    return weight > 0f ? 1f : 0f;
                case ExpressionOverrideType.Blend:
                    return weight;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public sealed class Factory : IExpressionValidatorFactory
        {
            public IExpressionValidator Create(VRM10ExpressionAvatar expressionAvatar)
            {
                return new DefaultExpressionValidator(expressionAvatar);
            }
        }
    }
}
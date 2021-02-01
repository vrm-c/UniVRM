using System.Collections.Generic;
using VrmLib;

namespace UniVRM10
{
    public sealed class DefaultExpressionValidator : IExpressionValidator
    {
        private DefaultExpressionValidator(VRM10ExpressionAvatar expressionAvatar)
        {
            
        }
        
        public void Validate(IReadOnlyDictionary<ExpressionKey, float> inputWeights, IDictionary<ExpressionKey, float> actualWeights,
            LookAtEyeDirection inputEyeDirection, out LookAtEyeDirection actualEyeDirection,
            out float blinkOverrideRate, out float lookAtOverrideRate, out float mouthOverrideRate)
        {
            // weights
            foreach (var (key, weight) in inputWeights)
            {
                if (!actualWeights.ContainsKey(key))
                {
                    actualWeights.Add(key, weight);
                }

                actualWeights[key] = weight;
            }
            
            // eye direction
            actualEyeDirection = inputEyeDirection;

            // override rate
            blinkOverrideRate = 0f;
            lookAtOverrideRate = 0f;
            mouthOverrideRate = 0f;
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
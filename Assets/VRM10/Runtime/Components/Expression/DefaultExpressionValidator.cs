using System.Collections.Generic;
using VrmLib;

namespace UniVRM10
{
    public sealed class DefaultExpressionValidator : IExpressionValidator
    {
        private DefaultExpressionValidator(VRM10ExpressionAvatar expressionAvatar)
        {
            
        }
        
        public void Validate(IReadOnlyDictionary<ExpressionKey, float> inputWeights, IDictionary<ExpressionKey, float> actualWeights, out float blinkNullifyWeight,
            out float lookAtNullifyWeight, out float mouthNullifyWeight)
        {
            foreach (var (key, weight) in inputWeights)
            {
                if (!actualWeights.ContainsKey(key))
                {
                    actualWeights.Add(key, weight);
                }

                actualWeights[key] = weight;
            }

            blinkNullifyWeight = 0f;
            lookAtNullifyWeight = 0f;
            mouthNullifyWeight = 0f;
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
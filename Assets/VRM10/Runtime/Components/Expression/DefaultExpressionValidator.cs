using System.Collections.Generic;
using VrmLib;

namespace UniVRM10
{
    public sealed class DefaultExpressionValidator : IExpressionValidator
    {
        private DefaultExpressionValidator(VRM10ExpressionAvatar expressionAvatar)
        {
            
        }
        
        public void Validate(IReadOnlyDictionary<ExpressionKey, float> inputWeights, IDictionary<ExpressionKey, float> actualWeights)
        {
            foreach (var (key, weight) in inputWeights)
            {
                if (!actualWeights.ContainsKey(key))
                {
                    actualWeights.Add(key, weight);
                }

                actualWeights[key] = weight;
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
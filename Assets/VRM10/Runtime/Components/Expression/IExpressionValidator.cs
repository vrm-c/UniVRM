using System.Collections.Generic;

namespace UniVRM10
{
    /// <summary>
    /// Validate Expression constraints (ex. overrideBlink)
    /// </summary>
    public interface IExpressionValidator
    {
        /// <summary>
        /// Validate input weights with Expression constraints.
        /// </summary>
        void Validate(IReadOnlyDictionary<ExpressionKey, float> inputWeights, IDictionary<ExpressionKey, float> actualWeights);
    }
}
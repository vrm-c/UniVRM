using System.Collections.Generic;

namespace UniVRM10
{
    /// <summary>
    /// Receive LooAtEyeDirection, and Apply to bone transforms.
    /// </summary>
    internal interface ILookAtEyeDirectionApplicable
    {
        void Apply(LookAtEyeDirection eyeDirection, Dictionary<ExpressionKey, float> actualWeights);
        void Restore();
    }
}
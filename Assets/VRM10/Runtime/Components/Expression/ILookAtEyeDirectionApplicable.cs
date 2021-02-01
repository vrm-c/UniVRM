using System.Collections.Generic;

namespace UniVRM10
{
    /// <summary>
    /// Receive LooAtEyeDirection, and Apply to bone transforms.
    /// </summary>
    internal interface ILookAtEyeDirectionApplicable
    {
        IEnumerable<KeyValuePair<ExpressionKey, float>> Apply(LookAtEyeDirection eyeDirection);
    }
}
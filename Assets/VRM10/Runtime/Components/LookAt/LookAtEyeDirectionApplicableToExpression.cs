using System.Collections.Generic;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    internal sealed class LookAtEyeDirectionApplicableToExpression : ILookAtEyeDirectionApplicable
    {
        private readonly CurveMapper _horizontalOuter;
        private readonly CurveMapper _horizontalInner;
        private readonly CurveMapper _verticalDown;
        private readonly CurveMapper _verticalUp;
        
        private readonly ExpressionKey _lookRightKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookRight);
        private readonly ExpressionKey _lookLeftKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookLeft);
        private readonly ExpressionKey _lookUpKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookUp);
        private readonly ExpressionKey _lookDownKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookDown);
        
        public LookAtEyeDirectionApplicableToExpression(
            CurveMapper horizontalOuter, CurveMapper horizontalInner, CurveMapper verticalDown, CurveMapper verticalUp)
        {
            _horizontalOuter = horizontalOuter;
            _horizontalInner = horizontalInner;
            _verticalDown = verticalDown;
            _verticalUp = verticalUp;
        }

        public IEnumerable<KeyValuePair<ExpressionKey, float>> Apply(LookAtEyeDirection eyeDirection)
        {
            var yaw = eyeDirection.LeftYaw;
            var pitch = eyeDirection.LeftPitch;
            
            if (yaw < 0)
            {
                // Left
                yield return new KeyValuePair<ExpressionKey, float>(_lookRightKey, 0);
                yield return new KeyValuePair<ExpressionKey, float>(_lookLeftKey, Mathf.Clamp(_horizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f));
            }
            else
            {
                // Right
                yield return new KeyValuePair<ExpressionKey, float>(_lookRightKey, Mathf.Clamp(_horizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f));
                yield return new KeyValuePair<ExpressionKey, float>(_lookLeftKey, 0);
            }

            if (pitch < 0)
            {
                // Down
                yield return new KeyValuePair<ExpressionKey, float>(_lookUpKey, 0);
                yield return new KeyValuePair<ExpressionKey, float>(_lookDownKey, Mathf.Clamp(_verticalDown.Map(Mathf.Abs(pitch)), 0, 1.0f));
            }
            else
            {
                // Up
                yield return new KeyValuePair<ExpressionKey, float>(_lookUpKey, Mathf.Clamp(_verticalUp.Map(Mathf.Abs(pitch)), 0, 1.0f));
                yield return new KeyValuePair<ExpressionKey, float>(_lookDownKey, 0);
            }
        }
    }
}
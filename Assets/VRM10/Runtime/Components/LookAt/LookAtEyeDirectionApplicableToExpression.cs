using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;

namespace UniVRM10
{
    internal sealed class LookAtEyeDirectionApplicableToExpression : ILookAtEyeDirectionApplicable
    {
        private readonly CurveMapper _horizontalOuter;
        private readonly CurveMapper _horizontalInner;
        private readonly CurveMapper _verticalDown;
        private readonly CurveMapper _verticalUp;

        private readonly ExpressionKey _lookRightKey = ExpressionKey.CreateFromPreset(ExpressionPreset.lookRight);
        private readonly ExpressionKey _lookLeftKey = ExpressionKey.CreateFromPreset(ExpressionPreset.lookLeft);
        private readonly ExpressionKey _lookUpKey = ExpressionKey.CreateFromPreset(ExpressionPreset.lookUp);
        private readonly ExpressionKey _lookDownKey = ExpressionKey.CreateFromPreset(ExpressionPreset.lookDown);

        public LookAtEyeDirectionApplicableToExpression(
            CurveMapper horizontalOuter, CurveMapper horizontalInner, CurveMapper verticalDown, CurveMapper verticalUp)
        {
            _horizontalOuter = horizontalOuter;
            _horizontalInner = horizontalInner;
            _verticalDown = verticalDown;
            _verticalUp = verticalUp;
        }

        public void Apply(LookAtEyeDirection eyeDirection, Dictionary<ExpressionKey, float> actualWeights)
        {
            var yaw = eyeDirection.LeftYaw;
            var pitch = eyeDirection.LeftPitch;

            if (yaw < 0)
            {
                // Left
                actualWeights[_lookRightKey] = 0;
                actualWeights[_lookLeftKey] = Mathf.Clamp(_horizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f);
            }
            else
            {
                // Right
                actualWeights[_lookRightKey] = Mathf.Clamp(_horizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f);
                actualWeights[_lookLeftKey] = 0;
            }

            if (pitch < 0)
            {
                // Down
                actualWeights[_lookUpKey] = 0;
                actualWeights[_lookDownKey] = Mathf.Clamp(_verticalDown.Map(Mathf.Abs(pitch)), 0, 1.0f);
            }
            else
            {
                // Up
                actualWeights[_lookUpKey] = Mathf.Clamp(_verticalUp.Map(Mathf.Abs(pitch)), 0, 1.0f);
                actualWeights[_lookDownKey] = 0;
            }
        }

        public void Restore()
        {
        }
    }
}
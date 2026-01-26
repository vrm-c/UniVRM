using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;


namespace UniVRM10
{
    internal sealed class BoneTransformBindingMerger
    {
        Dictionary<ExpressionKey, float> _acum = new();
        // Vrm10BoneTransformExpression[] _expressions;
        Transform _root;

        public BoneTransformBindingMerger(Transform root)
        {
            // _expressions = root.GetComponentsInChildren<Vrm10BoneTransformExpression>();
            _root = root;
        }

        public void AccumulateValue(ExpressionKey key, float value)
        {
            _acum[key] = value;
        }

        public void Apply(IReadOnlyDictionary<Transform, TransformState> initPose)
        {
            foreach (var expression in _root.GetComponentsInChildren<Vrm10BoneTransformExpression>())
            {
                if (initPose.TryGetValue(expression.transform, out var init))
                {
                    var weight = _acum.GetValueOrDefault(expression.Expression.ExpressionKey, 0);
                    expression.transform.SetLocalPositionAndRotation(
                        init.LocalPosition + expression.Expression.Translation * weight,
                        Quaternion.Slerp(init.LocalRotation, init.LocalRotation * expression.Expression.Rotation, weight)
                        );
                }
            }
        }
    }
}
using System.Collections.Generic;
using UniGLTF;
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

        public void Apply()
        {
            foreach (var expression in _root.GetComponentsInChildren<Vrm10BoneTransformExpression>())
            {
                var transform = expression.transform;
                var tr = expression.Expression.Transformation;
                var m = Matrix4x4.TRS(tr.Translation, tr.Rotation, Vector3.one);
                var w = m * transform.localToWorldMatrix;
                var (t, r, s) = w.Decompose();
                transform.SetPositionAndRotation(t, r);
            }
        }
    }
}
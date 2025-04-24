using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/schema/VRMC_node_constraint.rollConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10RollConstraint : MonoBehaviour, IVrm10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 1.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public RollAxis RollAxis;
        Vector3 GetRollVector()
        {
            switch (RollAxis)
            {
                case RollAxis.X: return Vector3.right;
                case RollAxis.Y: return Vector3.up;
                case RollAxis.Z: return Vector3.forward;
                default: throw new NotImplementedException();
            }
        }

        Transform IVrm10Constraint.ConstraintTarget => transform;

        Transform IVrm10Constraint.ConstraintSource => Source;

        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/README.ja.md#example-of-implementation
        /// 
        /// deltaSrcQuat = srcRestQuat.inverse * srcQuat
        /// deltaSrcQuatInParent = srcRestQuat * deltaSrcQuat * srcRestQuat.inverse // source to parent
        /// deltaSrcQuatInDst = dstRestQuat.inverse * deltaSrcQuatInWorld * dstRestQuat // parent to destination
        /// 
        /// toVec = rollAxis.applyQuaternion( deltaSrcQuatInDst )
        /// fromToQuat = Quaternion.fromToRotation( rollAxis, toVec )
        /// 
        /// targetQuat = Quaternion.slerp(
        ///   dstRestQuat,
        ///   dstRestQuat * fromToQuat.inverse * deltaSrcQuatInDst,
        ///   weight
        /// )
        /// </summary>
        void IVrm10Constraint.Process(in TransformState dstInitState, in TransformState srcInitState)
        {
            var deltaSrcQuat = Quaternion.Inverse(srcInitState.LocalRotation) * Source.localRotation;
            var deltaSrcQuatInParent = srcInitState.LocalRotation * deltaSrcQuat * Quaternion.Inverse(srcInitState.LocalRotation); // source to parent
            var deltaSrcQuatInDst = Quaternion.Inverse(dstInitState.LocalRotation) * deltaSrcQuatInParent * dstInitState.LocalRotation; // parent to destination

            var rollAxis = GetRollVector();
            var toVec = deltaSrcQuatInDst * rollAxis;
            var fromToQuat = Quaternion.FromToRotation(rollAxis, toVec);

            transform.localRotation = Quaternion.SlerpUnclamped(
              dstInitState.LocalRotation,
              dstInitState.LocalRotation * Quaternion.Inverse(fromToQuat) * deltaSrcQuatInDst,
              Weight
            );
        }
    }
}

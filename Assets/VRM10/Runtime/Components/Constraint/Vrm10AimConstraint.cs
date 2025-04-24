using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/schema/VRMC_node_constraint.aimConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10AimConstraint : MonoBehaviour, IVrm10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 1.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public AimAxis AimAxis;

        Vector3 GetAxisVector()
        {
            switch (AimAxis)
            {
                case AimAxis.PositiveX: return Vector3.right;
                case AimAxis.NegativeX: return Vector3.left;
                case AimAxis.PositiveY: return Vector3.up;
                case AimAxis.NegativeY: return Vector3.down;
                case AimAxis.PositiveZ: return Vector3.forward;
                case AimAxis.NegativeZ: return Vector3.back;
                default: throw new NotImplementedException();
            }
        }

        Transform IVrm10Constraint.ConstraintTarget => transform;

        Transform IVrm10Constraint.ConstraintSource => Source;

        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/README.ja.md#example-of-implementation-1
        /// 
        /// fromVec = aimAxis.applyQuaternion( dstParentWorldQuat * dstRestQuat )
        /// toVec = ( srcWorldPos - dstWorldPos ).normalized
        /// fromToQuat = Quaternion.fromToRotation( fromVec, toVec )
        /// targetQuat = Quaternion.slerp(
        ///   dstRestQuat,
        ///   dstParentWorldQuat.inverse * fromToQuat * dstParentWorldQuat * dstRestQuat,
        ///   weight
        /// )
        /// </summary>
        void IVrm10Constraint.Process(in TransformState dstInitState, in TransformState srcInitState)
        {
            // world coords
            var dstParentWorldQuat = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            var fromVec = (dstParentWorldQuat * dstInitState.LocalRotation) * GetAxisVector();
            var toVec = (Source.position - transform.position).normalized;
            var fromToQuat = Quaternion.FromToRotation(fromVec, toVec);

            transform.localRotation = Quaternion.SlerpUnclamped(
                dstInitState.LocalRotation,
                Quaternion.Inverse(dstParentWorldQuat) * fromToQuat * dstParentWorldQuat * dstInitState.LocalRotation,
                Weight
            );
        }

        public void OnDrawGizmosSelected()
        {
            if (Source == null)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, Source.position);
            Gizmos.DrawSphere(Source.position, 0.01f);

            Gizmos.matrix = transform.localToWorldMatrix;
            var len = 0.1f;
            switch (AimAxis)
            {
                case AimAxis.PositiveX:
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(Vector3.zero, Vector3.right * len);
                    break;
                case AimAxis.NegativeX:
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(Vector3.zero, Vector3.left * len);
                    break;
            }
        }
    }
}

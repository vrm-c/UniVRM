using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_draft/schema/VRMC_node_constraint.aimConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10AimConstraint : Vrm10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 10.0f)]
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

        Quaternion _dstRestLocalQuat;

        void Awake()
        {
            if (Source == null)
            {
                this.enabled = false;
                return;
            }

            _dstRestLocalQuat = transform.localRotation;
        }
        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_draft/README.ja.md#example-of-implementation-1
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
        public override void Process()
        {
            if (Source == null) return;

            // world coords
            var dstParentWorldQuat = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            var fromVec = (dstParentWorldQuat * _dstRestLocalQuat) * GetAxisVector();
            var toVec = (Source.position - transform.position).normalized;
            var fromToQuat = Quaternion.FromToRotation(fromVec, toVec);

            transform.rotation = Quaternion.SlerpUnclamped(
                _dstRestLocalQuat,
                Quaternion.Inverse(dstParentWorldQuat) * fromToQuat * dstParentWorldQuat * _dstRestLocalQuat,
                Weight
            );
        }
    }
}

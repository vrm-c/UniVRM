using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/schema/VRMC_node_constraint.rollConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10RollConstraint : MonoBehaviour, IVrm10Constraint
    {
        public GameObject ConstraintTarget => gameObject;

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

        Quaternion _srcRestLocalQuat;
        Quaternion _dstRestLocalQuat;

        void Start()
        {
            if (Source == null)
            {
                this.enabled = false;
                return;
            }

            _srcRestLocalQuat = Source.localRotation;
            _dstRestLocalQuat = transform.localRotation;
        }
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
        void IVrm10Constraint.Process()
        {
            if (Source == null) return;

            var deltaSrcQuat = Quaternion.Inverse(_srcRestLocalQuat) * Source.localRotation;
            var deltaSrcQuatInParent = _srcRestLocalQuat * deltaSrcQuat * Quaternion.Inverse(_srcRestLocalQuat); // source to parent
            var deltaSrcQuatInDst = Quaternion.Inverse(_dstRestLocalQuat) * deltaSrcQuatInParent * _dstRestLocalQuat; // parent to destination

            var rollAxis = GetRollVector();
            var toVec = deltaSrcQuatInDst * rollAxis;
            var fromToQuat = Quaternion.FromToRotation(rollAxis, toVec);

            transform.localRotation = Quaternion.SlerpUnclamped(
              _dstRestLocalQuat,
              _dstRestLocalQuat * Quaternion.Inverse(fromToQuat) * deltaSrcQuatInDst,
              Weight
            );
        }
    }
}

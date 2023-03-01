using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/schema/VRMC_node_constraint.rotationConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10RotationConstraint : MonoBehaviour, IVrm10Constraint
    {
        public GameObject ConstraintTarget => gameObject;

        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 1.0f)]
        public float Weight = 1.0f;

        Quaternion _srcRestLocalQuatInverse;
        Quaternion _dstRestLocalQuat;

        void Start()
        {
            if (Source == null)
            {
                this.enabled = false;
                return;
            }

            _srcRestLocalQuatInverse = Quaternion.Inverse(Source.localRotation);
            _dstRestLocalQuat = transform.localRotation;
        }

        /// <summary>
        /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/README.ja.md#example-of-implementation-2
        /// 
        /// srcDeltaQuat = srcRestQuat.inverse * srcQuat
        /// targetQuat = Quaternion.slerp(
        ///   dstRestQuat,
        ///   dstRestQuat * srcDeltaQuat,
        ///   weight
        /// )
        /// </summary>
        void IVrm10Constraint.Process()
        {
            if (Source == null) return;

            // local coords
            var srcDeltaLocalQuat = _srcRestLocalQuatInverse * Source.localRotation;
            transform.localRotation = Quaternion.SlerpUnclamped(_dstRestLocalQuat, _dstRestLocalQuat * srcDeltaLocalQuat, Weight);
        }
    }
}

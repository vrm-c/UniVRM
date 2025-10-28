using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_node_constraint-1.0_beta/schema/VRMC_node_constraint.rotationConstraint.schema.json
    /// </summary>
    [DisallowMultipleComponent]
    public class Vrm10RotationConstraint : MonoBehaviour, IVrm10Constraint
    {
        Transform IVrm10Constraint.ConstraintTarget => transform;

        Transform IVrm10Constraint.ConstraintSource => Source;

        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 1.0f)]
        public float Weight = 1.0f;

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
        void IVrm10Constraint.Process(in TransformState dstInitState, in TransformState srcInitState)
        {
            // local coords
            var srcDeltaLocalQuat = Quaternion.Inverse(srcInitState.LocalRotation) * Source.localRotation;
            transform.localRotation = Quaternion.SlerpUnclamped(dstInitState.LocalRotation, dstInitState.LocalRotation * srcDeltaLocalQuat, Weight);
        }
    }
}

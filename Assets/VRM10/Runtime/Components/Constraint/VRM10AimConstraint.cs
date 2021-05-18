using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;


namespace UniVRM10
{
    [DisallowMultipleComponent]
    public class VRM10AimConstraint : VRM10Constraint
    {
        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        [Header("Source")]
        [SerializeField]
        public Transform Source = default;

        [Header("Destination")]
        [SerializeField]
        ObjectSpace m_destinationCoordinate = default;

        [SerializeField]
        public Quaternion DestinationOffset = Quaternion.identity;

        public ConstraintSource m_src;

        public Quaternion Delta;

        /// <summary>
        /// TargetのUpdateよりも先か後かはその時による。
        /// 厳密に制御するのは無理。
        /// </summary>
        public override void Process()
        {
            if (Source == null)
            {
                enabled = false;
                return;
            }

            if (m_src == null)
            {
                m_src = new ConstraintSource(Source, ModelRoot);
            }

            var zAxis = (Source.position - transform.position).normalized;
            var xAxis = Vector3.Cross(Vector3.up, zAxis);
            var yAxis = Vector3.Cross(zAxis, xAxis);
            var m = new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(0, 0, 0, 1));
            var parent = TR.FromParent(transform);
            Delta = Quaternion.Inverse(parent.Rotation * m_src.LocalInitial.Rotation * DestinationOffset) * m.rotation;
            transform.rotation = parent.Rotation * m_src.LocalInitial.Rotation * Delta;
        }
    }
}

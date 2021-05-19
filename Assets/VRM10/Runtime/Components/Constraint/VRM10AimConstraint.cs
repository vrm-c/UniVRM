using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;


namespace UniVRM10
{
    [DisallowMultipleComponent]
    public class VRM10AimConstraint : VRM10Constraint
    {
        [Header("Source")]
        [SerializeField]
        Transform m_source = default;
        public override Transform Source
        {
            get => m_source;
            set => m_source = value;
        }

        [Header("Destination")]
        [SerializeField]
        ObjectSpace m_destinationCoordinate = default;

        [SerializeField]
        public Quaternion DestinationOffset = Quaternion.identity;

        Quaternion m_delta;
        public Quaternion Delta => m_delta;

        public Vector3 UpVector
        {
            get
            {
                switch (m_destinationCoordinate)
                {
                    case ObjectSpace.model: return ModelRoot.up;

                    case ObjectSpace.local:
                        {
                            if (m_src == null)
                            {
                                return transform.up;
                            }

                            return (TR.FromParent(transform).Rotation * m_dst.LocalInitial.Rotation) * Vector3.up;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override void OnProcess()
        {
            var zAxis = (Source.position - transform.position).normalized;
            var xAxis = Vector3.Cross(UpVector, zAxis);
            var yAxis = Vector3.Cross(zAxis, xAxis);
            var m = new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(0, 0, 0, 1));
            var parent = TR.FromParent(transform);
            m_delta = Quaternion.Inverse(parent.Rotation * m_src.LocalInitial.Rotation * DestinationOffset) * m.rotation;
            transform.rotation = parent.Rotation * m_src.LocalInitial.Rotation * Delta;
        }
    }
}

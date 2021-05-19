using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10RotationPositionConstraintBase
    {
        public override Vector3 Delta => FreezeAxes.Freeze(Quaternion.Slerp(Quaternion.identity, m_delta.Rotation, Weight).eulerAngles);

        public override TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }
            return coords * new TR(m_delta.Rotation);
        }

        public override TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }
            return coords * new TR(m_delta.Rotation);
        }

        protected override void ApplyDelta()
        {
            switch (DestinationCoordinate)
            {
                case ObjectSpace.local:
                    m_dst.ApplyLocal(m_dst.LocalInitial * new TR(DestinationOffset) * new TR(Quaternion.Euler(Delta)));
                    break;

                case ObjectSpace.model:
                    m_dst.ApplyModel(DestinationInitialCoords(ObjectSpace.model) * new TR(DestinationOffset) * new TR(Quaternion.Euler(Delta)));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

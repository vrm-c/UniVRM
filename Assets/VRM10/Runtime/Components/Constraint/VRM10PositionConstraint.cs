using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 対象の初期位置と現在位置の差分(delta)を、自身の初期位置に対してWeightを乗算して加算する。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10PositionConstraint : VRM10RotationPositionConstraintBase
    {
        public override Vector3 Delta => FreezeAxes.Freeze(m_delta.Translation) * Weight;

        public override TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }
            return coords * new TR(Delta);
        }

        public override TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }
            return coords * new TR(Delta);
        }

        protected override void ApplyDelta()
        {
            switch (DestinationCoordinate)
            {
                case ObjectSpace.local:
                    m_dst.ApplyLocal(DestinationInitialCoords(ObjectSpace.local) * new TR(DestinationOffset) * new TR(Delta));
                    break;

                case ObjectSpace.model:
                    m_dst.ApplyModel(DestinationInitialCoords(ObjectSpace.model) * new TR(DestinationOffset) * new TR(Delta));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

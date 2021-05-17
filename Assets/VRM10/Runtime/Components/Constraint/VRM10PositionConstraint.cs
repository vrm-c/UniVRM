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
        public override Vector3 Delta => m_delta.Translation;

        public override TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(m_delta.Translation);
        }

        public override TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(m_delta.Translation);
        }

        protected override void ApplyDelta()
        {
            var freezed = FreezeAxes.Freeze(Delta);
            m_dst.ApplyTranslation(freezed, Weight, DestinationCoordinate, DestinationOffset, ModelRoot);
        }
    }
}

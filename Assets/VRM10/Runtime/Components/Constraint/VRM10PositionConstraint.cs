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
        Vector3 m_delta;
        public override Vector3 Delta => m_delta;

        public override TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }

            return new TR(Delta) * coords;
        }

        public override TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }

            return new TR(Delta) * coords;
        }

        protected override void UpdateDelta()
        {
            m_delta = FreezeAxes.Freeze(m_src.TranslationDelta(SourceCoordinate));
            m_dst.ApplyTranslation(Delta, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

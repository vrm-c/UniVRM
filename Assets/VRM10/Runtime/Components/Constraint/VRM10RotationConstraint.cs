using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10RotationPositionConstraintBase
    {
        public override Vector3 Delta => m_delta.Rotation.eulerAngles;

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
            // 軸制限
            var fleezed = FreezeAxes.Freeze(Delta);
            var rotation = Quaternion.Euler(fleezed);

            // オイラー角を再度Quaternionへ。weight を加味してSlerpする
            m_dst.ApplyRotation(rotation, Weight, DestinationCoordinate, DestinationOffset, ModelRoot);
        }
    }
}

using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10RotationPositionConstraintBase
    {
        Quaternion m_delta;

        public override Vector3 Delta => m_delta.eulerAngles;

        public override TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(m_delta);
        }

        public override TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(m_delta);
        }

        protected override void UpdateDelta()
        {
            m_delta = m_src.Delta(SourceCoordinate, SourceOffset).Rotation;

            // 軸制限
            var fleezed = FreezeAxes.Freeze(Delta);
            var rotation = Quaternion.Euler(fleezed);

            // Debug.Log($"{delta} => {rotation}");
            // オイラー角を再度Quaternionへ。weight を加味してSlerpする
            m_dst.ApplyRotation(DestinationOffset * rotation, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

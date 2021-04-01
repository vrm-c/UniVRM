using UniGLTF.Extensions.VRMC_constraints;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 対象の初期位置と現在位置の差分(delta)を、自身の初期位置に対してWeightを乗算して加算する。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10PositionConstraint : VRM10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        public ObjectSpace SourceCoordinate = default;

        [SerializeField]
        public ObjectSpace DestinationCoordinate = default;

        [SerializeField]
        public AxisMask FreezeAxes = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        ConstraintSource m_src;

        ConstraintDestination m_dst;

        /// <summary>
        /// Editorで設定値の変更を反映するために、クリアする
        /// </summary>
        void OnValidate()
        {
            // Debug.Log("Validate");
            m_src = null;
            m_dst = null;
        }

        public override void Process()
        {
            if (Source == null)
            {
                enabled = false;
                return;
            }

            if (m_src == null)
            {
                m_src = new ConstraintSource(Source, SourceCoordinate, ModelRoot);
            }
            if (m_dst == null)
            {
                m_dst = new ConstraintDestination(transform, DestinationCoordinate);
            }

            var delta = FreezeAxes.Freeze(m_src.TranslationDelta);
            m_dst.ApplyTranslation(delta, Weight);
        }
    }
}

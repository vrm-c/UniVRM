using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        public VRM10RotationOffset SourceOffset = VRM10RotationOffset.Identity;

        [SerializeField]
        public ObjectSpace SourceCoordinate = default;

        [SerializeField]
        public ObjectSpace DestinationCoordinate = default;

        [SerializeField]
        [EnumFlags]
        public AxisMask FreezeAxes = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        ConstraintSource m_src;
        public Matrix4x4 GetSourceLocalInit()
        {
            if (m_src != null)
            {
                var parent = Matrix4x4.identity;
                if (Source != null && Source.parent != null)
                {
                    parent = Source.parent.localToWorldMatrix;
                }
                return parent * m_src.LocalInitial.Matrix;
            }
            else if (Source != null)
            {
                return Source.localToWorldMatrix;
            }
            else
            {
                return Matrix4x4.identity;
            }
        }

        public Quaternion Delta
        {
            get;
            private set;
        }

        ConstraintDestination m_dst;
        public Matrix4x4 GetDstLocalInit()
        {
            if (m_src != null)
            {
                var parent = Matrix4x4.identity;
                if (transform.parent != null)
                {
                    parent = transform.parent.localToWorldMatrix;
                }
                return parent * m_dst.LocalInitial.Matrix;
            }
            else
            {
                return transform.localToWorldMatrix;
            }
        }

        /// <summary>
        /// Editorで設定値の変更を反映するために、クリアする
        /// </summary>
        void OnValidate()
        {
            // Debug.Log("Validate");
            if (m_src != null && m_src.ModelRoot != ModelRoot)
            {
                m_src = null;
            }
            if (m_dst != null && m_dst.ModelRoot != ModelRoot)
            {
                m_dst = null;
            }
        }

        void Reset()
        {
            var current = transform;
            while (current.parent != null)
            {
                current = current.parent;
            }
            ModelRoot = current;
        }

        /// <summary>
        /// SourceのUpdateよりも先か後かはその時による。
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
            if (m_dst == null)
            {
                m_dst = new ConstraintDestination(transform, ModelRoot);
            }

            // 軸制限をしたオイラー角
            Delta = m_src.RotationDelta(SourceCoordinate);
            var fleezed = FreezeAxes.Freeze(Delta.eulerAngles);
            var rotation = Quaternion.Euler(fleezed);
            // Debug.Log($"{delta} => {rotation}");
            // オイラー角を再度Quaternionへ。weight を加味してSlerpする
            m_dst.ApplyRotation(rotation, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

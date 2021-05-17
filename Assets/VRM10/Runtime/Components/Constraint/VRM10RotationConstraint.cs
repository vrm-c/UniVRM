using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10Constraint, IVRM10ConstraintSourceDestination
    {
        [SerializeField]
        [EnumFlags]
        public AxisMask FreezeAxes = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        [Header("Source")]
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        public ObjectSpace SourceCoordinate = default;

        [SerializeField]
        public VRM10RotationOffset SourceOffset = VRM10RotationOffset.Identity;

        [Header("Destination")]
        [SerializeField]
        public ObjectSpace DestinationCoordinate = default;

        [SerializeField]
        public VRM10RotationOffset DestinationOffset = VRM10RotationOffset.Identity;

        ConstraintSource m_src;

        public TR GetSourceCoords()
        {
            if (Source == null)
            {
                throw new ConstraintException(ConstraintException.ExceptionTypes.NoSource);
            }

            switch (SourceCoordinate)
            {
                case ObjectSpace.model:
                    {
                        if (ModelRoot == null)
                        {
                            throw new ConstraintException(ConstraintException.ExceptionTypes.NoModelWithModelSpace);
                        }
                        return new TR(ModelRoot.rotation * SourceOffset.Rotation, Source.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return new TR(Source.rotation * SourceOffset.Rotation, Source.position);
                        }

                        // runtime
                        var parent = Quaternion.identity;
                        if (Source.parent != null)
                        {
                            parent = Source.parent.rotation;
                        }
                        return new TR(parent * m_src.LocalInitial.Rotation * SourceOffset.Rotation, Source.position);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(Delta);
        }

        public Quaternion Delta
        {
            get;
            private set;
        }

        ConstraintDestination m_dst;
        public TR GetDstCoords()
        {
            switch (DestinationCoordinate)
            {
                case ObjectSpace.model:
                    {
                        if (ModelRoot == null)
                        {
                            throw new ConstraintException(ConstraintException.ExceptionTypes.NoModelWithModelSpace);
                        }
                        return new TR(ModelRoot.rotation * DestinationOffset.Rotation, transform.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return new TR(transform.rotation * DestinationOffset.Rotation, transform.position);
                        }

                        // runtime
                        var parent = Quaternion.identity;
                        if (transform.parent != null)
                        {
                            parent = transform.parent.rotation;
                        }
                        return new TR(parent * m_dst.LocalInitial.Rotation * DestinationOffset.Rotation, transform.position);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }

            return coords * new TR(Delta);
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

            // 回転差分
            Delta = m_src.RotationDelta(SourceCoordinate, SourceOffset.Rotation);

            // 軸制限
            var fleezed = FreezeAxes.Freeze(Delta.eulerAngles);
            var rotation = Quaternion.Euler(fleezed);

            // Debug.Log($"{delta} => {rotation}");
            // オイラー角を再度Quaternionへ。weight を加味してSlerpする
            m_dst.ApplyRotation(DestinationOffset.Rotation * rotation, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

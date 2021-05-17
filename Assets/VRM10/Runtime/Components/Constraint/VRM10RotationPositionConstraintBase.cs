using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    public abstract class VRM10RotationPositionConstraintBase : VRM10Constraint
    {
        [SerializeField]
        [EnumFlags]
        AxisMask m_freezeAxes = default;
        public AxisMask FreezeAxes
        {
            get => m_freezeAxes;
            set => m_freezeAxes = value;
        }

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        #region Source
        [Header("Source")]
        [SerializeField]
        public Transform Source = default;

        public Transform GetSource() => Source;

        [SerializeField]
        ObjectSpace m_sourceCoordinate = default;
        public ObjectSpace SourceCoordinate
        {
            get => m_sourceCoordinate;
            set => m_sourceCoordinate = value;
        }

        [SerializeField]
        VRM10RotationOffset m_sourceOffset = VRM10RotationOffset.Identity;

        public Quaternion SourceOffset
        {
            get => m_sourceOffset.Rotation;
            set => m_sourceOffset.Rotation = value;
        }
        #endregion


        #region Destination
        [Header("Destination")]
        [SerializeField]
        ObjectSpace m_destinationCoordinate = default;
        public ObjectSpace DestinationCoordinate
        {
            get => m_destinationCoordinate;
            set => m_destinationCoordinate = value;
        }

        [SerializeField]
        public VRM10RotationOffset m_destinationOffset = VRM10RotationOffset.Identity;

        public Quaternion DestinationOffset
        {
            get => m_destinationOffset.Rotation;
            set => m_destinationOffset.Rotation = value;
        }
        #endregion

        public abstract Vector3 Delta { get; }

        protected ConstraintSource m_src;

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
                        return new TR(ModelRoot.rotation * SourceOffset, Source.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return new TR(Source.rotation * SourceOffset, Source.position);
                        }

                        // runtime
                        var parent = Quaternion.identity;
                        if (Source.parent != null)
                        {
                            parent = Source.parent.rotation;
                        }
                        return new TR(parent * m_src.LocalInitial.Rotation * SourceOffset, Source.position);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract TR GetSourceCurrent();

        protected ConstraintDestination m_dst;

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
                        return new TR(ModelRoot.rotation * DestinationOffset, transform.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return new TR(transform.rotation * DestinationOffset, transform.position);
                        }

                        // runtime
                        var parent = Quaternion.identity;
                        if (transform.parent != null)
                        {
                            parent = transform.parent.rotation;
                        }
                        return new TR(parent * m_dst.LocalInitial.Rotation * DestinationOffset, transform.position);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract TR GetDstCurrent();


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

        public Component GetComponent()
        {
            return this;
        }

        protected abstract void UpdateDelta();

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
            UpdateDelta();
        }
    }
}

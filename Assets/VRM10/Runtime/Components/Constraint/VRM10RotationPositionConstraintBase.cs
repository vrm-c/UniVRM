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

        protected ConstraintSource m_src;

        TR SourceModelCoords
        {
            get
            {
                if (m_src == null)
                {
                    return TR.FromWorld(ModelRoot);
                }
                else
                {
                    return TR.FromParent(ModelRoot) * m_src.ModelInitial;
                }
            }
        }

        TR SourceInitialCoords
        {
            get
            {
                if (m_src == null)
                {
                    return TR.FromWorld(Source);
                }
                else
                {
                    return TR.FromParent(Source) * m_src.LocalInitial;
                }
            }
        }

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
                        return new TR(SourceModelCoords.Rotation * SourceOffset, SourceInitialCoords.Translation);
                    }

                case ObjectSpace.local:
                    {
                        return new TR(SourceInitialCoords.Rotation * SourceOffset, SourceInitialCoords.Translation);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract TR GetSourceCurrent();

        protected ConstraintDestination m_dst;

        TR DestinationModelCoords
        {
            get
            {
                if (m_dst == null)
                {
                    return TR.FromWorld(ModelRoot);
                }
                else
                {
                    return TR.FromParent(ModelRoot) * m_dst.ModelInitial;
                }
            }
        }

        public TR DestinationInitialCoords
        {
            get
            {
                if (m_dst == null)
                {
                    return TR.FromWorld(transform);
                }
                else
                {
                    return TR.FromParent(transform) * m_dst.LocalInitial;
                }
            }
        }

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
                        return new TR(DestinationModelCoords.Rotation * DestinationOffset, DestinationInitialCoords.Translation);
                    }

                case ObjectSpace.local:
                    {
                        return new TR(DestinationInitialCoords.Rotation * DestinationOffset, DestinationInitialCoords.Translation);
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

        protected TR m_delta;
        public abstract Vector3 Delta { get; }

        protected abstract void ApplyDelta();

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

            m_delta = m_src.Delta(SourceCoordinate, SourceOffset);
            ApplyDelta();
        }
    }
}

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

        #region Source
        [Header("Source")]
        [SerializeField]
        Transform m_source = default;
        public override Transform Source
        {
            get => m_source;
            set => m_source = value;
        }

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
                        var init = SourceInitialCoords(ObjectSpace.model);
                        return new TR(init.Rotation * SourceOffset, init.Translation);
                    }

                case ObjectSpace.local:
                    {
                        var init = SourceInitialCoords(ObjectSpace.local);
                        return new TR(init.Rotation * SourceOffset, init.Translation);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract TR GetSourceCurrent();

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
                        var init = DestinationInitialCoords(ObjectSpace.model);
                        return new TR(init.Rotation * DestinationOffset, init.Translation);
                    }

                case ObjectSpace.local:
                    {
                        var init = DestinationInitialCoords(ObjectSpace.local);
                        return new TR(init.Rotation * DestinationOffset, init.Translation);
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

        public Component GetComponent()
        {
            return this;
        }

        protected TR m_delta;
        public abstract Vector3 Delta { get; }

        protected abstract void ApplyDelta();

        public override void OnProcess()
        {
            m_delta = m_src.Delta(SourceCoordinate, SourceOffset);
            ApplyDelta();
        }
    }
}

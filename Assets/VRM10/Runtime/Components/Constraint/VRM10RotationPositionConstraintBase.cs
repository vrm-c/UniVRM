using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    public abstract class VRM10RotationPositionConstraintBase : VRM10Constraint
    {
        [SerializeField]
        [EnumFlags]
        AxisMask m_axes = default;
        public AxisMask Axes
        {
            get => m_axes;
            set => m_axes = value;
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

            var init = SourceInitialCoords();
            return new TR(init.Rotation * SourceOffset, init.Translation);
        }

        public abstract TR GetSourceCurrent();

        public TR GetDstCoords()
        {
            var init = DestinationInitialCoords();
            return new TR(init.Rotation * DestinationOffset, init.Translation);
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
            m_delta = m_src.Delta(SourceOffset);
            ApplyDelta();
        }
    }
}

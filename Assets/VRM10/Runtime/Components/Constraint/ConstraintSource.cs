using UnityEngine;
using System;
using UniGLTF.Extensions.VRMC_node_constraint;

namespace UniVRM10
{
    public class ConstraintSource
    {
        public readonly Transform ModelRoot;
        readonly Transform m_transform;

        /// <summary>
        /// initial: ModelRoot.localToWorldMatrix^-1 * t.localToWorldMatrix
        /// </summary>
        public readonly TR ModelInitial;

        /// <summary>
        /// initial: t.localPosition, t.localRotation, t.localScale
        /// </summary>
        public readonly TR LocalInitial;

        public TR Delta(ObjectSpace coords, Quaternion sourceRotationOffset)
        {
            switch (coords)
            {
                // case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                case ObjectSpace.local: return TR.FromLocal(m_transform) * (LocalInitial * new TR(sourceRotationOffset)).Inverse();
                case ObjectSpace.model: return TR.FromWorld(m_transform) * (TR.FromWorld(ModelRoot) * ModelInitial * new TR(sourceRotationOffset)).Inverse();
                default: throw new NotImplementedException();
            }
        }

        public ConstraintSource(Transform t, Transform modelRoot = null)
        {
            {
                m_transform = t;
                LocalInitial = TR.FromLocal(t);
            }

            if (modelRoot != null)
            {
                ModelRoot = modelRoot;
                ModelInitial = TR.FromLocal(t);
            }
        }
    }
}

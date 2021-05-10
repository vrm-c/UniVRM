using UnityEngine;
using System;
using UniGLTF.Extensions.VRMC_node_constraint;

namespace UniVRM10
{
    class ConstraintSource
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

        public Vector3 TranslationDelta(ObjectSpace coords)
        {
            switch (coords)
            {
                // case ObjectSpace.World: return m_transform.position - m_initial.Translation;
                case ObjectSpace.local: return m_transform.localPosition - LocalInitial.Translation;
                case ObjectSpace.model: return ModelRoot.worldToLocalMatrix.MultiplyPoint(m_transform.position) - ModelInitial.Translation;
                default: throw new NotImplementedException();
            }
        }

        public Quaternion RotationDelta(ObjectSpace coords)
        {
            switch (coords)
            {
                // case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                case ObjectSpace.local: return m_transform.localRotation * Quaternion.Inverse(LocalInitial.Rotation);
                case ObjectSpace.model: return m_transform.rotation * Quaternion.Inverse(ModelInitial.Rotation) * Quaternion.Inverse(ModelRoot.rotation);
                default: throw new NotImplementedException();
            }
        }

        public ConstraintSource(Transform t, Transform modelRoot = null)
        {
            m_transform = t;

            {
                LocalInitial = TR.FromLocal(t);
            }

            {
                var world = TR.FromWorld(t);
                ModelRoot = modelRoot;
                ModelInitial = new TR(world.Rotation * Quaternion.Inverse(ModelRoot.rotation), modelRoot.worldToLocalMatrix.MultiplyPoint(world.Translation));
            }
        }
    }
}

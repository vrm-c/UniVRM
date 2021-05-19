using UnityEngine;
using System;
using UniGLTF.Extensions.VRMC_node_constraint;

namespace UniVRM10
{
    public class ConstraintSource
    {
        public readonly Transform ModelRoot;
        readonly Transform Source;

        public readonly TR ModelInitial;

        public readonly TR LocalInitial;

        public TR Delta(ObjectSpace coords, Quaternion sourceRotationOffset)
        {
            switch (coords)
            {
                // case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                case ObjectSpace.local: return TR.FromLocal(Source) * (LocalInitial * new TR(sourceRotationOffset)).Inverse();
                case ObjectSpace.model: return TR.FromWorld(Source) * (TR.FromParent(ModelRoot) * ModelInitial * new TR(sourceRotationOffset)).Inverse();
                default: throw new NotImplementedException();
            }
        }

        public ConstraintSource(Transform t, Transform modelRoot = null)
        {
            {
                Source = t;
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

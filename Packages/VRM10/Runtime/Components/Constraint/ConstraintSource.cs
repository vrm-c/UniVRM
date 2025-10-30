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

        public TR Delta(Quaternion sourceRotationOffset)
        {
            return TR.FromLocal(Source) * (LocalInitial * new TR(sourceRotationOffset)).Inverse();
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

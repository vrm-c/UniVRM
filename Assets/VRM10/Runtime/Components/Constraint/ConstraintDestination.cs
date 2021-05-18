using UnityEngine;

namespace UniVRM10
{
    public class ConstraintDestination
    {
        public readonly Transform Destination;
        public readonly TR ModelInitial;
        public readonly TR LocalInitial;
        public readonly Transform ModelRoot;

        public ConstraintDestination(Transform t, Transform modelRoot = null)
        {
            Destination = t;
            LocalInitial = TR.FromLocal(t);

            if (modelRoot != null)
            {
                ModelRoot = modelRoot;
                ModelInitial = TR.FromRelative(t, modelRoot);
            }
        }

        public void ApplyLocal(TR tr)
        {
            Destination.localPosition = tr.Translation;
            Destination.localRotation = tr.Rotation;
        }

        public void ApplyModel(TR tr)
        {
            Destination.position = tr.Translation;
            Destination.rotation = tr.Rotation;
        }
    }
}

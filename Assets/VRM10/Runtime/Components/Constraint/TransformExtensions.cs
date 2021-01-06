using UnityEngine;

namespace UniVRM10
{
    public static class TransformExtensions
    {
        public static Quaternion ParentRotation(this Transform transform)
        {
            return transform.parent == null ? Quaternion.identity : transform.parent.rotation;
        }
    }
}

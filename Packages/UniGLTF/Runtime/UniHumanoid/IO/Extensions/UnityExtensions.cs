using System.Collections.Generic;
using UnityEngine;


namespace UniHumanoid
{
    public static class UnityExtensions
    {
        public static Quaternion ReverseX(this Quaternion quaternion)
        {
            float angle;
            Vector3 axis;
            quaternion.ToAngleAxis(out angle, out axis);

            return Quaternion.AngleAxis(-angle, new Vector3(-axis.x, axis.y, axis.z));
        }

        public static IEnumerable<Transform> GetChildren(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child;
            }
        }

        public static IEnumerable<Transform> Traverse(this Transform parent)
        {
            yield return parent;

            foreach (Transform child in parent)
            {
                foreach (Transform descendant in Traverse(child))
                {
                    yield return descendant;
                }
            }
        }

        public static SkeletonBone ToSkeletonBone(this Transform t)
        {
            var sb = new SkeletonBone();
            sb.name = t.name;
            sb.position = t.localPosition;
            sb.rotation = t.localRotation;
            sb.scale = t.localScale;
            return sb;
        }
    }
}

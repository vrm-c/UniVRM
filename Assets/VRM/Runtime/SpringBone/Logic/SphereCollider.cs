using UnityEngine;

namespace VRM.SpringBone
{
    struct SphereCollider
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public SphereCollider(Transform transform, VRMSpringBoneColliderGroup.SphereCollider collider)
        {
            Position = transform.TransformPoint(collider.Offset);
            var ls = transform.lossyScale;
            var scale = Mathf.Max(Mathf.Max(ls.x, ls.y), ls.z);
            Radius = scale * collider.Radius;
        }
    }
}
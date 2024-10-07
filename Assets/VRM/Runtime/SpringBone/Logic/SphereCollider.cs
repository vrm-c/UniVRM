using UniGLTF;
using UnityEngine;

namespace VRM.SpringBone
{
    readonly struct SphereCollider
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

        public bool TryCollide(SpringBoneSettings settings, Transform transform, Vector3 nextTail, out Vector3 posFromCollider)
        {
            var m_radius = settings.HitRadius * transform.UniformedLossyScale();
            var r = m_radius + Radius;
            if (Vector3.SqrMagnitude(nextTail - Position) <= (r * r))
            {
                // ヒット。Colliderの半径方向に押し出す
                var normal = (nextTail - Position).normalized;
                posFromCollider = Position + normal * (m_radius + Radius);
                return true;
            }
            else
            {
                posFromCollider = default;
                return false;
            }
        }
    }
}
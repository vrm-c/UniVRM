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

        public Vector3 Collide(SpringBoneSettings settings, Transform m_transform, SpringBoneJointInit init, Vector3 nextTail)
        {
            var m_radius = settings.HitRadius * m_transform.UniformedLossyScale();
            var r = m_radius + Radius;
            if (Vector3.SqrMagnitude(nextTail - Position) <= (r * r))
            {
                // ヒット。Colliderの半径方向に押し出す
                var normal = (nextTail - Position).normalized;
                var posFromCollider = Position + normal * (m_radius + Radius);
                // 長さをboneLengthに強制
                nextTail = m_transform.position + (posFromCollider - m_transform.position).normalized * init.Length;
            }
            return nextTail;
        }
    }
}
using UnityEngine;
using UniVRM10;


namespace SphereTriangle
{
    public class SphereCapsuleCollider
    {
        public readonly VRM10SpringBoneCollider _vrm;
        public Color GizmoColor = Color.yellow;
        public bool SolidGizmo = false;

        public float Radius => _vrm.Radius;
        public bool IsCapsule => _vrm.ColliderType == VRM10SpringBoneColliderTypes.Capsule;
        public Vector3 HeadWorldPosition => _vrm.transform.TransformPoint(_vrm.Offset);
        public Vector3 TailWorldPosition => _vrm.transform.TransformPoint(_vrm.TailOrNormal);
        public Ray? HeadTailRay => IsCapsule ? new Ray { origin = HeadWorldPosition, direction = TailWorldPosition - HeadWorldPosition } : null;
        public float CapsuleLength => !IsCapsule ? 0 : Vector3.Distance(TailWorldPosition, HeadWorldPosition);

        public SphereCapsuleCollider(VRM10SpringBoneCollider vrmCollider)
        {
            _vrm = vrmCollider;
        }

        public Bounds GetBounds()
        {
            if (IsCapsule)
            {
                var h = HeadWorldPosition;
                var t = TailWorldPosition;
                var d = h - t;
                var aabb = new Bounds((h + t) * 0.5f, new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z)));
                aabb.Expand(_vrm.Radius * 2);
                return aabb;
            }
            else
            {
                return new Bounds(HeadWorldPosition, new Vector3(_vrm.Radius, _vrm.Radius, _vrm.Radius));
            }
        }

        /// <summary>
        /// collide sphere a and sphere b.
        /// move sphere b to resolved if collide.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="ra"></param>
        /// <param name="to"></param>
        /// <param name="ba"></param>
        /// <param name="resolved"></param>
        /// <returns></returns>
        static bool TryCollideSphereAndSphere(
            in Vector3 from, float ra,
            in Vector3 to, float rb,
            out LineSegment resolved
            )
        {
            var d = Vector3.Distance(from, to);
            if (d > (ra + rb))
            {
                resolved = default;
                return false;
            }
            Vector3 normal = (to - from).normalized;
            resolved = new(from, from + normal * (d - rb));
            return true;
        }

        /// <summary>
        /// collide capsule and sphere b.
        /// move sphere b to resolved if collide.
        /// </summary>
        /// <param name="capsuleHead"></param>
        /// <param name="capsuleTail"></param>
        /// <param name="capsuleRadius"></param>
        /// <param name="b"></param>
        /// <param name="rb"></param>
        static bool TryCollideCapsuleAndSphere(
            in Vector3 capsuleHead,
            in Vector3 capsuleTail,
            float capsuleRadius,
            in Vector3 b,
            float rb,
            out LineSegment resolved
            )
        {
            var P = (capsuleTail - capsuleHead).normalized;
            var Q = b - capsuleHead;
            var dot = Vector3.Dot(P, Q);
            if (dot <= 0)
            {
                // head側半球の球判定
                return TryCollideSphereAndSphere(capsuleHead, capsuleRadius, b, rb, out resolved);
            }

            var t = dot / P.magnitude;
            if (t >= 1.0f)
            {
                // tail側半球の球判定
                return TryCollideSphereAndSphere(capsuleTail, capsuleRadius, b, rb, out resolved);
            }

            // head-tail上の m_transform.position との最近点
            var p = capsuleHead + P * t;
            return TryCollideSphereAndSphere(p, capsuleRadius, b, rb, out resolved);
        }

        /// <summary>
        /// collision for strand
        /// </summary>
        /// <param name="p"></param>
        /// <param name="radius"></param>
        /// <param name="resolved"></param>
        /// <returns></returns>
        public bool TryCollide(in Vector3 p, float radius, out LineSegment resolved)
        {
            if (IsCapsule)
            {
                return TryCollideCapsuleAndSphere(HeadWorldPosition, TailWorldPosition, this.Radius, p, radius, out resolved);
            }
            else
            {
                return TryCollideSphereAndSphere(HeadWorldPosition, this.Radius, p, radius, out resolved);
            }
        }

        public static void DrawCapsuleGizmo(Vector3 start, Vector3 end, float radius)
        {
            var tail = end - start;
            var distance = (end - start).magnitude;
            Gizmos.matrix = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.forward, tail), Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.DrawWireSphere(Vector3.forward * distance, radius);
            var capsuleEnd = Vector3.forward * distance;
            var offsets = new Vector3[] { new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Gizmos.DrawLine(offsets[i] * radius, capsuleEnd + offsets[i] * radius);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }

        public void OnDrawGizmos()
        {
            if (SolidGizmo)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(HeadWorldPosition, Radius);
            }

            Gizmos.color = GizmoColor;
            if (_vrm.transform.parent != null)
            {
                Gizmos.DrawLine(HeadWorldPosition, HeadWorldPosition);
            }
            if (IsCapsule)
            {
                DrawCapsuleGizmo(HeadWorldPosition, TailWorldPosition, Radius);
            }
            else
            {
                Gizmos.DrawWireSphere(HeadWorldPosition, Radius);
            }

#if AABB_DEBUG
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.magenta;
            var aabb = GetBounds();
            Gizmos.DrawWireCube(aabb.center, aabb.size);
#endif
        }
    }
}
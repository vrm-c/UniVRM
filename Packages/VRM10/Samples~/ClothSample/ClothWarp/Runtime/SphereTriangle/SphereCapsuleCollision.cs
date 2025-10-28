using UnityEngine;


namespace SphereTriangle
{
    public static class SphereSphereCollision
    {
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
        public static bool TryCollideSphereAndSphere(
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
        public static bool TryCollideCapsuleAndSphere(
            in Vector3 capsuleHead,
            in Vector3 capsuleTail,
            float capsuleRadius,
            in Vector3 b,
            float rb,
            out LineSegment resolved
            )
        {
            var P = (capsuleTail - capsuleHead);
            var Q = b - capsuleHead;
            var dot = Vector3.Dot(P.normalized, Q);
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
    }
}
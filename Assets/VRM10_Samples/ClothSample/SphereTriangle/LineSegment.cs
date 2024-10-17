using UnityEngine;

namespace SphereTriangle
{
    public readonly struct LineSegment
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;

        public Vector3 Vector => End - Start;
        public Ray Ray => new Ray(Start, Vector);
        public float Length => Vector.magnitude;
        public float SqLength => Vector.sqrMagnitude;

        public LineSegment(in Vector3 start, in Vector3 end) => (Start, End) = (start, end);

        public Vector3 GetPoint(float t)
        {
            return Start + Vector * t;
        }

        /// <summary>
        /// 球との衝突時の移動ベクトル
        /// Start が球の中心。End 衝突点。
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public Vector3 GetDelta(float radius)
        {
            return Vector.normalized * (radius - Length);
        }

        public float Project(in Vector3 p)
        {
            var dir = p - Start;
            return Vector3.Dot(Vector.normalized, dir) / dir.magnitude;
        }

        // P(s)
        // Q(t)
        public static (float s, float t) CalcClosest(in LineSegment p, in LineSegment q)
        {
            var d_p2 = Vector3.Dot(p.Vector, p.Vector);
            var d_q2 = Vector3.Dot(q.Vector, q.Vector);
            var d_pq = Vector3.Dot(p.Vector, q.Vector);
            var d_q_p_p = Vector3.Dot(q.Start - p.Start, p.Vector);
            var d_p_q_q = Vector3.Dot(p.Start - q.Start, q.Vector);
            var denom = d_p2 * d_q2 - d_pq * d_pq;
            if (denom < 1e-4)
            {
                var pq = q.Start - p.Start;
                var d = Vector3.Dot(p.Vector, pq);
                var s = d / p.Length;
                return (s, 0);
            }
            else
            {
                var f = 1 / denom;
                var s = d_q2 * d_q_p_p + d_pq * d_p_q_q;
                var t = d_pq * d_q_p_p + d_p2 * d_p_q_q;
                return (f * s, f * t);
            }
        }

        // public static bool Intersect(in LineSegment p, in LineSegment q, out float s, out float t)
        // {
        //     (s, t) = CalcClosest(p, q);
        //     if (s >= 0.0 && s <= 1.0 && t >= 0.0 && t <= 1.0)
        //     {
        //         if (Vector3.Distance(p.GetPoint(s), q.GetPoint(t)) < 1e4f)
        //         {
        //             return true;
        //         }
        //     }
        //     return false;
        // }

        public bool TryClampPlaneDistance(in Plane p, float distance, out LineSegment clamped, out Vector3 o)
        {
            var ray = Ray;
            var hit = p.Raycast(ray, out var t);
            if (!hit && t == 0)
            {
                // 平行
                clamped = default;
                o = default;
                return false;
            }

            o = ray.GetPoint(t);
            var vs = Start - o;
            var ve = End - o;

            var ds = p.GetDistanceToPoint(Start);
            var de = p.GetDistanceToPoint(End);
            var ts = 1.0f;
            var te = 1.0f;
            if (ds > 0)
            {
                if (de > 0)
                {
                    //+s
                    //+e
                    //==
                    if (ds > distance && de > distance)
                    {
                        clamped = default;
                        return false;
                    }
                    ts = Mathf.Min(distance, ds) / ds;
                    te = Mathf.Min(distance, de) / de;
                }
                else
                {
                    //+s
                    //==
                    //-e
                    ts = Mathf.Min(distance, ds) / ds;
                    te = Mathf.Max(-distance, de) / de;
                }
            }
            else
            {
                if (de < 0)
                {
                    //==
                    //-s
                    //-e
                    if (ds < -distance && de < -distance)
                    {
                        clamped = default;
                        return false;
                    }
                    ts = Mathf.Max(-distance, ds) / ds;
                    te = Mathf.Max(-distance, de) / de;
                }
                else
                {
                    //+e
                    //==
                    //-s
                    ts = Mathf.Max(-distance, ds) / ds;
                    te = Mathf.Min(distance, de) / de;
                }
            }

            var cs = new Ray(o, vs).GetPoint(vs.magnitude * ts);
            var ce = new Ray(o, ve).GetPoint(ve.magnitude * te);
            clamped = new(cs, ce);
            return true;
        }

        public void DrawGizmos(float radius = 0.01f)
        {
            Gizmos.DrawLine(Start, End);
            Gizmos.DrawWireSphere(Start, radius);
            Gizmos.DrawWireSphere(End, radius);
        }
    }
}
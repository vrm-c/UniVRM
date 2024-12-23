using UnityEngine;


namespace SphereTriangle
{
    /// <summary>
    /// 三角形との交差が 2 => 交点 と 交点
    /// 三角形との交差が 1 => 交点 と 三角形内の線分端点
    /// 三角形との交差が 0 => 線分の端点が両方とも三角形内の場合。端点 と 端点
    /// </summary>
    public struct TriangleSegmentIntersection
    {
        public float t0;
        public float t1;
        public TriangleSegmentIntersection(float _t0, float _t1)
        {
            if (_t0 <= _t1)
            {
                t0 = _t0;
                t1 = _t1;
            }
            else
            {
                t0 = _t1;
                t1 = _t0;
            }
        }
    }

    public struct Triangle
    {
        public Plane Plane;

        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public LineSegment AB => new LineSegment(a, b);
        public LineSegment BC => new LineSegment(b, c);
        public LineSegment CA => new LineSegment(c, a);

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Plane = new Plane(a, b, c);
            this.a = a;
            this.b = b;
            this.c = c;
        }

        /// <summary>
        /// p がすべての辺の同じ側(内積的な意味で)にある => 三角形の内側にある
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsSameSide(Vector3 p)
        {
            var da = Vector3.Dot(Vector3.Cross(p - a, b - a), Plane.normal);
            var db = Vector3.Dot(Vector3.Cross(p - b, c - b), Plane.normal);
            var dc = Vector3.Dot(Vector3.Cross(p - c, a - c), Plane.normal);

            if (da > 0)
            {
                if (db > 0)
                {
                    if (dc > 0)
                    {
                        return true;
                    }
                }
            }
            else if (da < 0)
            {
                if (db < 0)
                {
                    if (dc < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // /// <summary>
        // /// p から lerp の係数を計算
        // /// </summary>
        // /// <param name="p0"></param>
        // /// <param name="p1"></param>
        // /// <param name="p"></param>
        // /// <returns></returns>
        // static float getT(in Vector3 p0, in Vector3 p1, in Vector3 p)
        // {
        //     return (p - p0).magnitude / (p1 - p0).magnitude;
        // }

        // /// <summary>
        // /// 三角形と線分の交差を判定する。
        // /// </summary>
        // /// <param name="p0">線分始点(t=0)。三角形 abc と同一平面を期待</param>
        // /// <param name="p1">線分終点(t=1)。三角形 abc と同一平面を期待</param>
        // /// <returns></returns>
        // public bool TryIntersectSegment(in Vector3 p0, in Vector3 p1, out TriangleSegmentIntersection intersection)
        // {
        //     // [ab bc ca]
        //     // [ab bc] [bc ca] [ca ab]
        //     // [ab] [bc] [ca]
        //     // []
        //     if (TryIntersectSegments(p0, p1, a, b, out var ab))
        //     {
        //         if (TryIntersectSegments(p0, p1, b, c, out var bc))
        //         {
        //             // [ab bc] or [ab bc ca]
        //             intersection = new TriangleSegmentIntersection(getT(p0, p1, ab), getT(p0, p1, bc));
        //             return true;
        //         }
        //         else
        //         {
        //             if (TryIntersectSegments(p0, p1, c, a, out var ca))
        //             {
        //                 // [ab ca]
        //                 intersection = new TriangleSegmentIntersection(getT(p0, p1, ab), getT(p0, p1, ca));
        //                 return true;
        //             }
        //             else
        //             {
        //                 // [ab]
        //                 if (IsSameSide(p0))
        //                 {
        //                     intersection = new TriangleSegmentIntersection(0, getT(p0, p1, ab));
        //                     return true;
        //                 }
        //                 else
        //                 {
        //                     intersection = new TriangleSegmentIntersection(getT(p0, p1, ab), 1.0f);
        //                     return true;
        //                 }
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (TryIntersectSegments(p0, p1, b, c, out var bc))
        //         {
        //             if (TryIntersectSegments(p0, p1, c, a, out var ca))
        //             {
        //                 // [bc  ca]
        //                 intersection = new TriangleSegmentIntersection(getT(p0, p1, bc), getT(p0, p1, ca));
        //                 return true;
        //             }
        //             else
        //             {
        //                 // [bc]
        //                 if (IsSameSide(p0))
        //                 {
        //                     intersection = new TriangleSegmentIntersection(0, getT(p0, p1, bc));
        //                     return true;
        //                 }
        //                 else
        //                 {
        //                     intersection = new TriangleSegmentIntersection(getT(p0, p1, bc), 1.0f);
        //                     return true;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             if (TryIntersectSegments(p0, p1, c, a, out var ca))
        //             {
        //                 // [ca]
        //                 if (IsSameSide(p0))
        //                 {
        //                     intersection = new TriangleSegmentIntersection(0, getT(p0, p1, ca));
        //                     return true;
        //                 }
        //                 else
        //                 {
        //                     intersection = new TriangleSegmentIntersection(getT(p0, p1, ca), 1.0f);
        //                     return true;
        //                 }
        //             }
        //             else
        //             {
        //                 // []
        //                 if (IsSameSide(p0) && IsSameSide(p1))
        //                 {
        //                     intersection = new TriangleSegmentIntersection(0, 1);
        //                     return true;
        //                 }
        //                 else
        //                 {
        //                     intersection = default;
        //                     return false;
        //                 }
        //             }
        //         }
        //     }
        // }

        // /// <summary>
        // /// 線分と線分の交差を判定する
        // /// a-b d
        // ///    /
        // ///   c
        // /// https://qiita.com/zu_rin/items/09876d2c7ec12974bc0f
        // /// </summary>
        // /// <param name="a"></param>
        // /// <param name="b"></param>
        // /// <param name="c"></param>
        // /// <param name="d"></param>
        // /// <returns></returns>
        // static bool TryIntersectSegments(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, out Vector3 p)
        // {
        //     var deno = Vector3.Cross(b - a, d - c).magnitude;
        //     if (deno < 1e-5)
        //     {
        //         // 線分が平行
        //         p = default;
        //         return false;
        //     }

        //     var s = Vector3.Cross(c - a, d - c).magnitude / deno;
        //     var t = Vector3.Cross(b - a, a - c).magnitude / deno;
        //     if (s < 0.0 || 1.0 < s || t < 0.0 || 1.0 < t)
        //     {
        //         // 線分が交差していない
        //         p = default;
        //         return false;
        //     }

        //     p = new Vector3(
        //         a.x + s * (b - a).x,
        //         a.y + s * (b - a).y,
        //         a.z + s * (b - a).z
        //      );
        //     return true;
        // }

        // public (Vector3, float) ProjectAB(in Vector3 p)
        // {
        //     var ab = (b - a).normalized;
        //     var d = Vector3.Dot(ab, (p - a));
        //     var x = a + ab * d;
        //     return (x, d / (b - a).magnitude);
        // }
        // public (Vector3, float) ProjectBC(in Vector3 p)
        // {
        //     var bc = (c - b).normalized;
        //     var d = Vector3.Dot(bc, (p - b));
        //     var x = b + bc * d;
        //     return (x, d / (c - b).magnitude);
        // }
        // public (Vector3, float) ProjectCA(in Vector3 p)
        // {
        //     var ca = (a - c).normalized;
        //     var d = Vector3.Dot(ca, (p - c));
        //     var x = c + ca * d;
        //     return (x, d / (a - c).magnitude);
        // }

        // public TriangleProjection Project(in Vector3 p)
        // {
        //     var (ab, ta) = ProjectAB(p);
        //     var (bc, tb) = ProjectBC(p);
        //     var (ca, tc) = ProjectCA(p);
        //     return new TriangleProjection
        //     {
        //         ab = ab,
        //         ta = ta,
        //         bc = bc,
        //         tb = tb,
        //         ca = ca,
        //         tc = tc,
        //     };
        // }

        public (Vector3, float) GetClosest(in Vector3 p)
        {
            var ab = AB;
            var t_ab = ab.Project(p);
            var p_ab = ab.GetPoint(t_ab);
            var d_ab = Vector3.Distance(p, p_ab);

            var bc = BC;
            var t_bc = bc.Project(p);
            var p_bc = bc.GetPoint(t_bc);
            var d_bc = Vector3.Distance(p, p_bc);

            var ca = CA;
            var t_ca = ca.Project(p);
            var p_ca = ca.GetPoint(t_ca);
            var d_ca = Vector3.Distance(p, p_ca);

            if (d_ab < d_bc)
            {
                if (d_ab < d_ca)
                {
                    return (p_ab, d_ab);
                }
                else
                {
                    return (p_ca, d_ca);
                }
            }
            else
            {
                if (d_bc < d_ca)
                {
                    return (p_bc, d_bc);
                }
                else
                {
                    return (p_ca, d_ca);
                }
            }
        }

        public void DrawGizmos()
        {
            var points = (stackalloc Vector3[3]);
            points[0] = a;
            points[1] = b;
            points[2] = c;
            Gizmos.DrawLineStrip(points, true);
        }
    }
}
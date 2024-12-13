// #define USE_VERTEX_DISTANCE
using System;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace SphereTriangle
{
    [Serializable]
    public class TriangleCapsuleCollisionSolver
    {
        public enum IntersectionType
        {
            PlaneInTriangle,
#if USE_VERTEX_DISTANCE
            VertexDistance,
#endif
            EdgeDistance,
        }

        public struct Result
        {
            /// <summary>
            /// カプセルStartの投影点が三角形内部
            /// </summary>
            public LineSegment? StartInTriangle;
            /// <summary>
            /// カプセルEndの投影点が三角形内部
            /// </summary>
            public LineSegment? EndInTriangle;

#if USE_VERTEX_DISTANCE
            /// <summary>
            /// カプセル線分と頂点Aの距離が半径以内
            /// </summary>
            public LineSegment? VertexADistance;
            /// <summary>
            /// カプセル線分と頂点Bの距離が半径以内
            /// </summary>
            public LineSegment? VertexBDistance;
            /// <summary>
            /// カプセル線分と頂点Cの距離が半径以内
            /// </summary>
            public LineSegment? VertexCDistance;
#endif

            /// <summary>
            /// カプセル線分と辺ABの距離が半径以内
            /// </summary>
            public LineSegment? EdgeABDistance;
            /// <summary>
            /// カプセル線分と辺BCの距離が半径以内
            /// </summary>
            public LineSegment? EdgeBCDistance;
            /// <summary>
            /// カプセル線分と辺CAの距離が半径以内
            /// </summary>
            public LineSegment? EdgeCADistance;

            public IntersectionType? TryGetClosest(out LineSegment value)
            {
                // using var profile = new ProfileSample("TryGetClosest");
                var d = float.PositiveInfinity;
                value = default;
                IntersectionType? it = default;

                if (StartInTriangle.HasValue && StartInTriangle.Value.SqLength < d)
                {
                    value = StartInTriangle.Value;
                    it = IntersectionType.PlaneInTriangle;
                }

                if (EndInTriangle.HasValue && EndInTriangle.Value.SqLength < d)
                {
                    value = EndInTriangle.Value;
                    it = IntersectionType.PlaneInTriangle;
                }

#if USE_VERTEX_DISTANCE
                if (VertexADistance.HasValue) yield return (IntersectionType.VertexDistance, VertexADistance.Value);
                if (VertexBDistance.HasValue) yield return (IntersectionType.VertexDistance, VertexBDistance.Value);
                if (VertexCDistance.HasValue) yield return (IntersectionType.VertexDistance, VertexCDistance.Value);
#endif

                if (EdgeABDistance.HasValue && EdgeABDistance.Value.SqLength < d)
                {
                    value = EdgeABDistance.Value;
                    it = IntersectionType.EdgeDistance;
                }

                if (EdgeBCDistance.HasValue && EdgeBCDistance.Value.SqLength < d)
                {
                    value = EdgeBCDistance.Value;
                    it = IntersectionType.EdgeDistance;
                }

                if (EdgeCADistance.HasValue && EdgeCADistance.Value.SqLength < d)
                {
                    value = EdgeCADistance.Value;
                    it = IntersectionType.EdgeDistance;
                }

                return it;
            }
        };

        public struct Status
        {
            /// <summary>
            /// Capsule clamped by radius distance from plane
            /// </summary>
            public LineSegment Clamped;

            /// <summary>
            /// capsule line intersect plane
            /// </summary>
            public Vector3 O;

            /// <summary>
            /// capsule Start project plane
            /// </summary>
            public Vector3 PS;
            /// <summary>
            /// capsule End project plane
            /// </summary>
            public Vector3 PE;

            public int CollisionCount;

            public Result Result;
        }

        // 複数コライダーのデバッグ表示のため
        // public Dictionary<VRM10SpringBoneCollider, Status> collider_status_map = new();

        // public void BeginFrame()
        // {
        //     collider_status_map.Clear();
        // }

        public static Result Collide(in Triangle t, in LineSegment capsule, float radius)
        {
            // if (collider == null)
            // {
            //     throw new ArgumentNullException("collider");
            // }
            Status status = default;

            // using (new ProfileSample("Parallel Prepare"))
            {
                // if (collider_status_map.TryGetValue(collider, out status))
                // {
                // }
                // else
                // {
                //     status = new Status();
                //     collider_status_map[collider] = status;
                // }

                float dot = default;
                // using (new ProfileSample("Dot"))
                {
                    dot = Vector3.Dot(t.Plane.normal, capsule.Vector);
                }

                if (Mathf.Abs(dot) < 1e-4)
                {
                    // using var profile = new ProfileSample("Parallel");
                    // 三角面とカプセルが平行
                    var d = t.Plane.GetDistanceToPoint(capsule.Start);
                    if (d < -radius || d > radius)
                    {
                        return default;
                    }
                    // 距離による clamp できない
                    status.Clamped = capsule;
                }
                else
                {
                    // using var profile = new ProfileSample("TryClampPlaneDistance");
                    if (capsule.TryClampPlaneDistance(t.Plane, radius, out status.Clamped, out status.O))
                    {
                    }
                    else
                    {
                        return default;
                    }
                }

                // using (new ProfileSample("ClosestPointOnPlane"))
                {
                    status.PS = t.Plane.ClosestPointOnPlane(status.Clamped.Start);
                    status.PE = t.Plane.ClosestPointOnPlane(status.Clamped.End);
                }
            }

            LineSegment? startInTriangle = default;
            LineSegment? endInTriangle = default;
            // using (new ProfileSample("InTriangle"))
            {
                if (t.IsSameSide(status.PS))
                {
                    startInTriangle = new(status.Clamped.Start, status.PS);
                    ++status.CollisionCount;
                }

                if (t.IsSameSide(status.PE))
                {
                    endInTriangle = new(status.Clamped.End, status.PE);
                    ++status.CollisionCount;
                }
            }

#if USE_VERTEX_DISTANCE
            var vertexADistance = calcVertexDistance(t.a, status.Clamped, radius);
            if (vertexADistance.HasValue)
            {
                ++status.CollisionCount;
            }
            var vertexBDistance = calcVertexDistance(t.b, status.Clamped, radius);
            if (vertexBDistance.HasValue)
            {
                ++status.CollisionCount;
            }
            var vertexCDistance = calcVertexDistance(t.c, status.Clamped, radius);
            if (vertexCDistance.HasValue)
            {
                ++status.CollisionCount;
            }
#endif

            LineSegment? edgeABDistance = default;
            LineSegment? edgeBCDistance = default;
            LineSegment? edgeCADistance = default;
            // using (new ProfileSample("EdgeDistance"))
            {
                // triangle edges
                var ab = new LineSegment(t.a, t.b);
                var bc = new LineSegment(t.b, t.c);
                var ca = new LineSegment(t.c, t.a);

                edgeABDistance = calcEdgeDistance(ab, status.Clamped, radius);
                if (edgeABDistance.HasValue)
                {
                    ++status.CollisionCount;
                }
                edgeBCDistance = calcEdgeDistance(bc, status.Clamped, radius);
                if (edgeBCDistance.HasValue)
                {
                    ++status.CollisionCount;
                }
                edgeCADistance = calcEdgeDistance(ca, status.Clamped, radius);
                if (edgeCADistance.HasValue)
                {
                    ++status.CollisionCount;
                }
            }

            status.Result = new Result
            {
                StartInTriangle = startInTriangle,
                EndInTriangle = endInTriangle,
#if USE_VERTEX_DISTANCE
                VertexADistance = vertexADistance,
                VertexBDistance = vertexBDistance,
                VertexCDistance = vertexCDistance,
#endif
                EdgeABDistance = edgeABDistance,
                EdgeBCDistance = edgeBCDistance,
                EdgeCADistance = edgeCADistance,
            };

            return status.Result;
        }

        LineSegment? calcVertexDistance(in Vector3 a, in LineSegment clamped, float radius)
        {
            var t = clamped.Project(a);
            if (t < 0 || t > 1)
            {
                return default;
            }

            var p = clamped.GetPoint(t);
            if (Vector3.Distance(a, p) > radius)
            {
                return default;
            }

            return new LineSegment(p, a);
        }

        static LineSegment? calcEdgeDistance(in LineSegment ab, in LineSegment clamped, float radius)
        {
            var (a_s, a_t) = LineSegment.CalcClosest(ab, clamped);

            // clamp range
            a_s = Mathf.Clamp(a_s, 0, 1);
            a_t = Mathf.Clamp(a_t, 0, 1);

            var ab_s = ab.GetPoint(a_s);
            var ab_t = clamped.GetPoint(a_t);

            var distance_a = (ab_s - ab_t).magnitude;
            if (distance_a > radius)
            {
                return default;
            }
            return new LineSegment(ab_t, ab_s);
        }

        Color GetGizmoColor(IntersectionType type, float color)
        {
            var g = new Color(0.3f, 0.3f, 0.3f);
            return Color.Lerp(g, type.ToColor(), color);
        }

        // public void DrawGizmos(in Triangle t, float collision, float radius)
        // {
        //     Gizmos.matrix = Matrix4x4.identity;

        //     var hit = false;
        //     foreach (var (_, status) in collider_status_map)
        //     {
        //         var type = status.Result.TryGetClosest(out var l);
        //         if (!type.HasValue)
        //         {
        //             continue;
        //         }

        //         // capsule
        //         hit = true;
        //         Gizmos.color = GetGizmoColor(type.Value, collision);
        //         t.DrawGizmos();
        //         l.DrawGizmos();
        //         Gizmos.DrawSphere(l.End, radius);
        //         Gizmos.DrawWireSphere(l.Start, radius);
        //     }

        //     if (!hit)
        //     {
        //         Gizmos.color = Color.gray;
        //         t.DrawGizmos();
        //     }
        // }
    }

    public static class IntersectionTypeExtensions
    {
        public static Color ToColor(this TriangleCapsuleCollisionSolver.IntersectionType type)
        {
            switch (type)
            {
                case TriangleCapsuleCollisionSolver.IntersectionType.PlaneInTriangle:
                    return Color.magenta;
#if USE_VERTEX_DISTANCE
                case TriangleCapsuleCollisionSolver.IntersectionType.VertexDistance:
                    return Color.blue;
#endif
                case TriangleCapsuleCollisionSolver.IntersectionType.EdgeDistance:
                    return Color.cyan;

                default:
                    throw new Exception();
            }
        }
    }
}
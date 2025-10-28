using System;
using SphereTriangle;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace UniVRM10.ClothWarp.Jobs
{
    public struct RectCollisionJob : IJobParallelFor
    {
        // cloth
        [ReadOnly] public NativeArray<(int, SpringConstraint, ClothRect)> ClothRects;
        [WriteOnly] public NativeArray<(Vector3, Vector3, Vector3, Vector3)> ClothRectResults;

        // collider
        [ReadOnly] public NativeArray<BlittableCollider> Colliders;
        [ReadOnly] public NativeArray<Matrix4x4> CurrentColliders;

        // particle
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<Vector3> NextPositions;

        // collider group
        [ReadOnly] public NativeArray<ClothInfo> Cloths;
        [ReadOnly] public NativeArray<int> ColliderGroupRef;
        [ReadOnly] public NativeArray<ArrayRange> ColliderGroup;
        [ReadOnly] public NativeArray<int> ColliderRef;

        public void Execute(int rectIndex)
        {
            var (clothGridIndex, spring, rect) = ClothRects[rectIndex];

            var a = NextPositions[rect._a];
            var b = NextPositions[rect._b];
            var c = NextPositions[rect._c];
            var d = NextPositions[rect._d];
            var aabb = GetBoundsFrom4(a, b, c, d);

            // d x-x c
            //   |/
            // a x
            var _triangle1 = new Triangle(c, d, a);
            //     x c
            //    /|
            // a x-x b
            var _triangle0 = new Triangle(a, b, c);

            var cloth = Cloths[clothGridIndex];

            for (int groupRefIndex = cloth.ColliderGroupRefRange.Start; groupRefIndex < cloth.ColliderGroupRefRange.End; ++groupRefIndex)
            {
                var groupIndex = ColliderGroupRef[groupRefIndex];

                var group = ColliderGroup[groupIndex];
                for (int colliderRefIndex = group.Start; colliderRefIndex < group.End; ++colliderRefIndex)
                {
                    var colliderIndex = ColliderRef[colliderRefIndex];
                    var collider = Colliders[colliderIndex];
                    var collider_matrix = CurrentColliders[collider.transformIndex];
                    if (!aabb.Intersects(GetBounds(collider, collider_matrix)))
                    {
                        continue;
                    }

                    // 面の片側だけにヒットさせる
                    // 行き過ぎて戻るときに素通りする
                    // var p = _triangle0.Plane.ClosestPointOnPlane(col_pos);
                    // var dot = Vector3.Dot(_triangle0.Plane.normal, col_pos - p);
                    // if (_initialColliderNormalSide[collider] * dot < 0)
                    // {
                    //     // 片側
                    //     continue;
                    // }

                    var abc = TryCollide(collider, collider_matrix, _triangle0, out var l0);
                    var cda = TryCollide(collider, collider_matrix, _triangle1, out var l1);
                    if (!Info[rect._c].TransformType.Movable())
                    {
                        // cloth の上端。cd が固定
                        if (abc)
                        {
                            a += l0.GetDelta(collider.radius);
                            b += l0.GetDelta(collider.radius);
                        }
                        else if (cda)
                        {
                            a += l1.GetDelta(collider.radius);
                            b += l1.GetDelta(collider.radius);
                        }
                    }
                    else
                    {
                        if (abc && cda)
                        {
                            a += l0.GetDelta(collider.radius);
                            b += l0.GetDelta(collider.radius);
                            c += l1.GetDelta(collider.radius);
                            d += l1.GetDelta(collider.radius);
                        }
                        else if (abc)
                        {
                            a += l0.GetDelta(collider.radius);
                            b += l0.GetDelta(collider.radius);
                            c += l0.GetDelta(collider.radius);
                        }
                        else if (cda)
                        {
                            c += l1.GetDelta(collider.radius);
                            d += l1.GetDelta(collider.radius);
                            a += l1.GetDelta(collider.radius);
                        }
                    }
                }
            }

            ClothRectResults[rectIndex] = (a, b, c, d);
        }

        static bool TryCollide(BlittableCollider collider, in Matrix4x4 colliderMatrix, in Triangle t, out LineSegment l)
        {
            var col_pos = colliderMatrix.MultiplyPoint(collider.offset);
            if (collider.colliderType == BlittableColliderType.Capsule)
            {
                // capsule
                var tail_pos = colliderMatrix.MultiplyPoint(collider.tailOrNormal);
                var result = TriangleCapsuleCollisionSolver.Collide(t, new LineSegment(col_pos, tail_pos), collider.radius);
                var type = result.TryGetClosest(out l);
                return type.HasValue;
            }
            else
            {
                // sphere
                return TryCollideSphere(t, col_pos, collider.radius, out l);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="collider"></param>
        /// <param name="radius"></param>
        /// <returns>collider => 衝突点 への線分を返す</returns>
        static bool TryCollideSphere(in Triangle triangle, in Vector3 collider, float radius, out LineSegment l)
        {
            var p = triangle.Plane.ClosestPointOnPlane(collider);
            var distance = Vector3.Distance(p, collider);
            if (distance > radius)
            {
                l = default;
                return false;
            }

            if (triangle.IsSameSide(p))
            {
                l = new LineSegment(collider, p);
                return true;
            }

            var (closestPoint, d) = triangle.GetClosest(collider);
            if (d > radius)
            {
                l = default;
                return false;
            }
            l = new LineSegment(collider, closestPoint);
            return true;
        }

        public static Bounds GetBoundsFrom4(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d)
        {
            var aabb = new Bounds(a, Vector3.zero);
            aabb.Encapsulate(b);
            aabb.Encapsulate(c);
            aabb.Encapsulate(d);
            return aabb;
        }

        public static Bounds GetBounds(BlittableCollider collider, Matrix4x4 m)
        {
            switch (collider.colliderType)
            {
                case BlittableColliderType.Capsule:
                    {
                        var h = m.MultiplyPoint(collider.offset);
                        var t = m.MultiplyPoint(collider.tailOrNormal);
                        var d = h - t;
                        var aabb = new Bounds((h + t) * 0.5f, new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z)));
                        aabb.Expand(collider.radius * 2);
                        return aabb;
                    }

                case BlittableColliderType.Sphere:
                    return new Bounds(m.MultiplyPoint(collider.offset), new Vector3(collider.radius, collider.radius, collider.radius));

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public struct RectCollisionReduceJob : IJob
    {
        [ReadOnly] public NativeArray<(int, SpringConstraint, ClothRect)> ClothRects;
        [ReadOnly] public NativeArray<(Vector3, Vector3, Vector3, Vector3)> ClothRectResults;
        [ReadOnly] public NativeArray<Vector3> NextPositions;

        public NativeArray<int> RectCollisionCount;
        public NativeArray<Vector3> RectCollisionDelta;
        public void Execute()
        {
            for (int rectIndex = 0; rectIndex < ClothRects.Length; ++rectIndex)
            {
                var (clothGridIndex, spring, rect) = ClothRects[rectIndex];
                var (a, b, c, d) = ClothRectResults[rectIndex];

                if (a != NextPositions[rect._a])
                {
                    RectCollisionDelta[rect._a] += a - NextPositions[rect._a];
                    RectCollisionCount[rect._a] += 1;
                }
                if (b != NextPositions[rect._b])
                {
                    RectCollisionDelta[rect._b] += b - NextPositions[rect._b];
                    RectCollisionCount[rect._b] += 1;
                }
                if (c != NextPositions[rect._c])
                {
                    RectCollisionDelta[rect._c] += c - NextPositions[rect._c];
                    RectCollisionCount[rect._c] += 1;
                }
                if (d != NextPositions[rect._d])
                {
                    RectCollisionDelta[rect._d] += d - NextPositions[rect._d];
                    RectCollisionCount[rect._d] += 1;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace SphereTriangle
{
    public struct ClothRect
    {
        // ２枚の三角形
        // abc
        // cda
        // に対する衝突(球 or カプセル)を管理する
        public readonly int _a;
        public readonly int _b;
        public readonly int _c;
        public readonly int _d;

        /// <summary>
        /// two triangles
        /// d x-x c
        ///   |/|
        /// a x-x b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public ClothRect(
            int a, int b, int c, int d)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
        }
    }

    public class ClothRectCollision
    {
        Triangle _triangle0;
        float _trinagle0Collision;
        Triangle _triangle1;
        float _triangle1Collision;

        // TriangleCapsuleCollisionSolver _s0 = new();
        // TriangleCapsuleCollisionSolver _s1 = new();

        // 各コライダーが初期姿勢で三角形ABCの法線の正か負のどちらにあるのかを記録する
        Dictionary<VRM10SpringBoneCollider, float> _initialColliderNormalSide = new();

        public void InitializeColliderSide(PositionList list, IReadOnlyList<VRM10SpringBoneColliderGroup> colliderGroups, ClothRect _rect)
        {
            var a = list.Get(_rect._a);
            var b = list.Get(_rect._b);
            var c = list.Get(_rect._c);
            var d = list.Get(_rect._d);

            //     x c
            //    /|
            // a x-x b
            var t = new Triangle(a, b, c);

            foreach (var g in colliderGroups)
            {
                foreach (var collider in g.Colliders)
                {
                    var headWorldPosition = collider.transform.TransformPoint(collider.Offset);
                    var p = t.Plane.ClosestPointOnPlane(headWorldPosition);
                    var dot = Vector3.Dot(t.Plane.normal, headWorldPosition - p);
                    _initialColliderNormalSide[collider] = dot;
                }
            }
        }

        public void Collide(PositionList list, IList<VRM10SpringBoneCollider> colliders, ClothRect _rect)
        {
            // using (new ProfileSample("Rect: Prepare"))
            {
                // _s0.BeginFrame();
                // _s1.BeginFrame();

                var a = list.Get(_rect._a);
                var b = list.Get(_rect._b);
                var c = list.Get(_rect._c);
                var d = list.Get(_rect._d);

                // d x-x c
                //   |/
                // a x
                _triangle1 = new Triangle(c, d, a);
                _triangle1Collision -= 0.1f;
                if (_triangle1Collision < 0)
                {
                    _triangle1Collision = 0;
                }
                //     x c
                //    /|
                // a x-x b
                _triangle0 = new Triangle(a, b, c);
                _trinagle0Collision -= 0.1f;
                if (_trinagle0Collision < 0)
                {
                    _trinagle0Collision = 0;
                }
            }

            // using (new ProfileSample("Rect: Collide"))
            {
                var aabb = list.GetBounds(_rect);

                for (int i = 0; i < colliders.Count; ++i)
                {
                    var collider = colliders[i];
                    // using (new ProfileSample("EaryOut"))
                    {
                        if (!aabb.Intersects(GetBounds(collider)))
                        {
                            continue;
                        }

                        var headWorldPosition = collider.transform.TransformPoint(collider.Offset);
                        var p = _triangle0.Plane.ClosestPointOnPlane(headWorldPosition);
                        var dot = Vector3.Dot(_triangle0.Plane.normal, headWorldPosition - p);
                        if (_initialColliderNormalSide[collider] * dot < 0)
                        {
                            // 片側
                            continue;
                        }
                    }

                    if (TryCollide(collider, _triangle0, out var l0))
                    {
                        _trinagle0Collision = 1.0f;
                        list.CollisionMove(_rect._a, l0, collider.Radius);
                        list.CollisionMove(_rect._b, l0, collider.Radius);
                        list.CollisionMove(_rect._c, l0, collider.Radius);
                    }
                    if (TryCollide(collider, _triangle1, out var l1))
                    {
                        _triangle1Collision = 1.0f;
                        list.CollisionMove(_rect._c, l1, collider.Radius);
                        list.CollisionMove(_rect._d, l1, collider.Radius);
                        list.CollisionMove(_rect._a, l1, collider.Radius);
                    }
                }
            }
        }

        static Bounds GetBounds(VRM10SpringBoneCollider c)
        {
            var headWorldPosition = c.transform.TransformPoint(c.Offset);
            switch (c.ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Capsule:
                    {
                        var h = headWorldPosition;
                        var t = c.transform.TransformPoint(c.TailOrNormal);
                        var d = h - t;
                        var aabb = new Bounds((h + t) * 0.5f, new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z)));
                        aabb.Expand(c.Radius * 2);
                        return aabb;
                    }

                case VRM10SpringBoneColliderTypes.Sphere:
                    return new Bounds(headWorldPosition, new Vector3(c.Radius, c.Radius, c.Radius));

                default:
                    throw new NotImplementedException();
            }
        }


        /// <summary>
        /// 衝突して移動デルタを得る
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="t"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        static bool TryCollide(VRM10SpringBoneCollider collider, in Triangle t, out LineSegment l)
        {
            var headWorldPosition = collider.transform.TransformPoint(collider.Offset);
            if (collider.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
            {
                // capsule
                var tailWorldPosition = collider.transform.TransformPoint(collider.TailOrNormal);
                var result = TriangleCapsuleCollisionSolver.Collide(t, new(headWorldPosition, tailWorldPosition), collider.Radius);
                var type = result.TryGetClosest(out l);
                return type.HasValue;
            }
            else
            {
                // sphere
                // using var profile = new ProfileSample("Sphere");
                return TryCollideSphere(t, headWorldPosition, collider.Radius, out l);
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

        //         public void DrawGizmos()
        //         {
        //             var r = Vector3.Distance(_triangle0.b, _triangle0.c) * 0.1f;
        //             _DrawGizmos(_triangle0, _s0, _trinagle0Collision, r);
        //             _DrawGizmos(_triangle1, _s1, _triangle1Collision, r);

        // #if AABB_DEBUG
        //             Gizmos.matrix = Matrix4x4.identity;
        //             Gizmos.color = Color.cyan;
        //             var aabb = GetBoundsFrom4(_triangle0.a, _triangle0.b, _triangle1.a, _triangle1.b);
        //             Gizmos.DrawWireCube(aabb.center, aabb.size);
        // #endif
        //         }

        // void _DrawGizmos(in Triangle t, TriangleCapsuleCollisionSolver solver, float collision, float radius)
        // {
        //     solver.DrawGizmos(t, collision, radius);
        // }
    }
}
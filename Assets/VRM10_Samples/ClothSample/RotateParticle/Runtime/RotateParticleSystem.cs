using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RotateParticle.Components;
using SphereTriangle;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UniVRM10;


namespace RotateParticle
{
    public class RotateParticleSystem : IRotateParticleSystem
    {
        public SimulationEnv Env = new()
        {
            DragForce = 0.6f,
            Stiffness = 0.07f,
        };

        readonly List<WarpRoot> _warps = new();
        readonly List<RectCloth> _cloths = new();

        public List<VRM10SpringBoneColliderGroup> _colliderGroups = new();

        public float _clothFactor = 0.5f;

        // runtime
        public List<Strand> _strands = new List<Strand>();
        public ParticleList _list = new();
        public List<(SpringConstraint, ClothRect)> _clothRects = new();
        public List<ClothRectCollision> _clothRectCollisions = new();

        public PositionList _newPos;
        Vector3[] _restPositions;

        static Color[] Colors = new Color[]
        {
            Color.yellow,
            Color.green,
            Color.magenta,
        };

        Color GetGizmoColor(VRM10SpringBoneColliderGroup g)
        {
            for (int i = 0; i < _colliderGroups.Count; ++i)
            {
                if (_colliderGroups[i] == g)
                {
                    return Colors[i];
                }
            }

            return Color.gray;
        }

        HashSet<int> _clothUsedParticles = new();

        void IDisposable.Dispose()
        {
        }

        public void InitializeCloth(
            RectCloth cloth,
            ParticleList list,
            Dictionary<WarpRoot, Strand> strandMap)
        {
            for (int i = 1; i < cloth.Warps.Count; ++i)
            {
                var s0 = strandMap[cloth.Warps[i - 1]];
                var s1 = strandMap[cloth.Warps[i]];
                for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                {
                    // d x x c
                    //   | |
                    // a x-x b
                    var a = s0.Particles[j];
                    var b = s1.Particles[j];
                    var c = s1.Particles[j - 1];
                    var d = s0.Particles[j - 1];
                    _clothUsedParticles.Add(a.Init.Index);
                    _clothUsedParticles.Add(b.Init.Index);
                    _clothUsedParticles.Add(c.Init.Index);
                    _clothUsedParticles.Add(d.Init.Index);
                    if (i % 2 == 1)
                    {
                        // 互い違いに
                        // abcd to badc
                        (a, b) = (b, a);
                        (c, d) = (d, c);
                    }
                    _clothRects.Add((
                        new SpringConstraint(
                            a.Init.Index,
                            b.Init.Index,
                            Vector3.Distance(
                                list._particles[a.Init.Index].State.Current,
                                list._particles[b.Init.Index].State.Current)),
                        new ClothRect(
                            a.Init.Index, b.Init.Index, c.Init.Index, d.Init.Index)));
                    _clothRectCollisions.Add(new());
                }
            }

            if (cloth.Warps.Count >= 3 && cloth.LoopIsClosed)
            {
                // close loop
                var i = cloth.Warps.Count;
                var s0 = strandMap[cloth.Warps.Last()];
                var s1 = strandMap[cloth.Warps.First()];
                for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                {
                    var a = s0.Particles[j];
                    var b = s1.Particles[j];
                    var c = s1.Particles[j - 1];
                    var d = s0.Particles[j - 1];
                    _clothUsedParticles.Add(a.Init.Index);
                    _clothUsedParticles.Add(b.Init.Index);
                    _clothUsedParticles.Add(c.Init.Index);
                    _clothUsedParticles.Add(d.Init.Index);
                    if (i % 2 == 1)
                    {
                        // 互い違いに
                        // abcd to badc
                        (a, b) = (b, a);
                        (c, d) = (d, c);
                    }
                    _clothRects.Add((
                        new SpringConstraint(
                            a.Init.Index,
                            b.Init.Index,
                            Vector3.Distance(
                                list._particles[a.Init.Index].State.Current,
                                list._particles[b.Init.Index].State.Current)
                            ),
                        new ClothRect(
                            a.Init.Index, b.Init.Index, c.Init.Index, d.Init.Index
                        )
                    ));
                    _clothRectCollisions.Add(new());
                }
            }
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            var strandMap = new Dictionary<WarpRoot, Strand>();
            var warps = vrm.GetComponentsInChildren<WarpRoot>();
            foreach (var warp in warps)
            {
                var strands = new List<Strand>();
                var strand = _list.MakeParticleStrand(Env, warp);
                strands.Add(strand);
                _strands.AddRange(strands);
                strandMap.Add(warp, strand);

                foreach (var g in warp.ColliderGroups)
                {
                    foreach (var c in g.Colliders)
                    {
                        AddColliderIfNotExists(g.name, c);
                    }
                }
            }

            var cloths = vrm.GetComponentsInChildren<RectCloth>();
            foreach (var cloth in cloths)
            {
                InitializeCloth(cloth, _list, strandMap);
            }

            _newPos = new(_list._particles.Count);
            _list.EndInitialize(_newPos.Init);
            _restPositions = new Vector3[_list._particles.Count];
            _newPos.EndInitialize();

            for (int i = 0; i < _clothRects.Count; ++i)
            {
                var (s, r) = _clothRects[i];
                var c = _clothRectCollisions[i];
                c.InitializeColliderSide(_newPos, _colliderGroups, r);
            }

            await awaitCaller.NextFrame();
        }

        /// <summary>
        /// すべての Particle を Init 状態にする。
        /// Verlet の Prev を現在地に更新する(速度0)。
        /// </summary>
        /// 
        void IRotateParticleSystem.ResetInitialRotation()
        {
            foreach (var strand in _strands)
            {
                strand.Reset(_list._particleTransforms);
            }
            // foreach (var p in _system._list._particleTransforms)
            // {
            //     p.transform.localRotation = _vrm.DefaultTransformStates[p.transform].LocalRotation;
            // }
        }


        void IRotateParticleSystem.Process(float deltaTime)
        {
            using var profile = new ProfileSample("RotateParticle");

            _newPos.BoundsCache.Clear();

            using (new ProfileSample("UpdateRoot"))
            {
                //
                // input
                //
                // 各strandのrootの移動と回転を外部から入力する。
                // それらを元に各 joint の方向を元に戻した場合の戻り位置を計算する
                foreach (var strand in _strands)
                {
                    strand.UpdateRoot(_list._particleTransforms, _newPos, _restPositions);
                }
            }

            using (new ProfileSample("Verlet"))
            {
                //
                // particle simulation
                //
                // verlet 積分
                var time = new FrameTime(deltaTime);
                _list.BeginFrame(Env, time, _restPositions);
                foreach (var (spring, collision) in _clothRects)
                {
                    // cloth constraint
                    spring.Resolve(time, _clothFactor, _list._particles);
                }
                _list.Verlet(Env, time, _newPos.Init);
                // 長さで拘束
                foreach (var strand in _strands)
                {
                    strand.ForceLength(_list._particleTransforms, _newPos);
                }
            }

            // collision
            using (new ProfileSample("Collision"))
            {
                for (int i = 0; i < _colliderGroups.Count; ++i)
                {
                    var g = _colliderGroups[i];

                    for (int j = 0; j < _clothRects.Count; ++j)
                    {
                        var (spring, rect) = _clothRects[j];
                        var collision = _clothRectCollisions[j];
                        // using var prof = new ProfileSample("Collision: Cloth");
                        // 頂点 abcd は同じ CollisionMask
                        // TODO:
                        // if (_list._particles[rect._a].Init.CollisionMask.HasFlag((CollisionGroupMask)(i + 1)))
                        {
                            // cloth
                            collision.Collide(_newPos, g.Colliders, rect);
                        }
                    }

                    for (int j = 0; j < _list._particles.Count; ++j)
                    {
                        // using var prof = new ProfileSample("Collision: Strand");
                        if (_clothUsedParticles.Contains(j))
                        {
                            // 布で処理された
                            continue;
                        }

                        var particle = _list._particles[j];
                        if (particle.Init.Mass == 0)
                        {
                            continue;
                        }

                        // 紐の当たり判定
                        // TODO:
                        // if (particle.Init.CollisionMask.HasFlag((CollisionGroupMask)(i + 1)))
                        {
                            var p = _newPos.Get(j);
                            foreach (var c in g.Colliders)
                            {
                                // strand
                                if (c != null && TryCollide(c, p, particle.Init.Radius, out var resolved))
                                {
                                    _newPos.CollisionMove(particle.Init.Index, resolved, c.Radius);
                                }
                            }
                        }
                    }
                }
            }

            using (new ProfileSample("Apply"))
            {
                for (int i = 0; i < _newPos.CollisionCount.Length; ++i)
                {
                    if (_newPos.CollisionCount[i] > 0)
                    {
                        _list._particles[i].HasCollide = true;
                    }
                }
                var result = _newPos.Resolve();
                //
                // apply result
                //
                // apply positions and
                // calc rotation from positions recursive
                foreach (var strand in _strands)
                {
                    strand.Apply(_list._particleTransforms, result);
                }
            }
        }

        public VRM10SpringBoneColliderGroup GetOrAddColliderGroup(string groupName, GameObject go)
        {
            foreach (var g in _colliderGroups)
            {
                if (g.Name == groupName)
                {
                    return g;
                }
            }

            var group = go.GetOrAddComponent<VRM10SpringBoneColliderGroup>();
            _colliderGroups.Add(group);
            return group;
        }

        void AddColliderIfNotExists(string groupName,
           VRM10SpringBoneCollider c)
        {
            var group = GetOrAddColliderGroup(groupName, c.gameObject);

            foreach (var collider in group.Colliders)
            {
                if (collider == c)
                {
                    return;
                }
            }

            // c.GizmoColor = GetGizmoColor(group);
            group.Colliders.Add(c);
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
        public bool TryCollide(UniVRM10.VRM10SpringBoneCollider c, in Vector3 p, float radius, out LineSegment resolved)
        {
            if (c.ColliderType == UniVRM10.VRM10SpringBoneColliderTypes.Capsule)
            {
                return TryCollideCapsuleAndSphere(c.HeadWorldPosition, c.TailWorldPosition, c.Radius, p, radius, out resolved);
            }
            else
            {
                return TryCollideSphereAndSphere(c.HeadWorldPosition, c.Radius, p, radius, out resolved);
            }
        }

        void IRotateParticleSystem.DrawGizmos()
        {
            _list.DrawGizmos();

            for (int i = 0; i < _clothRects.Count; ++i)
            {
                // var (spring, rect) = _clothRects[i];
                var collision = _clothRectCollisions[i];
                collision.DrawGizmos();
            }

            if (_newPos != null)
            {
                _newPos.DrawGizmos();
            }
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            throw new NotImplementedException();
        }
    }
}
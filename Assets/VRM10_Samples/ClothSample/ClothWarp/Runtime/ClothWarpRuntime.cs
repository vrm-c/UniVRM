using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniVRM10.ClothWarp.Components;
using SphereTriangle;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;


namespace UniVRM10.ClothWarp
{
    /// <summary>
    /// プロトタイプ。非 job
    /// </summary>
    public class ClothWarpRuntime : IVrm10SpringBoneRuntime
    {
        Vrm10Instance _vrm;
        Action<Vrm10Instance> _onInit;
        bool _initialized = false;
        bool _building = false;

        public SimulationEnv Env = new()
        {
            DragForce = 0.6f,
            Stiffness = 0.07f,
        };

        public List<VRM10SpringBoneColliderGroup> _colliderGroups = new();

        public float _clothFactor = 0.5f;

        // runtime
        public List<Strand> _strands = new List<Strand>();
        public ParticleList _list = new();

        ClothRectList _clothRects;
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

        public Task InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _building = true;
            _vrm = vrm;

            if (_onInit != null)
            {
                _onInit(vrm);
                _onInit = null;
            }

            _initialized = false;
            var strandMap = new Dictionary<Components.ClothWarpRoot, Strand>();
            var warps = vrm.GetComponentsInChildren<Components.ClothWarpRoot>();
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
                        if (c != null)
                        {
                            AddColliderIfNotExists(g.name, c);
                        }
                    }
                }
            }

            _clothRects = new ClothRectList(_list._particleTransforms, vrm);

            _newPos = new(_list._particles.Count);
            _list.EndInitialize(_newPos.Init);
            _restPositions = new Vector3[_list._particles.Count];
            _newPos.EndInitialize();

            _clothRectCollisions = new();
            for (int i = 0; i < _clothRects.List.Count; ++i)
            {
                var (grid, s, r) = _clothRects.List[i];
                _clothRectCollisions.Add(new());
                var c = _clothRectCollisions.Last();
                c.InitializeColliderSide(_newPos, _colliderGroups, r);
            }

            // await awaitCaller.NextFrame();

            _initialized = true;
            _building = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// すべての Particle を Init 状態にする。
        /// Verlet の Prev を現在地に更新する(速度0)。
        /// </summary>
        public void RestoreInitialTransform()
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

        public void Process()
        {
            Process(Time.deltaTime);
        }

        void Process(float deltaTime)
        {
            if (!_initialized)
            {
                return;
            }

            using var profile = new ProfileSample("ClothWarp");

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
                foreach (var (gridIndex, spring, collision) in _clothRects.List)
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

                    for (int j = 0; j < _clothRects.List.Count; ++j)
                    {
                        var (gridIndex, spring, rect) = _clothRects.List[j];
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
                        if (_clothRects.ClothUsedParticles[j])
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
                if (collider == null)
                {
                    continue;
                }
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
        public bool TryCollide(VRM10SpringBoneCollider c, in Vector3 p, float radius, out LineSegment resolved)
        {
            var headWorldPosition = c.transform.TransformPoint(c.Offset);
            if (c.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
            {
                var tailWorldPosition = c.transform.TransformPoint(c.TailOrNormal);
                return TryCollideCapsuleAndSphere(headWorldPosition, tailWorldPosition, c.Radius, p, radius, out resolved);
            }
            else
            {
                return TryCollideSphereAndSphere(headWorldPosition, c.Radius, p, radius, out resolved);
            }
        }

        void IVrm10SpringBoneRuntime.DrawGizmos()
        {
            _list.DrawGizmos();

            for (int i = 0; i < _clothRectCollisions.Count; ++i)
            {
                // var (spring, rect) = _clothRects[i];
                var collision = _clothRectCollisions[i];
                // collision.DrawGizmos();
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

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
        }

        public ClothWarpRuntime(Action<Vrm10Instance> onInit = null)
        {
            _onInit = onInit;
        }

        public void Dispose()
        {
        }

        public bool ReconstructSpringBone()
        {
            if (_vrm == null)
            {
                return false;
            }
            if (_building)
            {
                return false;
            }
            var task = InitializeAsync(_vrm, new ImmediateCaller());
            task.Wait();
            return true;
        }
    }
}
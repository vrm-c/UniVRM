using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RotateParticle.Components;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UniVRM10;


namespace RotateParticle.Jobs
{
    public class RotateParticleJobSystem : IRotateParticleSystem
    {
        Vrm10Instance _vrm;

        List<Transform> _colliderTransforms;
        TransformAccessArray _colliderTransformAccessArray;
        NativeArray<Matrix4x4> _currentColliders;
        NativeArray<BlittableCollider> _colliders;

        List<Transform> _transforms;
        TransformAccessArray _transformAccessArray;
        NativeArray<TransformData> _inputData;
        NativeArray<TransformInfo> _info;
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _prevPositions;
        NativeArray<Vector3> _nextPositions;
        NativeArray<Quaternion> _nextRotations;

        NativeArray<WarpInfo> _warps;

        //
        // cloth
        //
        NativeArray<bool> _clothUsedParticles;
        NativeArray<(SpringConstraint, SphereTriangle.ClothRect)> _clothRects;

        void IDisposable.Dispose()
        {
            if (_colliderTransformAccessArray.isCreated) _colliderTransformAccessArray.Dispose();
            if (_currentColliders.IsCreated) _currentColliders.Dispose();

            if (_warps.IsCreated) _warps.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_inputData.IsCreated) _inputData.Dispose();
            if (_info.IsCreated) _info.Dispose();
            if (_currentPositions.IsCreated) _currentPositions.Dispose();
            if (_prevPositions.IsCreated) _prevPositions.Dispose();
            if (_nextPositions.IsCreated) _nextPositions.Dispose();
            if (_nextRotations.IsCreated) _nextRotations.Dispose();

            if (_clothUsedParticles.IsCreated) _clothUsedParticles.Dispose();
            if (_clothRects.IsCreated) _clothRects.Dispose();
        }

        (int index, bool isNew) GetTransformIndex(Transform t,
            TransformInfo info,
            List<TransformInfo> infos,
            List<Vector3> positions)
        {
            Debug.Assert(t != null);
            var i = _transforms.IndexOf(t);
            if (i != -1)
            {
                return (i, false);
            }

            i = _transforms.Count;
            _transforms.Add(t);
            infos.Add(info);
            positions.Add(t.position);
            return (i, true);
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;

            //
            // colliders
            //
            _colliderTransforms = new();
            List<BlittableCollider> colliders = new();
            foreach (var collider in vrm.GetComponentsInChildren<VRM10SpringBoneCollider>())
            {
                colliders.Add(new BlittableCollider
                {
                    offset = collider.Offset,
                    radius = collider.Radius,
                    tailOrNormal = collider.TailOrNormal,
                    colliderType = TranslateColliderType(collider.ColliderType)
                });
                _colliderTransforms.Add(collider.transform);
            }
            _colliderTransformAccessArray = new(_colliderTransforms.ToArray(), 128);
            _colliders = new(colliders.ToArray(), Allocator.Persistent);
            _currentColliders = new(_colliderTransforms.Count, Allocator.Persistent);

            //
            // warps
            //
            _transforms = new();
            List<TransformInfo> info = new();
            List<Vector3> positions = new();
            List<WarpInfo> warps = new();
            var warpSrcs = vrm.GetComponentsInChildren<WarpRoot>();
            for (int warpIndex = 0; warpIndex < warpSrcs.Length; ++warpIndex)
            {
                var warp = warpSrcs[warpIndex];
                var start = _transforms.Count;

                if (warp.Center != null)
                {
                    GetTransformIndex(warp.Center, new TransformInfo
                    {
                        TransformType = TransformType.Center
                    }, info, positions);
                    start += 1;
                }

                var warpRootParentTransformIndex = GetTransformIndex(warp.transform.parent, new TransformInfo
                {
                    TransformType = TransformType.WarpRootParent
                }, info, positions);
                Debug.Assert(warpRootParentTransformIndex.index != -1);
                if (warpRootParentTransformIndex.isNew)
                {
                    start += 1;
                }

                var warpRootTransformIndex = GetTransformIndex(warp.transform, new TransformInfo
                {
                    TransformType = TransformType.WarpRoot,
                    ParentIndex = warpRootParentTransformIndex.index,
                    InitLocalPosition = vrm.DefaultTransformStates[warp.transform].LocalPosition,
                    InitLocalRotation = vrm.DefaultTransformStates[warp.transform].LocalRotation,
                }, info, positions);
                Debug.Assert(warpRootTransformIndex.index != -1);
                Debug.Assert(warpRootTransformIndex.isNew);

                var parentIndex = warpRootTransformIndex.index;
                foreach (var particle in warp.Particles)
                {
                    if (particle.Transform != null && particle.Mode != WarpRoot.ParticleMode.Disabled)
                    {
                        var outputParticleTransformIndex = GetTransformIndex(particle.Transform, new TransformInfo
                        {
                            TransformType = TransformType.Particle,
                            ParentIndex = parentIndex,
                            InitLocalPosition = vrm.DefaultTransformStates[particle.Transform].LocalPosition,
                            InitLocalRotation = vrm.DefaultTransformStates[particle.Transform].LocalRotation,
                            Settings = particle.Settings,
                        }, info, positions);
                        parentIndex = outputParticleTransformIndex.index;
                    }
                }

                warps.Add(new WarpInfo
                {
                    StartIndex = start,
                    EndIndex = _transforms.Count,
                });

                await awaitCaller.NextFrame();
            }
            _warps = new(warps.ToArray(), Allocator.Persistent);
            _transformAccessArray = new(_transforms.ToArray(), 128);
            _inputData = new(_transforms.Count, Allocator.Persistent);
            var pos = positions.ToArray();
            _currentPositions = new(pos, Allocator.Persistent);
            _prevPositions = new(pos, Allocator.Persistent);
            _nextPositions = new(pos.Length, Allocator.Persistent);
            _nextRotations = new(pos.Length, Allocator.Persistent);
            _info = new(info.ToArray(), Allocator.Persistent);

            //
            // cloths
            //
            List<(SpringConstraint, SphereTriangle.ClothRect)> clothRects = new();
            _clothUsedParticles = new(_transforms.Count, Allocator.Persistent);
            var cloths = vrm.GetComponentsInChildren<RectCloth>();
            foreach (var cloth in cloths)
            {
                for (int i = 1; i < cloth.Warps.Count; ++i)
                {
                    var s0 = cloth.Warps[i - 1];
                    var s1 = cloth.Warps[i];
                    for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                    {
                        // d x x c
                        //   | |
                        // a x-x b
                        var a = s0.Particles[j];
                        var b = s1.Particles[j];
                        var c = s1.Particles[j - 1];
                        var d = s0.Particles[j - 1];
                        _clothUsedParticles[_transforms.IndexOf(a.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(b.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(c.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(d.Transform)] = true;
                        if (i % 2 == 1)
                        {
                            // 互い違いに
                            // abcd to badc
                            (a, b) = (b, a);
                            (c, d) = (d, c);
                        }
                        clothRects.Add((
                            new SpringConstraint(
                                _transforms.IndexOf(a.Transform),
                                _transforms.IndexOf(b.Transform),
                                Vector3.Distance(
                                    vrm.DefaultTransformStates[a.Transform].Position,
                                    vrm.DefaultTransformStates[b.Transform].Position)),
                            new SphereTriangle.ClothRect(
                                _transforms.IndexOf(a.Transform),
                                _transforms.IndexOf(b.Transform),
                                _transforms.IndexOf(c.Transform),
                                _transforms.IndexOf(d.Transform))));
                    }
                }

                if (cloth.Warps.Count >= 3 && cloth.LoopIsClosed)
                {
                    // close loop
                    var i = cloth.Warps.Count;
                    var s0 = cloth.Warps.Last();
                    var s1 = cloth.Warps.First();
                    for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                    {
                        var a = s0.Particles[j];
                        var b = s1.Particles[j];
                        var c = s1.Particles[j - 1];
                        var d = s0.Particles[j - 1];
                        _clothUsedParticles[_transforms.IndexOf(a.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(b.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(c.Transform)] = true;
                        _clothUsedParticles[_transforms.IndexOf(d.Transform)] = true;
                        if (i % 2 == 1)
                        {
                            // 互い違いに
                            // abcd to badc
                            (a, b) = (b, a);
                            (c, d) = (d, c);
                        }
                        clothRects.Add((
                            new SpringConstraint(
                                _transforms.IndexOf(a.Transform),
                                _transforms.IndexOf(b.Transform),
                                Vector3.Distance(
                                    vrm.DefaultTransformStates[a.Transform].Position,
                                    vrm.DefaultTransformStates[b.Transform].Position)
                                ),
                            new SphereTriangle.ClothRect(
                                _transforms.IndexOf(a.Transform),
                                _transforms.IndexOf(b.Transform),
                                _transforms.IndexOf(c.Transform),
                                _transforms.IndexOf(d.Transform)
                            )
                        ));
                    }
                }
            }
            _clothRects = new(clothRects.ToArray(), Allocator.Persistent);
        }

        private static BlittableColliderType TranslateColliderType(VRM10SpringBoneColliderTypes colliderType)
        {
            switch (colliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    return BlittableColliderType.Sphere;
                case VRM10SpringBoneColliderTypes.Capsule:
                    return BlittableColliderType.Capsule;
                case VRM10SpringBoneColliderTypes.Plane:
                    return BlittableColliderType.Plane;
                case VRM10SpringBoneColliderTypes.SphereInside:
                    return BlittableColliderType.SphereInside;
                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    return BlittableColliderType.CapsuleInside;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void IRotateParticleSystem.Process(float deltaTime)
        {
            var frame = new FrameInfo(deltaTime, Vector3.zero);

            JobHandle handle = default;

            // input
            handle = new InputColliderJob
            {
                CurrentCollider = _currentColliders,
            }.Schedule(_colliderTransformAccessArray, handle);

            handle = new InputTransformJob
            {
                Info = _info,
                InputData = _inputData,
                CurrentPositions = _currentPositions,
            }.Schedule(_transformAccessArray, handle);

            // verlet
            handle = new VerletJob
            {
                Frame = frame,
                Info = _info,
                CurrentTransforms = _inputData,
                PrevPositions = _prevPositions,
                CurrentPositions = _currentPositions,
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Schedule(_info.Length, 128, handle);

            // 親子の長さで拘束
            handle = new ParentLengthConstraintJob
            {
                Warps = _warps,
                Info = _info,
                NextPositions = _nextPositions,
            }.Schedule(_warps.Length, 16, handle);

            // collision
            handle = new CollisionJob
            {
                Colliders = _colliders,
                CurrentColliders = _currentColliders,
                Info = _info,
                NextPositions = _nextPositions,
                ClothUsedParticles = _clothUsedParticles,
                ClothRects = _clothRects,
            }.Schedule(_colliders.Length, 128, handle);

            // NextPositions から NextRotations を作る
            handle = new ApplyRotationJob
            {
                Warps = _warps,
                Info = _info,
                CurrentTransforms = _inputData,
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Schedule(_warps.Length, 16, handle);

            // output
            handle = new OutputTransformJob
            {
                Info = _info,
                NextRotations = _nextRotations,
            }.Schedule(_transformAccessArray, handle);

            handle.Complete();

            // update state
            NativeArray<Vector3>.Copy(_currentPositions, _prevPositions);
            NativeArray<Vector3>.Copy(_nextPositions, _currentPositions);
        }

        void IRotateParticleSystem.ResetInitialRotation()
        {
            foreach (var warp in _warps)
            {
                for (int i = warp.StartIndex; i < warp.EndIndex; ++i)
                {
                    var p = _info[i];
                    var t = _transforms[i];
                    switch (p.TransformType)
                    {
                        case TransformType.Particle:
                            t.localRotation = _vrm.DefaultTransformStates[t].LocalRotation;
                            _currentPositions[i] = t.position;
                            _prevPositions[i] = t.position;
                            _nextPositions[i] = t.position;
                            break;
                    }
                }
            }
        }

        void IRotateParticleSystem.DrawGizmos()
        {
            for (int i = 0; i < _info.Length; ++i)
            {
                var info = _info[i];
                var v = _currentPositions[i];
                switch (info.TransformType)
                {
                    case TransformType.Center:
                    case TransformType.WarpRootParent:
                        break;
                    case TransformType.WarpRoot:
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(v, info.Settings.radius);
                        break;
                    case TransformType.Particle:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(v, info.Settings.radius);
                        break;
                }
            }
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            var i = _transforms.IndexOf(joint);
            if (i != -1)
            {
                var info = _info[i];
                info.Settings = jointSettings;
                _info[i] = info;
            }
        }
    }
}
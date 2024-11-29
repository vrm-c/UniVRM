using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UniVRM10;


namespace UniVRM10.ClothWarp.Jobs
{
    /// <summary>
    /// Job 版
    /// </summary>
    public class ClothWarpJobRuntime : IVrm10SpringBoneRuntime
    {
        Vrm10Instance _vrm;
        Action<Vrm10Instance> _onInit;
        bool _building = false;

        //
        // collider
        //
        List<Transform> _colliderTransforms;
        TransformAccessArray _colliderTransformAccessArray;
        NativeArray<Matrix4x4> _currentColliders;
        NativeArray<BlittableCollider> _colliders;

        //
        // particle
        //
        List<Transform> _transforms;
        TransformAccessArray _transformAccessArray;
        NativeArray<TransformData> _inputData;
        NativeArray<TransformInfo> _info;
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _prevPositions;
        NativeArray<Vector3> _nextPositions;
        NativeArray<Quaternion> _nextRotations;

        NativeArray<Vector3> _strandCollision;
        NativeArray<int> _clothCollisionCount;
        NativeArray<Vector3> _clothCollisionDelta;
        NativeArray<Vector3> _forces;

        //
        // warp
        //
        NativeArray<WarpInfo> _warps;

        //
        // cloth
        //
        NativeArray<bool> _clothUsedParticles;
        NativeArray<(SpringConstraint, SphereTriangle.ClothRect)> _clothRects;

        public void Dispose()
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
            if (_strandCollision.IsCreated) _strandCollision.Dispose();
            if (_clothCollisionCount.IsCreated) _clothCollisionCount.Dispose();
            if (_clothCollisionDelta.IsCreated) _clothCollisionDelta.Dispose();
            if (_forces.IsCreated) _forces.Dispose();

            if (_warps.IsCreated) _warps.Dispose();

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

        public async Task InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;
            _building = true;

            if (_onInit != null)
            {
                _onInit(vrm);
                _onInit = null;
            }

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
            // warps => particles
            //
            _transforms = new();
            List<TransformInfo> info = new();
            List<Vector3> positions = new();
            List<WarpInfo> warps = new();
            var warpSrcs = vrm.GetComponentsInChildren<Components.ClothWarpRoot>();
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
                    TransformType = TransformType.ClothWarp,
                    ParentIndex = warpRootParentTransformIndex.index,
                    InitLocalPosition = vrm.DefaultTransformStates[warp.transform].LocalPosition,
                    InitLocalRotation = vrm.DefaultTransformStates[warp.transform].LocalRotation,
                }, info, positions);
                Debug.Assert(warpRootTransformIndex.index != -1);
                Debug.Assert(warpRootTransformIndex.isNew);

                var parentIndex = warpRootTransformIndex.index;
                foreach (var particle in warp.Particles)
                {
                    if (particle.Transform != null && particle.Mode != Components.ClothWarpRoot.ParticleMode.Disabled)
                    {
                        var outputParticleTransformIndex = GetTransformIndex((Transform)particle.Transform, new TransformInfo
                        {
                            TransformType = TransformType.Particle,
                            ParentIndex = parentIndex,
                            InitLocalPosition = vrm.DefaultTransformStates[(Transform)particle.Transform].LocalPosition,
                            InitLocalRotation = vrm.DefaultTransformStates[(Transform)particle.Transform].LocalRotation,
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
            _strandCollision = new(pos.Length, Allocator.Persistent);
            _clothCollisionCount = new(pos.Length, Allocator.Persistent);
            _clothCollisionDelta = new(pos.Length, Allocator.Persistent);
            _forces = new(pos.Length, Allocator.Persistent);

            //
            // cloths
            //
            var clothRects = new ClothRectList(_transforms, vrm);
            _clothRects = new(clothRects.List.ToArray(), Allocator.Persistent);
            _clothUsedParticles = new(clothRects.ClothUsedParticles, Allocator.Persistent);
            _building = false;
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

        public void Process()
        {
            Process(Time.deltaTime);
        }

        public void Process(float deltaTime)
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
                Forces = _forces,

                CollisionCount = _clothCollisionCount,
                CollisionDelta = _clothCollisionDelta,
            }.Schedule(_transformAccessArray, handle);

            // spring(cloth weft)
            handle = new WeftConstraintJob
            {
                ClothRects = _clothRects,
                CurrentPositions = _currentPositions,

                Force = _forces,
            }.Schedule(_clothRects.Length, 128, handle);

            // verlet
            handle = new VerletJob
            {
                Frame = frame,
                Info = _info,
                CurrentTransforms = _inputData,
                PrevPositions = _prevPositions,
                CurrentPositions = _currentPositions,
                Forces = _forces,

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
            {
                var handle0 = new StrandCollisionJob
                {
                    Colliders = _colliders,
                    CurrentColliders = _currentColliders,

                    Info = _info,
                    NextPositions = _nextPositions,
                    ClothUsedParticles = _clothUsedParticles,
                    StrandCollision = _strandCollision,
                }.Schedule(_info.Length, 128, handle);

                var handle1 = new ClothCollisionJob
                {
                    Colliders = _colliders,
                    CurrentColliders = _currentColliders,

                    Info = _info,
                    NextPositions = _nextPositions,
                    CollisionCount = _clothCollisionCount,
                    CollisionDelta = _clothCollisionDelta,

                    ClothRects = _clothRects,
                }.Schedule(_clothRects.Length, 128, handle);

                handle = JobHandle.CombineDependencies(handle0, handle1);

                handle = new CollisionApplyJob
                {
                    ClothUsedParticles = _clothUsedParticles,
                    StrandCollision = _strandCollision,
                    ClothCollisionCount = _clothCollisionCount,
                    ClothCollisionDelta = _clothCollisionDelta,
                    NextPosition = _nextPositions,
                }.Schedule(_info.Length, 128, handle);
            }

            // 親子の長さで拘束. TODO: ApplyRotationJob と合体
            handle = new ParentLengthConstraintJob
            {
                Warps = _warps,
                Info = _info,
                NextPositions = _nextPositions,
            }.Schedule(_warps.Length, 16, handle);
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

        public void RestoreInitialTransform()
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

        public void DrawGizmos()
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
                    case TransformType.ClothWarp:
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

        public ClothWarpJobRuntime(Action<Vrm10Instance> onInit = null)
        {
            _onInit = onInit;
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
            return true;
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
        }
    }
}
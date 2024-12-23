using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


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
        // colliderTransform
        //
        List<Transform> _colliderTransforms;
        TransformAccessArray _colliderTransformAccessArray;
        NativeArray<Matrix4x4> _currentColliders;

        //
        // collider
        //
        List<VRM10SpringBoneCollider> _colliders;
        NativeArray<BlittableCollider> _colliderInfo;

        //
        // colliderGroup
        //
        List<VRM10SpringBoneColliderGroup> _colliderGroups;
        NativeArray<int> _colliderRef;
        NativeArray<ArrayRange> _colliderGroup;
        NativeArray<int> _colliderGroupRef;

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

        NativeArray<Vector3> _warpCollision;
        NativeArray<int> _rectCollisionCount;
        NativeArray<Vector3> _rectCollisionDelta;
        NativeArray<Vector3> _impulsiveForces;

        //
        // warp
        //
        NativeArray<WarpInfo> _warps;

        //
        // cloth rect
        //
        NativeArray<bool> _clothUsedParticles;
        NativeArray<(int ClothGridIndex, SpringConstraint SpringConstraint, SphereTriangle.ClothRect Rect)> _clothRects;
        NativeArray<(Vector3, Vector3, Vector3, Vector3)> _clothRectResults;

        //
        // cloth grid
        //
        NativeArray<ClothInfo> _cloths;

        public void Dispose()
        {
            if (_colliderTransformAccessArray.isCreated) _colliderTransformAccessArray.Dispose();
            if (_currentColliders.IsCreated) _currentColliders.Dispose();

            if (_colliderRef.IsCreated) _colliderRef.Dispose();
            if (_colliderGroup.IsCreated) _colliderGroup.Dispose();
            if (_colliderGroupRef.IsCreated) _colliderGroupRef.Dispose();

            if (_warps.IsCreated) _warps.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_inputData.IsCreated) _inputData.Dispose();
            if (_info.IsCreated) _info.Dispose();
            if (_currentPositions.IsCreated) _currentPositions.Dispose();
            if (_prevPositions.IsCreated) _prevPositions.Dispose();
            if (_nextPositions.IsCreated) _nextPositions.Dispose();
            if (_nextRotations.IsCreated) _nextRotations.Dispose();
            if (_warpCollision.IsCreated) _warpCollision.Dispose();
            if (_rectCollisionCount.IsCreated) _rectCollisionCount.Dispose();
            if (_rectCollisionDelta.IsCreated) _rectCollisionDelta.Dispose();
            if (_impulsiveForces.IsCreated) _impulsiveForces.Dispose();

            if (_warps.IsCreated) _warps.Dispose();
            if (_cloths.IsCreated) _cloths.Dispose();

            if (_clothUsedParticles.IsCreated) _clothUsedParticles.Dispose();
            if (_clothRects.IsCreated) _clothRects.Dispose();
            if (_clothRectResults.IsCreated) _clothRectResults.Dispose();
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

        Transform GetParent(Transform t, Transform root)
        {
            for (var parent = t.parent; parent != null; parent = parent.parent)
            {
                if (_transforms.Contains(parent))
                {
                    return parent;
                }
                if (parent == root)
                {
                    break;
                }
            }
            throw new Exception();
        }

        int GetOrAddColliderTransform(Transform t)
        {
            var index = _colliderTransforms.IndexOf(t);
            if (index == -1)
            {
                index = _colliderTransforms.Count;
                _colliderTransforms.Add(t);
            }
            return index;
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
            _colliders = new();
            _colliderGroups = new();
            _colliderTransforms = new();
            List<BlittableCollider> colliderInfo = new();
            List<int> colliderRef = new();
            List<ArrayRange> colliderGroups = new();
            foreach (var colliderGroup in vrm.GetComponentsInChildren<VRM10SpringBoneColliderGroup>())
            {
                if (colliderGroup == null)
                {
                    continue;
                }

                var startColliderRef = colliderRef.Count;
                foreach (var collider in colliderGroup.Colliders)
                {
                    if (collider == null)
                    {
                        continue;
                    }
                    if (_colliders.Contains(collider))
                    {
                        continue;
                    }

                    var colliderTransformIndex = GetOrAddColliderTransform(collider.transform);

                    colliderRef.Add(colliderInfo.Count);
                    colliderInfo.Add(new BlittableCollider
                    {
                        offset = collider.Offset,
                        radius = collider.Radius,
                        tailOrNormal = collider.TailOrNormal,
                        colliderType = TranslateColliderType(collider.ColliderType),
                        transformIndex = colliderTransformIndex,
                    });
                    _colliders.Add(collider);
                }

                _colliderGroups.Add(colliderGroup);
                colliderGroups.Add(new ArrayRange
                {
                    Start = startColliderRef,
                    End = colliderRef.Count,
                });
            }
            _colliderTransformAccessArray = new(_colliderTransforms.ToArray(), 128);
            _currentColliders = new(_colliderTransforms.Count, Allocator.Persistent);
            _colliderInfo = new(colliderInfo.ToArray(), Allocator.Persistent);

            _colliderRef = new(colliderRef.ToArray(), Allocator.Persistent);
            _colliderGroup = new(colliderGroups.ToArray(), Allocator.Persistent);

            //
            // warps => particles
            //
            _transforms = new();
            List<TransformInfo> info = new();
            List<Vector3> positions = new();
            List<WarpInfo> warps = new();
            List<int> colliderGroupRef = new();
            var warpSrcs = vrm.GetComponentsInChildren<Components.ClothWarpRoot>();
            for (int warpIndex = 0; warpIndex < warpSrcs.Length; ++warpIndex)
            {
                var warp = warpSrcs[warpIndex];
                var start = _transforms.Count;

                if (warp.Center != null)
                {
                    GetTransformIndex(warp.Center, new TransformInfo
                    {
                        TransformType = TransformType.Center,
                        WarpIndex = warpIndex,
                    }, info, positions);
                    start += 1;
                }

                var warpRootParentTransformIndex = GetTransformIndex(warp.transform.parent, new TransformInfo
                {
                    TransformType = TransformType.WarpRootParent,
                    WarpIndex = warpIndex,
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
                    Settings = warp.BaseSettings,
                    WarpIndex = warpIndex,
                }, info, positions);
                Debug.Assert(warpRootTransformIndex.index != -1);
                var colliderGroupRefStart = colliderGroupRef.Count;
                if (warpRootTransformIndex.isNew)
                {
                    // var parentIndex = warpRootTransformIndex.index;

                    Func<int, int> GetFirstSiblingIndex = (parent) =>
                    {
                        for (int i = 0; i < info.Count; ++i)
                        {
                            if (info[i].ParentIndex == parent)
                            {
                                return i;
                            }
                        }
                        throw new Exception();
                    };

                    HashSet<int> parentIndexSet = new();
                    foreach (var particle in warp.Particles)
                    {
                        if (particle.Transform != null && particle.Mode != Components.ClothWarpRoot.ParticleMode.Disabled)
                        {
                            var parentIndex = _transforms.IndexOf(GetParent(particle.Transform, warp.transform));
                            BranchInfo? branch = default;
                            if (parentIndexSet.Contains(parentIndex))
                            {
                                branch = new BranchInfo
                                {
                                    FirstSiblingIndex = GetFirstSiblingIndex(parentIndex),
                                };
                            }
                            else
                            {
                                parentIndexSet.Add(parentIndex);
                            }

                            var outputParticleTransformIndex = GetTransformIndex(particle.Transform, new TransformInfo
                            {
                                TransformType = TransformType.Particle,
                                ParentIndex = parentIndex,
                                InitLocalPosition = vrm.DefaultTransformStates[particle.Transform].LocalPosition,
                                InitLocalRotation = vrm.DefaultTransformStates[particle.Transform].LocalRotation,
                                Settings = particle.Settings,
                                WarpIndex = warpIndex,
                                Branch = branch,
                            }, info, positions);
                            // parentIndex = outputParticleTransformIndex.index;
                        }
                    }

                    foreach (var group in warp.ColliderGroups)
                    {
                        if (group != null)
                        {
                            colliderGroupRef.Add(_colliderGroups.IndexOf(group));
                        }
                    }
                }

                warps.Add(new WarpInfo
                {
                    PrticleRange = new ArrayRange
                    {
                        Start = start,
                        End = _transforms.Count,
                    },
                    ColliderGroupRefRange = new ArrayRange
                    {
                        Start = colliderGroupRefStart,
                        End = colliderGroupRef.Count,
                    },
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
            _warpCollision = new(pos.Length, Allocator.Persistent);
            _rectCollisionCount = new(pos.Length, Allocator.Persistent);
            _rectCollisionDelta = new(pos.Length, Allocator.Persistent);
            _impulsiveForces = new(pos.Length, Allocator.Persistent);

            //
            // cloths
            //
            var clothRects = new ClothRectList(_transforms, vrm);
            _clothRects = new(clothRects.List.ToArray(), Allocator.Persistent);
            _clothRectResults = new(clothRects.List.Count, Allocator.Persistent);
            _clothUsedParticles = new(clothRects.ClothUsedParticles, Allocator.Persistent);
            _building = false;

            List<ClothInfo> cloths = new();
            foreach (var grid in clothRects.ClothGrids)
            {
                var colliderGroupRefStart = colliderGroupRef.Count;
                HashSet<VRM10SpringBoneColliderGroup> groups = new();
                foreach (var warp in grid.Warps)
                {
                    foreach (var group in warp.ColliderGroups)
                    {
                        if (group != null && !groups.Contains(group))
                        {
                            groups.Add(group);
                            colliderGroupRef.Add(_colliderGroups.IndexOf(group));
                        }
                    }
                }
                cloths.Add(new ClothInfo
                {
                    ColliderGroupRefRange = new ArrayRange
                    {
                        Start = colliderGroupRefStart,
                        End = colliderGroupRef.Count,
                    },
                });
            }
            _cloths = new(cloths.ToArray(), Allocator.Persistent);

            _colliderGroupRef = new(colliderGroupRef.ToArray(), Allocator.Persistent);
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
                ImpulsiveForces = _impulsiveForces,

                CollisionCount = _rectCollisionCount,
                CollisionDelta = _rectCollisionDelta,
            }.Schedule(_transformAccessArray, handle);

            // spring(cloth weft)
            handle = new WeftConstraintJob
            {
                ClothRects = _clothRects,
                CurrentPositions = _currentPositions,

                ImpulsiveForces = _impulsiveForces,
            }.Schedule(_clothRects.Length, 1, handle);

            // verlet
            handle = new VerletJob
            {
                Frame = frame,
                Info = _info,
                CurrentTransforms = _inputData,
                PrevPositions = _prevPositions,
                CurrentPositions = _currentPositions,
                ImpulsiveForces = _impulsiveForces,

                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Schedule(_info.Length, 1, handle);

            // 親子の長さで拘束
            handle = new ParentLengthConstraintJob
            {
                Warps = _warps,
                Info = _info,
                Data = _inputData,
                NextPositions = _nextPositions,
            }.Schedule(_warps.Length, 1, handle);

            // collision
            {
                var handle0 = new WarpCollisionJob
                {
                    Colliders = _colliderInfo,
                    CurrentColliders = _currentColliders,

                    Info = _info,
                    NextPositions = _nextPositions,
                    ClothUsedParticles = _clothUsedParticles,
                    StrandCollision = _warpCollision,

                    Warps = _warps,
                    ColliderGroupRef = _colliderGroupRef,
                    ColliderGroup = _colliderGroup,
                    ColliderRef = _colliderRef,
                }.Schedule(_info.Length, 1, handle);

                var handle1 = new RectCollisionJob
                {
                    ClothRects = _clothRects,
                    ClothRectResults = _clothRectResults,

                    Colliders = _colliderInfo,
                    CurrentColliders = _currentColliders,

                    Info = _info,
                    NextPositions = _nextPositions,

                    Cloths = _cloths,
                    ColliderGroupRef = _colliderGroupRef,
                    ColliderGroup = _colliderGroup,
                    ColliderRef = _colliderRef,
                }.Schedule(_clothRects.Length, 1, handle);

                handle1 = new RectCollisionReduceJob
                {
                    ClothRects = _clothRects,
                    ClothRectResults = _clothRectResults,
                    NextPositions = _nextPositions,

                    RectCollisionCount = _rectCollisionCount,
                    RectCollisionDelta = _rectCollisionDelta,
                }.Schedule(handle1);

                handle = JobHandle.CombineDependencies(handle0, handle1);

                handle = new CollisionApplyJob
                {
                    ClothUsedParticles = _clothUsedParticles,
                    StrandCollision = _warpCollision,
                    RectCollisionCount = _rectCollisionCount,
                    RectCollisionDelta = _rectCollisionDelta,
                    NextPosition = _nextPositions,
                }.Schedule(_info.Length, 1, handle);
            }

            // 親子の長さで拘束. TODO: ApplyRotationJob と合体
            handle = new ParentLengthConstraintJob
            {
                Warps = _warps,
                Info = _info,
                Data = _inputData,
                NextPositions = _nextPositions,
            }.Schedule(_warps.Length, 1, handle);
            // NextPositions から NextRotations を作る
            handle = new ApplyRotationJob
            {
                Warps = _warps,
                Info = _info,
                CurrentTransforms = _inputData,
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Schedule(_warps.Length, 1, handle);

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
                for (int i = warp.PrticleRange.Start; i < warp.PrticleRange.End; ++i)
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
                        Gizmos.DrawSphere(v, info.Settings.Radius);
                        break;
                    case TransformType.Particle:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(v, info.Settings.Radius);
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
                info.Settings.FromBlittableJointMutable(jointSettings);
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RotateParticle.Components;
using UniGLTF;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UniVRM10;


namespace RotateParticle.Jobs
{
    public enum TransformType
    {
        Center,
        WarpRootParent,
        WarpRoot,
        Particle,
    }

    public static class TransformTypeExtensions
    {
        public static bool PositionInput(this TransformType t)
        {
            return t == TransformType.WarpRoot;
        }

        public static bool Movable(this TransformType t)
        {
            return t == TransformType.Particle;
        }

        public static bool Writable(this TransformType t)
        {
            switch (t)
            {
                case TransformType.WarpRoot:
                case TransformType.Particle:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class RotateParticleJobSystem : IRotateParticleSystem
    {
        Vrm10Instance _vrm;

        public struct TransformInfo
        {
            public TransformType TransformType;
            public int ParentIndex;
            public Quaternion InitLocalRotation;
            public Vector3 InitLocalPosition;
            public Warp.ParticleSettings Settings;
        }

        public struct TransformData
        {
            public Matrix4x4 ToWorld;
            public Vector3 Position => ToWorld.GetPosition();
            public Quaternion Rotation => ToWorld.rotation;
            public Matrix4x4 ToLocal;

            public TransformData(TransformAccess t)
            {
                ToWorld = t.localToWorldMatrix;
                ToLocal = t.worldToLocalMatrix;
            }
            public TransformData(Transform t)
            {
                ToWorld = t.localToWorldMatrix;
                ToLocal = t.worldToLocalMatrix;
            }
        }

        public struct WarpInfo
        {
            public int StartIndex;
            public int EndIndex;
        }

        List<Transform> _transforms;
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

        NativeArray<WarpInfo> _warps;

        TransformAccessArray _transformAccessArray;
        NativeArray<TransformData> _inputData;
        NativeArray<TransformInfo> _info;
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _prevPositions;
        NativeArray<Vector3> _nextPositions;
        NativeArray<Quaternion> _nextRotations;

        void IDisposable.Dispose()
        {
            if (_warps.IsCreated) _warps.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_inputData.IsCreated) _inputData.Dispose();
            if (_info.IsCreated) _info.Dispose();
            if (_currentPositions.IsCreated) _currentPositions.Dispose();
            if (_prevPositions.IsCreated) _prevPositions.Dispose();
            if (_nextPositions.IsCreated) _nextPositions.Dispose();
            if (_nextRotations.IsCreated) _nextRotations.Dispose();
        }

        // [Input]
        public struct InputTransformJob : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<TransformData> InputData;
            public void Execute(int index, TransformAccess transform)
            {
                InputData[index] = new TransformData(transform);
            }
        }

        public struct VerletJob : IJobParallelFor
        {
            public FrameInfo Frame;
            [ReadOnly] public NativeArray<TransformInfo> Info;
            [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
            [ReadOnly] public NativeArray<Vector3> CurrentPositions;
            [ReadOnly] public NativeArray<Vector3> PrevPositions;
            [WriteOnly] public NativeArray<Vector3> NextPositions;
            [WriteOnly] public NativeArray<Quaternion> NextRotations;

            public void Execute(int index)
            {
                var particle = Info[index];
                if (particle.TransformType.Movable())
                {
                    var parentIndex = particle.ParentIndex;
                    var parentPosition = CurrentPositions[parentIndex];
                    var parent = Info[parentIndex];
                    var parentParentRotation = CurrentTransforms[parent.ParentIndex].Rotation;

                    var newPosition = CurrentPositions[index]
                         + (CurrentPositions[index] - PrevPositions[index]) * (1.0f - particle.Settings.DragForce)
                         + parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition *
                               particle.Settings.Stiffness * Frame.DeltaTime // 親の回転による子ボーンの移動目標
                         + Frame.Force * Frame.SqDeltaTime
                         ;

                    // 位置を長さで拘束
                    newPosition = parentPosition + (newPosition - parentPosition).normalized * particle.InitLocalPosition.magnitude;

                    NextPositions[index] = newPosition;
                }
                else
                {
                    // kinematic
                    NextPositions[index] = CurrentPositions[index];
                }

                NextRotations[index] = CurrentTransforms[index].Rotation;
            }
        }

        public struct ApplyRotationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<WarpInfo> Warps;
            [ReadOnly] public NativeArray<TransformInfo> Info;
            [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
            [ReadOnly] public NativeArray<Vector3> NextPositions;
            [NativeDisableParallelForRestriction] public NativeArray<Quaternion> NextRotations;
            public void Execute(int index)
            {
                // warp 一本を親から子に再帰的に実行して回転を確定させる
                var warp = Warps[index];
                for (int i = warp.StartIndex; i < warp.EndIndex - 1; ++i)
                {
                    //回転を適用
                    var p = Info[i];
                    var rotation = NextRotations[p.ParentIndex] * Info[i].InitLocalRotation;
                    NextRotations[i] = Quaternion.FromToRotation(
                        rotation * Info[i + 1].InitLocalPosition,
                        NextPositions[i + 1] - NextPositions[i]) * rotation;
                }
            }
        }

        // [Output]
        public struct OutputTransformJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<TransformInfo> Info;
            [ReadOnly] public NativeArray<Quaternion> NextRotations;
            public void Execute(int index, TransformAccess transform)
            {
                var info = Info[index];
                if (info.TransformType.Writable())
                {
                    transform.rotation = NextRotations[index];
                }
            }
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;
            _transforms = new();
            List<TransformInfo> info = new();
            List<Vector3> positions = new();
            List<WarpInfo> warps = new();
            var warpSrcs = vrm.GetComponentsInChildren<Warp>();
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
                    if (particle != null && particle.Transform != null)
                    {
                        var outputParticleTransformIndex = GetTransformIndex(particle.Transform, new TransformInfo
                        {
                            TransformType = TransformType.Particle,
                            ParentIndex = parentIndex,
                            InitLocalPosition = vrm.DefaultTransformStates[particle.Transform].LocalPosition,
                            InitLocalRotation = vrm.DefaultTransformStates[particle.Transform].LocalRotation,
                            Settings = particle.GetSettings(warp.BaseSettings),
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
        }

        void IRotateParticleSystem.Process(float deltaTime)
        {
            var frame = new FrameInfo(deltaTime, Vector3.zero);

            // input
            new InputTransformJob
            {
                InputData = _inputData,
            }.Schedule(_transformAccessArray, default).Complete();

            // update root position
            for (int i = 0; i < _info.Length; ++i)
            {
                var particle = _info[i];
                if (particle.TransformType.PositionInput())
                {
                    _currentPositions[i] = _inputData[i].ToWorld.GetPosition();
                }
            }

            // verlet
            new VerletJob
            {
                Frame = frame,
                Info = _info,
                CurrentTransforms = _inputData,
                PrevPositions = _prevPositions,
                CurrentPositions = _currentPositions,
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Run(_info.Length);

            // NextPositions から NextRotations を作る
            new ApplyRotationJob
            {
                Warps = _warps,
                Info = _info,
                CurrentTransforms = _inputData,
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Run(_warps.Length);

            // output
            new OutputTransformJob
            {
                Info = _info,
                NextRotations = _nextRotations,
            }.Schedule(_transformAccessArray, default).Complete();

            // update state
            NativeArray<Vector3>.Copy(_currentPositions, _prevPositions);
            NativeArray<Vector3>.Copy(_nextPositions, _currentPositions);
        }

        void IRotateParticleSystem.ResetInitialRotation()
        {
        }

        void IRotateParticleSystem.DrawGizmos()
        {
        }
    }
}
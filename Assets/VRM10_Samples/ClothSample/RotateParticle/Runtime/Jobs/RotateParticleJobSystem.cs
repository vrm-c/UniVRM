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
    public class RotateParticleJobSystem : IRotateParticleSystem
    {
        Vrm10Instance _vrm;

        public enum TransformType
        {
            WarpRoot,
            Particle,
            Center,
        }

        public struct TransformIndo
        {
            public TransformType TransformType;
            public int? ParentIndex;
            public Vector3 InitLocalPosition;
            // DragForce: 1 で即時停止
            public float DragForce;
        }

        public struct TransformData
        {
            public Matrix4x4 ToWorld;
            public Vector3 Position => ToWorld.GetPosition();
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

        // [Input]
        List<Transform> _transforms;
        int GetTransformIndex(Transform t,
            TransformIndo info,
            List<TransformIndo> infos,
            List<Vector3> positions)
        {
            if (t == null) return -1;
            var i = _transforms.IndexOf(t);
            if (i == -1)
            {
                i = _transforms.Count;
                _transforms.Add(t);
                infos.Add(info);
                positions.Add(t.position);
            }
            return i;
        }

        TransformAccessArray _transformAccessArray;
        NativeArray<TransformData> _inputData;
        public struct InputTransformJob : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<TransformData> InputData;
            public void Execute(int index, TransformAccess transform)
            {
                InputData[index] = new TransformData(transform);
            }
        }

        // [Output]
        NativeArray<Vector3> _nextPositions;
        public struct OutputTransformJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<TransformIndo> Info;
            [ReadOnly] public NativeArray<Vector3> NextPositions;
            public void Execute(int index, TransformAccess transform)
            {
                var info = Info[index];
                if (info.TransformType == TransformType.Particle)
                {
                    transform.position = NextPositions[index];
                }
            }
        }

        NativeArray<TransformIndo> _info;

        // [Init/State]
        // 初期化時に値を作って毎フレーム更新していく
        // output と同じ長さ index
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _prevPositions;
        public struct VerletJob : IJobParallelFor
        {
            public FrameInfo Frame;
            [ReadOnly] public NativeArray<TransformIndo> Info;
            [ReadOnly] public NativeArray<Vector3> Current;
            [ReadOnly] public NativeArray<Vector3> Prev;
            [WriteOnly] public NativeArray<Vector3> Next;

            public void Execute(int index)
            {
                var particle = Info[index];
                if (particle.ParentIndex.HasValue)
                {
                    var velocity = Current[index] - Prev[index];
                    velocity *= (1 - particle.DragForce);
                    var newPosition = Current[index] + velocity + Frame.Force * Frame.SqDeltaTime;

                    var parentIndex = particle.ParentIndex.Value;
                    var parentPosition = Current[parentIndex];

                    // 位置を長さで拘束
                    newPosition = parentPosition + (newPosition - parentPosition).normalized * particle.InitLocalPosition.magnitude;

                    Next[index] = newPosition;
                }
                else
                {
                    // kinematic
                    Next[index] = Current[index];
                }
            }
        }

        void IDisposable.Dispose()
        {
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;
            _transforms = new();
            List<TransformIndo> info = new();
            List<Vector3> positions = new();
            var warpSrcs = vrm.GetComponentsInChildren<Warp>();
            for (int warpIndex = 0; warpIndex < warpSrcs.Length; ++warpIndex)
            {
                var warp = warpSrcs[warpIndex];
                var centerTransformIndex = GetTransformIndex(warp.Center, new TransformIndo { TransformType = TransformType.Center }, info, positions);
                var warpRootTransformIndex = GetTransformIndex(warp.transform, new TransformIndo { TransformType = TransformType.WarpRoot }, info, positions);
                Debug.Assert(warpRootTransformIndex != -1);

                var parentIndex = warpRootTransformIndex;
                foreach (var particle in warp.Particles)
                {
                    if (particle != null && particle.Transform != null)
                    {
                        var outputParticleTransformIndex = GetTransformIndex(particle.Transform, new TransformIndo
                        {
                            TransformType = TransformType.Particle,
                            ParentIndex = parentIndex,
                            InitLocalPosition = vrm.DefaultTransformStates[particle.Transform].LocalPosition,
                            DragForce = particle.GetSettings(warp.BaseSettings).DragForce,
                        }, info, positions);
                        parentIndex = outputParticleTransformIndex;
                    }
                }
                await awaitCaller.NextFrame();
            }
            _transformAccessArray = new(_transforms.ToArray(), 128);
            _inputData = new(_transforms.Count, Allocator.Persistent);
            var pos = positions.ToArray();
            _currentPositions = new(pos, Allocator.Persistent);
            _prevPositions = new(pos, Allocator.Persistent);
            _nextPositions = new(pos.Length, Allocator.Persistent);
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
                if (particle.TransformType == TransformType.WarpRoot)
                {
                    _currentPositions[i] = _inputData[i].ToWorld.GetPosition();
                }
            }

            // verlet
            new VerletJob
            {
                Frame = frame,
                Info = _info,
                Prev = _prevPositions,
                Current = _currentPositions,
                Next = _nextPositions,
            }.Run(_info.Length);

            // output
            new OutputTransformJob
            {
                Info = _info,
                NextPositions = _nextPositions,
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
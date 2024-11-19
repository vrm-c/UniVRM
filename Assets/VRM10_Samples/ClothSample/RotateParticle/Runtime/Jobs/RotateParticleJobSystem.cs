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

        // [Input]
        // center
        // rootParent
        // root
        //
        // の入力(jobにわたす)
        List<Transform> _inputTransforms;
        TransformAccessArray _inputTransformAccessArray;
        public struct InputTransformData
        {
            public Matrix4x4 ToWorld;
            public Matrix4x4 TOLocal;

            public InputTransformData(TransformAccess t)
            {
                ToWorld = t.localToWorldMatrix;
                TOLocal = t.worldToLocalMatrix;
            }
        }
        NativeArray<InputTransformData> _inputData;
        public struct InputTransformJob : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<InputTransformData> InputData;
            public void Execute(int index, TransformAccess transform)
            {
                InputData[index] = new InputTransformData(transform);
            }
        }

        // [Output]
        // root 回転のみ
        // particle 回転 + 移動
        // を出力(jobから反映する)
        List<Transform> _outputTransforms;
        List<Vector3> _positions;
        NativeArray<Vector3> _nextPositions;
        TransformAccessArray _outputTransformAccessArray;
        public struct OutputTransformJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Vector3> NextPositions;
            public void Execute(int index, TransformAccess transform)
            {
                transform.position = NextPositions[index];
            }
        }

        public struct ParticleIndo
        {
            public int InputTransformIndex;
            public int? ParentIndex;
            public Vector3 InitLocalPosition;
            // DragForce: 1 で即時停止
            public float DragForce;
        }
        NativeArray<ParticleIndo> _particles;

        // [Init/State]
        // 初期化時に値を作って毎フレーム更新していく
        // output と同じ長さ index
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _prevPositions;
        public struct VerletJob : IJobParallelFor
        {
            public FrameInfo Frame;
            [WriteOnly] public NativeArray<Vector3> NextPositions;
            [ReadOnly] public NativeArray<Vector3> CurrentPositions;
            [ReadOnly] public NativeArray<Vector3> PrevPositions;
            [ReadOnly] public NativeArray<ParticleIndo> Particles;

            public void Execute(int index)
            {
                var particle = Particles[index];
                if (particle.ParentIndex.HasValue)
                {
                    var velocity = CurrentPositions[index] - PrevPositions[index];
                    velocity *= (1 - particle.DragForce);
                    var newPosition = CurrentPositions[index] + velocity + Frame.Force * Frame.SqDeltaTime;

                    var parentIndex = particle.ParentIndex.Value;
                    var parentPosition = CurrentPositions[parentIndex];

                    // 位置を長さで拘束
                    newPosition = parentPosition + (newPosition - parentPosition).normalized * particle.InitLocalPosition.magnitude;

                    NextPositions[index] = newPosition;
                }
                else
                {
                    // kinematic
                    NextPositions[index] = CurrentPositions[index];
                }
            }
        }

        void IDisposable.Dispose()
        {
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;

            _inputTransforms = new();
            _outputTransforms = new();
            _positions = new();

            List<ParticleIndo> particles = new();
            var warpSrcs = vrm.GetComponentsInChildren<Warp>();
            for (int warpIndex = 0; warpIndex < warpSrcs.Length; ++warpIndex)
            {
                var warp = warpSrcs[warpIndex];
                var inputCenterTransformIndex = GetInputTransformIndex(warp.Center);
                var inputWarpRootParentTransformIndex = GetInputTransformIndex(warp.transform.parent);
                Debug.Assert(inputWarpRootParentTransformIndex != -1);
                var inputWarpRootTransformIndex = GetInputTransformIndex(warp.transform);
                Debug.Assert(inputWarpRootParentTransformIndex != -1);

                var ouputWarpRootTransformIndex = GetOutputTransformIndex(warp.transform);
                particles.Add(new ParticleIndo
                {
                    InputTransformIndex = inputWarpRootTransformIndex,
                });
                var parentIndex = ouputWarpRootTransformIndex;
                foreach (var particle in warp.Particles)
                {
                    if (particle != null && particle.Transform != null)
                    {
                        var outputParticleTransformIndex = GetOutputTransformIndex(particle.Transform);
                        particles.Add(new ParticleIndo
                        {
                            ParentIndex = parentIndex,
                            InitLocalPosition = vrm.DefaultTransformStates[particle.Transform].LocalPosition,
                            DragForce = particle.GetSettings(warp.BaseSettings).DragForce,
                        });
                        parentIndex = outputParticleTransformIndex;
                    }
                }
                await awaitCaller.NextFrame();
            }
            _inputTransformAccessArray = new(_inputTransforms.ToArray(), 128);
            _inputData = new(_inputTransforms.Count, Allocator.Persistent);

            _outputTransformAccessArray = new(_outputTransforms.ToArray(), 128);
            var positions = _positions.ToArray();
            _currentPositions = new(positions, Allocator.Persistent);
            _prevPositions = new(positions, Allocator.Persistent);
            _nextPositions = new(positions, Allocator.Persistent);
            _particles = new(particles.ToArray(), Allocator.Persistent);
        }

        int GetInputTransformIndex(Transform t)
        {
            if (t == null) return -1;
            var i = _inputTransforms.IndexOf(t);
            if (i == -1)
            {
                i = _inputTransforms.Count;
                _inputTransforms.Add(t);
            }
            return i;
        }

        int GetOutputTransformIndex(Transform t)
        {
            if (t == null) return -1;
            var i = _outputTransforms.IndexOf(t);
            if (i == -1)
            {
                i = _outputTransforms.Count;
                _outputTransforms.Add(t);
                _positions.Add(t.position);
            }
            return i;
        }

        void IRotateParticleSystem.Process(float deltaTime)
        {
            var frame = new FrameInfo(deltaTime, Vector3.zero);

            // input
            new InputTransformJob
            {
                InputData = _inputData,
            }.Schedule(_inputTransformAccessArray, default).Complete();

            // update root position
            for (int i = 0; i < _particles.Length; ++i)
            {
                var particle = _particles[i];
                if (!particle.ParentIndex.HasValue)
                {
                    _currentPositions[i] = _inputData[particle.InputTransformIndex].ToWorld.GetPosition();
                }
            }

            // verlet
            new VerletJob
            {
                Frame = frame,
                Particles = _particles,
                PrevPositions = _prevPositions,
                CurrentPositions = _currentPositions,
                NextPositions = _nextPositions,
            }.Run(_particles.Length);

            // output
            new OutputTransformJob
            {
                NextPositions = _nextPositions,
            }.Schedule(_outputTransformAccessArray, default).Complete();

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
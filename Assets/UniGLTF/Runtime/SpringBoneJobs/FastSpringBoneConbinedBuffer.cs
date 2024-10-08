#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF.SpringBoneJobs.InputPorts;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace UniGLTF.SpringBoneJobs
{
    /// <summary>
    /// FastSpringBoneの処理に利用するバッファを全て結合して持つクラス
    /// </summary>
    public class FastSpringBoneCombinedBuffer : IDisposable
    {
        // Joint Level
        private NativeArray<BlittableJointImmutable> _logics;
        private NativeArray<BlittableJointMutable> _joints;
        private NativeArray<Vector3> _prevTails;
        private NativeArray<Vector3> _currentTails;
        private NativeArray<Vector3> _nextTails;
        // Spring Level
        private NativeArray<BlittableSpring> _springs;
        // Moodel Level
        private NativeArray<BlittableModelLevel> _models;

        // その他
        private NativeArray<BlittableCollider> _colliders;
        private NativeArray<BlittableTransform> _transforms;
        private TransformAccessArray _transformAccessArray;

        // accessor: Joint Level 
        public NativeArray<BlittableJointImmutable> Logics => _logics;
        public NativeArray<BlittableJointMutable> Joints => _joints;
        public NativeArray<Vector3> PrevTails => _prevTails;
        public NativeArray<Vector3> CurrentTails => _currentTails;
        public NativeArray<Vector3> NextTails => _nextTails;
        // accessor: Spring Level
        public NativeArray<BlittableSpring> Springs => _springs;
        // accessor: Model LEvel
        public NativeArray<BlittableModelLevel> Models => _models;
        // accessor: Other
        public NativeArray<BlittableCollider> Colliders => _colliders;
        public NativeArray<BlittableTransform> Transforms => _transforms;
        public TransformAccessArray TransformAccessArray => _transformAccessArray;
        // 構築情報
        private FastSpringBoneBuffer[] _batchedBuffers;
        private int[] _batchedBufferLogicSizes;
        private Dictionary<Transform, int> _modelMap = new();
        private Dictionary<Transform, int> _jointMap = new();

        private FastSpringBoneCombinedBuffer(int logicsCount, int springsCount, int modelCount, int collidersCount, int transformsCount,
            FastSpringBoneBuffer[] batchedBuffers,
            int[] batchedBufferLogicSizes
        )
        {
            // joint level
            _logics = new NativeArray<BlittableJointImmutable>(logicsCount, Allocator.Persistent);
            _joints = new NativeArray<BlittableJointMutable>(logicsCount, Allocator.Persistent);
            _prevTails = new NativeArray<Vector3>(logicsCount, Allocator.Persistent);
            _currentTails = new NativeArray<Vector3>(logicsCount, Allocator.Persistent);
            _nextTails = new NativeArray<Vector3>(logicsCount, Allocator.Persistent);
            // spring level
            _springs = new NativeArray<BlittableSpring>(springsCount, Allocator.Persistent);
            // model level
            _models = new NativeArray<BlittableModelLevel>(modelCount, Allocator.Persistent);
            // others
            _colliders = new NativeArray<BlittableCollider>(collidersCount, Allocator.Persistent);
            _transforms = new NativeArray<BlittableTransform>(transformsCount, Allocator.Persistent);
            _batchedBuffers = batchedBuffers;
            _batchedBufferLogicSizes = batchedBufferLogicSizes;
        }

        internal static JobHandle Create(JobHandle handle,
            LinkedList<FastSpringBoneBuffer> _buffers, out FastSpringBoneCombinedBuffer combined)
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CopyToBatchedBuffers");
            var batchedBuffers = _buffers.ToArray();
            var batchedBufferLogicSizes = batchedBuffers.Select(buffer => buffer.Logics.Length).ToArray();
            Profiler.EndSample();

            // バッファを数える
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CountBufferSize");
            var springsCount = 0;
            var collidersCount = 0;
            var logicsCount = 0;
            var transformsCount = 0;
            foreach (var buffer in _buffers)
            {
                springsCount += buffer.Springs.Length;
                collidersCount += buffer.Colliders.Length;
                logicsCount += buffer.Logics.Length;
                transformsCount += buffer.BlittableTransforms.Length;
            }
            Profiler.EndSample();

            // バッファの構築
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CreateBuffers");
            combined = new FastSpringBoneCombinedBuffer(logicsCount, springsCount, _buffers.Count,
                collidersCount, transformsCount, batchedBuffers, batchedBufferLogicSizes);
            Profiler.EndSample();

            return combined.Batching(handle);
        }

        private JobHandle Batching(JobHandle handle)
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.ScheduleLoadBufferJobs");
            var springsOffset = 0;
            var collidersOffset = 0;
            var logicsOffset = 0;
            var transformOffset = 0;
            for (var i = 0; i < _batchedBuffers.Length; i++)
            {
                var buffer = _batchedBuffers[i];
                // 逆引き
                _modelMap.Add(buffer.Model, i);

                for (var j = 0; j < _batchedBufferLogicSizes[i]; ++j)
                {
                    var head = buffer.Logics[j].headTransformIndex;
                    _jointMap.Add(buffer.Transforms[head], logicsOffset + j);
                }

                // バッファの読み込みをスケジュール
                handle = new LoadTransformsJob
                {
                    SrcTransforms = buffer.BlittableTransforms,
                    DestTransforms = new NativeSlice<BlittableTransform>(_transforms, transformOffset,
                            buffer.BlittableTransforms.Length)
                }.Schedule(buffer.BlittableTransforms.Length, 1, handle);

                handle = new LoadSpringsJob
                {
                    ModelIndex = i,
                    SrcSprings = buffer.Springs,
                    DestSprings = new NativeSlice<BlittableSpring>(_springs, springsOffset, buffer.Springs.Length),
                    CollidersOffset = collidersOffset,
                    LogicsOffset = logicsOffset,
                    TransformOffset = transformOffset,
                }.Schedule(buffer.Springs.Length, 1, handle);

                handle = new LoadCollidersJob
                {
                    SrcColliders = buffer.Colliders,
                    DestColliders = new NativeSlice<BlittableCollider>(_colliders, collidersOffset, buffer.Colliders.Length)
                }.Schedule(buffer.Colliders.Length, 1, handle);

                handle = new OffsetLogicsJob
                {
                    SrcLogics = buffer.Logics,
                    SrcJoints = buffer.Joints,

                    DestLogics = new NativeSlice<BlittableJointImmutable>(_logics, logicsOffset, buffer.Logics.Length),
                    DestJoints = new NativeSlice<BlittableJointMutable>(_joints, logicsOffset, buffer.Logics.Length),
                }.Schedule(buffer.Logics.Length, 1, handle);

                springsOffset += buffer.Springs.Length;
                collidersOffset += buffer.Colliders.Length;
                logicsOffset += buffer.Logics.Length;
                transformOffset += buffer.BlittableTransforms.Length;
            }

            handle = InitCurrentTails(handle);

            // TransformAccessArrayの構築と並行してJobを行うため、この時点で走らせておく
            JobHandle.ScheduleBatchedJobs();
            Profiler.EndSample();

            // TransformAccessArrayの構築
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.LoadTransformAccessArray");
            var transforms = new Transform[_transforms.Length];
            var transformAccessArrayOffset = 0;
            foreach (var buffer in _batchedBuffers)
            {
                Array.Copy(buffer.Transforms, 0, transforms, transformAccessArrayOffset, buffer.Transforms.Length);
                transformAccessArrayOffset += buffer.BlittableTransforms.Length;
            }

            _transformAccessArray = new TransformAccessArray(transforms);
            Profiler.EndSample();

            return handle;
        }

        public void Dispose()
        {
            // joint
            if (_logics.IsCreated) _logics.Dispose();
            if (_joints.IsCreated) _joints.Dispose();
            if (_prevTails.IsCreated) _prevTails.Dispose();
            if (_currentTails.IsCreated) _currentTails.Dispose();
            if (_nextTails.IsCreated) _nextTails.Dispose();
            // spring
            if (_springs.IsCreated) _springs.Dispose();
            // model
            if (_models.IsCreated) _models.Dispose();
            // other
            if (_colliders.IsCreated) _colliders.Dispose();
            if (_transforms.IsCreated) _transforms.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        }

        /// <summary>
        /// バッチングされたバッファから、個々のバッファへと値を戻す
        /// Logics to _batchedBuffers[].Logics
        /// バッファの再構築前にこの処理を行わないと、揺れの状態がリセットされてしまい、不自然な挙動になる
        /// </summary>
        internal void SaveToSourceBuffer()
        {
            var logicsIndex = 0;
            for (var i = 0; i < _batchedBuffers.Length; ++i)
            {
                var length = _batchedBufferLogicSizes[i];
                if (!_batchedBuffers[i].IsDisposed && length > 0)
                {
                    NativeArray<BlittableJointImmutable>.Copy(Logics, logicsIndex, _batchedBuffers[i].Logics, 0, length);
                }
                logicsIndex += length;
            }
        }

        public void FlipBuffer()
        {
            var tmp = _prevTails;
            _prevTails = _currentTails;
            _currentTails = _nextTails;
            _nextTails = tmp;
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            if (_jointMap.TryGetValue(joint, out var jointIndex))
            {
                _joints[jointIndex] = jointSettings;
            }
        }

        public void SetModelLevel(Transform model, BlittableModelLevel modelSetting)
        {
            if (_modelMap.TryGetValue(model, out var modelIndex))
            {
                _models[modelIndex] = modelSetting;
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        /// <summary>
        /// 
        /// </summary>
        private struct LoadTransformsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlittableTransform> SrcTransforms;
            [WriteOnly] public NativeSlice<BlittableTransform> DestTransforms;

            public void Execute(int index)
            {
                DestTransforms[index] = SrcTransforms[index];
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct LoadSpringsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlittableSpring> SrcSprings;
            [WriteOnly] public NativeSlice<BlittableSpring> DestSprings;

            public int ModelIndex;
            public int CollidersOffset;
            public int LogicsOffset;
            public int TransformOffset;

            public void Execute(int index)
            {
                var spring = SrcSprings[index];
                spring.modelIndex = ModelIndex;
                spring.colliderSpan.startIndex += CollidersOffset;
                spring.logicSpan.startIndex += LogicsOffset;
                spring.transformIndexOffset = TransformOffset;
                DestSprings[index] = spring;
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct LoadCollidersJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlittableCollider> SrcColliders;
            [WriteOnly] public NativeSlice<BlittableCollider> DestColliders;

            public void Execute(int index)
            {
                DestColliders[index] = SrcColliders[index];
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct OffsetLogicsJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<BlittableJointImmutable> SrcLogics;
            [ReadOnly] public NativeSlice<BlittableJointMutable> SrcJoints;
            [WriteOnly] public NativeSlice<BlittableJointImmutable> DestLogics;
            [WriteOnly] public NativeSlice<BlittableJointMutable> DestJoints;

            public void Execute(int index)
            {
                DestLogics[index] = SrcLogics[index];
                DestJoints[index] = SrcJoints[index];
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct InitCurrentTailsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlittableJointImmutable> Logics;
            [ReadOnly] public NativeArray<BlittableTransform> Transforms;
            [WriteOnly] public NativeSlice<Vector3> CurrentTails;
            [WriteOnly] public NativeSlice<Vector3> PrevTails;
            [WriteOnly] public NativeSlice<Vector3> NextTails;

            public void Execute(int jointIndex)
            {
                var tailIndex = Logics[jointIndex].tailTransformIndex;
                if (tailIndex == -1)
                {
                    // tail 無い
                    var tail = Transforms[Logics[jointIndex].headTransformIndex];
                    CurrentTails[jointIndex] = tail.position;
                    PrevTails[jointIndex] = tail.position;
                    NextTails[jointIndex] = tail.position;
                }
                else
                {
                    var tail = Transforms[tailIndex];
                    CurrentTails[jointIndex] = tail.position;
                    PrevTails[jointIndex] = tail.position;
                    NextTails[jointIndex] = tail.position;
                }
            }
        }

        /// <summary>
        /// Transform から currentTail を更新。
        /// prevTail も同じ内容にする(速度0)。
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public JobHandle InitCurrentTails(JobHandle handle)
        {
            return new InitCurrentTailsJob
            {
                Logics = Logics,
                Transforms = Transforms,
                CurrentTails = CurrentTails,
                PrevTails = PrevTails,
                NextTails = NextTails,
            }.Schedule(Logics.Length, 1, handle);
        }

        public void InitializeJointsLocalRotation(FastSpringBoneBuffer buffer)
        {
            var logicsIndex = 0;
            for (var i = 0; i < _batchedBuffers.Length; ++i)
            {
                var length = _batchedBufferLogicSizes[i];
                Debug.Assert(length == buffer.Logics.Length);
                if (_batchedBuffers[i] == buffer)
                {
                    for (var j = 0; j < length; ++j)
                    {
                        var logic = buffer.Logics[j];
                        if (logic.tailTransformIndex != -1)
                        {
                            var tailPosition = buffer.Transforms[logic.tailTransformIndex].position;
                            var dst = logicsIndex + j;
                            // tail 位置を初期化し速度を0にする
                            _currentTails[dst] = _prevTails[dst] = _nextTails[dst] = tailPosition;
                        }
                    }
                    break;
                }
                logicsIndex += length;
            }
        }
    }
}
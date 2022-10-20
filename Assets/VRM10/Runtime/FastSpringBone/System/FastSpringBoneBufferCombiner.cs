using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UniVRM10.FastSpringBones.Blittables;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif

namespace UniVRM10.FastSpringBones.System
{
    /// <summary>
    /// FastSpringBoneの処理に利用するバッファを全て結合して持つクラス
    /// </summary>
    public sealed class FastSpringBoneBufferCombiner : IDisposable
    {
        private NativeArray<BlittableSpring> _springs;
        private NativeArray<BlittableTransform> _transforms;
        private NativeArray<BlittableCollider> _colliders;
        private NativeArray<BlittableJoint> _joints;
        private NativeArray<BlittableLogic> _logics;
        private TransformAccessArray _transformAccessArray;

        private readonly LinkedList<FastSpringBoneBuffer> _buffers = new LinkedList<FastSpringBoneBuffer>();
        private FastSpringBoneBuffer[] _batchedBuffers;
        private int[] _batchedBufferLogicSizes;

        private bool _isDirty;

        public NativeArray<BlittableSpring> Springs => _springs;
        public NativeArray<BlittableJoint> Joints => _joints;
        public NativeArray<BlittableTransform> Transforms => _transforms;
        public TransformAccessArray TransformAccessArray => _transformAccessArray;
        public NativeArray<BlittableCollider> Colliders => _colliders;
        public NativeArray<BlittableLogic> Logics => _logics;

        public bool HasBuffer => _batchedBuffers != null && _batchedBuffers.Length > 0;

        public void Register(FastSpringBoneBuffer buffer)
        {
            _buffers.AddLast(buffer);
            _isDirty = true;
        }

        public void Unregister(FastSpringBoneBuffer buffer)
        {
            _buffers.Remove(buffer);
            _isDirty = true;
        }

        /// <summary>
        /// 変更があったならばバッファを再構築する
        /// </summary>
        public JobHandle ReconstructIfDirty(JobHandle handle)
        {
            if (_isDirty)
            {
                var result = ReconstructBuffers(handle);
                _isDirty = false;
                return result;
            }

            return handle;
        }

        /// <summary>
        /// バッチングされたバッファから、個々のバッファへと値を戻す
        /// バッファの再構築前にこの処理を行わないと、揺れの状態がリセットされてしまい、不自然な挙動になる
        /// </summary>
        private void SaveToSourceBuffer()
        {
            if (_batchedBuffers == null) return;

            var logicsIndex = 0;
            for (var i = 0; i < _batchedBuffers.Length; ++i)
            {
                var length = _batchedBufferLogicSizes[i];
                if (!_batchedBuffers[i].IsDisposed && length > 0)
                {
                    NativeArray<BlittableLogic>.Copy(_logics, logicsIndex, _batchedBuffers[i].Logics, 0, length);
                }

                logicsIndex += length;
            }
        }

        /// <summary>
        /// バッファを再構築する
        /// </summary>
        private JobHandle ReconstructBuffers(JobHandle handle)
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers");

            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.SaveToSourceBuffer");
            SaveToSourceBuffer();
            Profiler.EndSample();

            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.DisposeBuffers");
            DisposeAllBuffers();
            Profiler.EndSample();

            var springsCount = 0;
            var collidersCount = 0;
            var logicsCount = 0;
            var transformsCount = 0;

            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CopyToBatchedBuffers");
            _batchedBuffers = _buffers.ToArray();
            _batchedBufferLogicSizes = _batchedBuffers.Select(buffer => buffer.Logics.Length).ToArray();
            Profiler.EndSample();

            // バッファを数える
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CountBufferSize");
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
            _springs = new NativeArray<BlittableSpring>(springsCount, Allocator.Persistent);

            _joints = new NativeArray<BlittableJoint>(logicsCount, Allocator.Persistent);
            _logics = new NativeArray<BlittableLogic>(logicsCount, Allocator.Persistent);
            _colliders = new NativeArray<BlittableCollider>(collidersCount, Allocator.Persistent);

            _transforms = new NativeArray<BlittableTransform>(transformsCount, Allocator.Persistent);
            Profiler.EndSample();

            var springsOffset = 0;
            var collidersOffset = 0;
            var logicsOffset = 0;
            var transformOffset = 0;

            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.ScheduleLoadBufferJobs");
            for (var i = 0; i < _batchedBuffers.Length; i++)
            {
                var buffer = _batchedBuffers[i];

                // バッファの読み込みをスケジュール
                handle = new LoadTransformsJob
                {
                    SrcTransforms = buffer.BlittableTransforms,
                    DestTransforms =
                        new NativeSlice<BlittableTransform>(_transforms, transformOffset,
                            buffer.BlittableTransforms.Length)
                }.Schedule(buffer.BlittableTransforms.Length, 1, handle);
                handle = new LoadSpringsJob
                {
                    SrcSprings = buffer.Springs,
                    DestSprings = new NativeSlice<BlittableSpring>(_springs, springsOffset, buffer.Springs.Length),
                    CollidersOffset = collidersOffset,
                    LogicsOffset = logicsOffset,
                    TransformOffset = transformOffset,
                }.Schedule(buffer.Springs.Length, 1, handle);
                handle = new LoadCollidersJob
                {
                    SrcColliders = buffer.Colliders,
                    DestColliders =
                        new NativeSlice<BlittableCollider>(_colliders, collidersOffset, buffer.Colliders.Length)
                }.Schedule(buffer.Colliders.Length, 1, handle);
                handle = new OffsetLogicsJob
                {
                    SrcLogics = buffer.Logics,
                    SrcJoints = buffer.Joints,
                    DestLogics = new NativeSlice<BlittableLogic>(_logics, logicsOffset, buffer.Logics.Length),
                    DestJoints = new NativeSlice<BlittableJoint>(_joints, logicsOffset, buffer.Logics.Length),
                }.Schedule(buffer.Logics.Length, 1, handle);

                springsOffset += buffer.Springs.Length;
                collidersOffset += buffer.Colliders.Length;
                logicsOffset += buffer.Logics.Length;
                transformOffset += buffer.BlittableTransforms.Length;
            }

            // TransformAccessArrayの構築と並行してJobを行うため、この時点で走らせておく
            JobHandle.ScheduleBatchedJobs();
            Profiler.EndSample();

            // TransformAccessArrayの構築
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.LoadTransformAccessArray");
            var transforms = new Transform[transformsCount];
            var transformAccessArrayOffset = 0;
            foreach (var buffer in _batchedBuffers)
            {
                Array.Copy(buffer.Transforms, 0, transforms, transformAccessArrayOffset, buffer.Transforms.Length);
                transformAccessArrayOffset += buffer.BlittableTransforms.Length;
            }

            _transformAccessArray = new TransformAccessArray(transforms);
            Profiler.EndSample();

            Profiler.EndSample();

            return handle;
        }

        private void DisposeAllBuffers()
        {
            if (_springs.IsCreated) _springs.Dispose();
            if (_joints.IsCreated) _joints.Dispose();
            if (_transforms.IsCreated) _transforms.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_colliders.IsCreated) _colliders.Dispose();
            if (_logics.IsCreated) _logics.Dispose();
        }

        public void Dispose()
        {
            DisposeAllBuffers();
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
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

            public int CollidersOffset;
            public int LogicsOffset;
            public int TransformOffset;

            public void Execute(int index)
            {
                var spring = SrcSprings[index];
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
            [ReadOnly] public NativeSlice<BlittableLogic> SrcLogics;
            [ReadOnly] public NativeSlice<BlittableJoint> SrcJoints;
            [WriteOnly] public NativeSlice<BlittableLogic> DestLogics;
            [WriteOnly] public NativeSlice<BlittableJoint> DestJoints;

            public void Execute(int index)
            {
                DestLogics[index] = SrcLogics[index];
                DestJoints[index] = SrcJoints[index];
            }
        }
    }
}
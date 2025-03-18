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

        /// <summary>
        /// Job向けに、Lidt[FastSpringBoneBuffer] をひとつの FastSpringBoneCombinedBuffer に統合する
        /// </summary>
        internal static JobHandle Create(JobHandle handle,
            IReadOnlyList<FastSpringBoneBuffer> buffers, out FastSpringBoneCombinedBuffer combined)
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CopyToBatchedBuffers");
            var batchedBuffers = buffers.ToArray();
            var batchedBufferLogicSizes = batchedBuffers.Select(buffer => buffer.Logics.Length).ToArray();
            Profiler.EndSample();

            // バッファを数える
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CountBufferSize");
            var springsCount = 0;
            var collidersCount = 0;
            var logicsCount = 0;
            var transformsCount = 0;
            foreach (var buffer in buffers)
            {
                springsCount += buffer.Springs.Length;
                collidersCount += buffer.Colliders.Length;
                logicsCount += buffer.Logics.Length;
                transformsCount += buffer.Transforms.Length;
            }
            Profiler.EndSample();

            // バッファの構築
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.CreateBuffers");
            combined = new FastSpringBoneCombinedBuffer(logicsCount, springsCount, buffers.Count,
                collidersCount, transformsCount, batchedBuffers, batchedBufferLogicSizes);
            Profiler.EndSample();

            return combined.Batching(handle);
        }

        private JobHandle Batching(JobHandle handle)
        {
            // TransformAccessArrayの構築
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.LoadTransformAccessArray");
            var transforms = new Transform[_transforms.Length];
            var transformAccessArrayOffset = 0;
            foreach (var buffer in _batchedBuffers)
            {
                Array.Copy(buffer.Transforms, 0, transforms, transformAccessArrayOffset, buffer.Transforms.Length);
                transformAccessArrayOffset += buffer.Transforms.Length;
            }
            _transformAccessArray = new TransformAccessArray(transforms);
            Profiler.EndSample();

            // Transforms を更新。後続の InitCurrentTails で使う
            handle = new PullTransformJob
            {
                Transforms = Transforms
            }.Schedule(TransformAccessArray, handle);

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

                // 速度の維持
                buffer.RestoreCurrentTails(_currentTails, _nextTails, logicsOffset);

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
                transformOffset += buffer.Transforms.Length;
            }

            handle = InitCurrentTails(handle);
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
            [ReadOnly] public NativeArray<BlittableSpring> Springs;

            [ReadOnly] public NativeArray<BlittableJointImmutable> Logics;
            [ReadOnly] public NativeArray<BlittableTransform> Transforms;
            [NativeDisableParallelForRestriction] public NativeSlice<Vector3> CurrentTails;
            [NativeDisableParallelForRestriction] public NativeSlice<Vector3> PrevTails;
            [NativeDisableParallelForRestriction] public NativeSlice<Vector3> NextTails;

            public void Execute(int springIndex)
            {
                var spring = Springs[springIndex];
                var center = spring.centerTransformIndex >= 0
                    ? Transforms[spring.transformIndexOffset + spring.centerTransformIndex]
                    : (BlittableTransform?)null;
                for (int jointIndex = spring.logicSpan.startIndex; jointIndex < spring.logicSpan.EndIndex; ++jointIndex)
                {
                    if (float.IsNaN(CurrentTails[jointIndex].x))
                    {
                        // Transsform の現状を使う。velocity を zero にする
                        int tailIndex;
                        if (Logics[jointIndex].tailTransformIndex == -1)
                        {
                            // tail 無い
                            tailIndex = spring.transformIndexOffset + Logics[jointIndex].headTransformIndex;
                        }
                        else
                        {
                            tailIndex = spring.transformIndexOffset + Logics[jointIndex].tailTransformIndex;
                        }

                        var tail = Transforms[tailIndex];
                        var tailPos = center.HasValue ? center.Value.worldToLocalMatrix.MultiplyPoint3x4(tail.position) : tail.position;
                        CurrentTails[jointIndex] = tailPos;
                        PrevTails[jointIndex] = tailPos;
                        NextTails[jointIndex] = tailPos;
                    }
                }
            }
        }

        /// <summary>        
        /// # CurrentTails[i] == NAN
        /// 
        /// Transforms から Current, Prev, Next を代入する。
        /// 速度 0 で初期化することになる。
        /// 
        /// # CurrentTails[i] != NAN
        /// 
        /// 本処理はスキップされて Current, Next の利用が継続されます。
        /// 
        /// # NAN
        /// 
        /// Batching 関数内の FastSpringBoneBuffer.RestoreCurrentTails にて backup の Current, Next が無かったときに
        /// 目印として NAN が代入されます。
        /// </summary>
        public JobHandle InitCurrentTails(JobHandle handle)
        {
            return new InitCurrentTailsJob
            {
                Springs = Springs,

                Logics = Logics,
                Transforms = Transforms,
                CurrentTails = CurrentTails,
                PrevTails = PrevTails,
                NextTails = NextTails,
            }.Schedule(Springs.Length, 1, handle);
        }

        public void InitializeJointsLocalRotation(FastSpringBoneBuffer buffer)
        {
            var logicsIndex = 0;
            for (var i = 0; i < _batchedBuffers.Length; ++i)
            {
                var length = _batchedBufferLogicSizes[i];
                if (_batchedBuffers[i] == buffer)
                {
                    Debug.Assert(length == buffer.Logics.Length);
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

        public void DrawGizmos()
        {
            foreach (var spring in _springs)
            {
                for (int i = spring.colliderSpan.startIndex; i < spring.colliderSpan.EndIndex; ++i)
                {
                    var collider = _colliders[i];
                    collider.DrawGizmo(_transforms[spring.transformIndexOffset + collider.transformIndex]);
                }

                for (int i = spring.logicSpan.startIndex; i < spring.logicSpan.EndIndex; ++i)
                {
                    var joint = _logics[i];
                    joint.DrawGizmo(_transforms[spring.transformIndexOffset + joint.tailTransformIndex], _joints[i]);

                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawLine(
                        _transforms[spring.transformIndexOffset + joint.tailTransformIndex].position,
                        _transforms[spring.transformIndexOffset + joint.headTransformIndex].position);
                }
            }
        }
    }
}
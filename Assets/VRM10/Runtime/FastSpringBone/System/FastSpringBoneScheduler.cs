using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace UniVRM10.FastSpringBones.System
{
    public sealed class FastSpringBoneScheduler : IDisposable
    {
        private readonly FastSpringBoneBufferCombiner _bufferCombiner;
        public FastSpringBoneScheduler(FastSpringBoneBufferCombiner bufferCombiner)
        {
            _bufferCombiner = bufferCombiner;
        }

        /// <summary>
        /// SpringBone の依存関係のある Job を直列にスケジュールする
        /// 
        /// 1. ReconstructIfDirty
        /// 2. PullTransformJob
        /// 3. UpdateFastSpringBoneJob
        /// 4. PushTransformJob
        /// 
        /// </summary>
        public JobHandle Schedule(float deltaTime)
        {
            var handle0 = _bufferCombiner.ReconstructIfDirty(default);
            if (!_bufferCombiner.HasBuffer)
            {
                return handle0;
            }

            var handle1 = new PullTransformJob
            {
                Transforms = _bufferCombiner.Transforms
            }.Schedule(_bufferCombiner.TransformAccessArray, handle0);

            var handle2 = new UpdateFastSpringBoneJob
            {
                Colliders = _bufferCombiner.Colliders,
                Joints = _bufferCombiner.Joints,
                Logics = _bufferCombiner.Logics,
                Springs = _bufferCombiner.Springs,
                Transforms = _bufferCombiner.Transforms,
                DeltaTime = deltaTime,
            }.Schedule(_bufferCombiner.Springs.Length, 1, handle1);

            var handle3 = new PushTransformJob
            {
                Transforms = _bufferCombiner.Transforms
            }.Schedule(_bufferCombiner.TransformAccessArray, handle2);

            return handle3;
        }

        public void Dispose()
        {
            _bufferCombiner.Dispose();
        }
    }
}
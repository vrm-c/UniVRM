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

        public JobHandle Schedule()
        {
            var handle = default(JobHandle);
            handle = _bufferCombiner.ReconstructIfDirty(handle);
            
            handle = new PullTransformJob
                {
                    Transforms = _bufferCombiner.Transforms
                }.Schedule(_bufferCombiner.TransformAccessArray, handle);
            
            handle = new UpdateFastSpringBoneJob
            {
                Colliders = _bufferCombiner.Colliders,
                Joints = _bufferCombiner.Joints,
                Logics = _bufferCombiner.Logics,
                Springs = _bufferCombiner.Springs,
                Transforms = _bufferCombiner.Transforms,
                DeltaTime = Time.deltaTime,
            }.Schedule(_bufferCombiner.Springs.Length, 1, handle);

            handle = new PushTransformJob
                {
                    Transforms = _bufferCombiner.Transforms
                }.Schedule(_bufferCombiner.TransformAccessArray, handle);

            return handle;
        }

        public void Dispose()
        {
            _bufferCombiner.Dispose();
        }
    }
}
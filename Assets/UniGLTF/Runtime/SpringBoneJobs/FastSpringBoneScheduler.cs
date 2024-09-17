using System;
using Unity.Jobs;
using UnityEngine.Jobs;


namespace UniGLTF.SpringBoneJobs
{
    public sealed class FastSpringBoneScheduler : IDisposable
    {
        private readonly FastSpringBoneBufferCombiner _bufferCombiner;
        public FastSpringBoneScheduler(FastSpringBoneBufferCombiner bufferCombiner)
        {
            _bufferCombiner = bufferCombiner;
        }

        public void Dispose()
        {
            _bufferCombiner.Dispose();
        }

        public JobHandle Schedule(float deltaTime)
        {
            var handle = _bufferCombiner.ReconstructIfDirty(default);
            if (!_bufferCombiner.HasBuffer)
            {
                return handle;
            }

            handle = new PullTransformJob
            {
                Transforms = _bufferCombiner.Transforms
            }.Schedule(_bufferCombiner.TransformAccessArray, handle);

            _bufferCombiner.FlipBuffer();

            handle = new UpdateFastSpringBoneJob
            {
                Joints = _bufferCombiner.Joints,
                Logics = _bufferCombiner.Logics,
                CurrentTail = _bufferCombiner.CurrentTails,
                PrevTail = _bufferCombiner.PrevTails,
                NextTail = _bufferCombiner.NextTails,
                Springs = _bufferCombiner.Springs,
                Colliders = _bufferCombiner.Colliders,
                Transforms = _bufferCombiner.Transforms,
                DeltaTime = deltaTime,
            }.Schedule(_bufferCombiner.Springs.Length, 1, handle);

            handle = new PushTransformJob
            {
                Transforms = _bufferCombiner.Transforms
            }.Schedule(_bufferCombiner.TransformAccessArray, handle);

            return handle;
        }
    }
}
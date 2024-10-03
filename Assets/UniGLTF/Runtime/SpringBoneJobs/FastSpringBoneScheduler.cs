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

            if (_bufferCombiner.Combined is FastSpringBoneCombinedBuffer combined)
            {
                handle = new PullTransformJob
                {
                    Transforms = combined.Transforms
                }.Schedule(combined.TransformAccessArray, handle);

                combined.FlipBuffer();

                handle = new UpdateFastSpringBoneJob
                {
                    Joints = combined.Joints,
                    Logics = combined.Logics,
                    CurrentTail = combined.CurrentTails,
                    PrevTail = combined.PrevTails,
                    NextTail = combined.NextTails,
                    Springs = combined.Springs,
                    Models = combined.Models,
                    Colliders = combined.Colliders,
                    Transforms = combined.Transforms,
                    DeltaTime = deltaTime,
                }.Schedule(combined.Springs.Length, 1, handle);

                handle = new PushTransformJob
                {
                    Transforms = combined.Transforms
                }.Schedule(combined.TransformAccessArray, handle);
            }

            return handle;
        }
    }
}
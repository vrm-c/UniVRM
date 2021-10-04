using System;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace UniVRM10.FastSpringBones.System
{
    public sealed class FastSpringBoneScheduler : IDisposable
    {
        private readonly FastSpringBoneBufferCombiner _fastSpringBoneBufferCombiner;

        public FastSpringBoneScheduler(FastSpringBoneBufferCombiner fastSpringBoneBufferCombiner)
        {
            _fastSpringBoneBufferCombiner = fastSpringBoneBufferCombiner;
        }

        public JobHandle Schedule()
        {
            var handle = new PullTransformJob
                {
                    Transforms = _fastSpringBoneBufferCombiner.Transforms
                }.Schedule(_fastSpringBoneBufferCombiner.TransformAccessArray);
            
            handle = new UpdateFastSpringBoneJob
            {
                Colliders = _fastSpringBoneBufferCombiner.Colliders,
                Joints = _fastSpringBoneBufferCombiner.Joints,
                Springs = _fastSpringBoneBufferCombiner.Springs,
                Transforms = _fastSpringBoneBufferCombiner.Transforms,
            }.Schedule(_fastSpringBoneBufferCombiner.Springs.Length, 1, handle);

            handle = new PushTransformJob
                {
                    Transforms = _fastSpringBoneBufferCombiner.Transforms
                }.Schedule(_fastSpringBoneBufferCombiner.TransformAccessArray, handle);

            return handle;
        }

        public void Dispose()
        {
            _fastSpringBoneBufferCombiner.Dispose();
        }
    }
}
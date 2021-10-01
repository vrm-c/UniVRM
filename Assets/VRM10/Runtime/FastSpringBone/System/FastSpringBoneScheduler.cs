using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace UniVRM10.FastSpringBones.System
{
    public sealed class FastSpringBoneScheduler : IDisposable
    {
        private readonly FastSpringBoneBuffers _fastSpringBoneBuffers;

        public FastSpringBoneScheduler(IReadOnlyList<FastSpringBoneSpring> springs)
        {
            _fastSpringBoneBuffers = new FastSpringBoneBuffers(springs);
        }

        public JobHandle Schedule()
        {
            var handle = JobHandle.CombineDependencies(
                new PullTransformJob
                {
                    Transforms = _fastSpringBoneBuffers.ColliderTransforms
                }.Schedule(_fastSpringBoneBuffers.ColliderTransformAccessArray),
                new PullTransformJob
                {
                    Transforms = _fastSpringBoneBuffers.JointTransforms
                }.Schedule(_fastSpringBoneBuffers.JointsTransformAccessArray));
            
            handle = new UpdateFastSpringBoneJob
            {
                Colliders = _fastSpringBoneBuffers.Colliders,
                Joints = _fastSpringBoneBuffers.Joints,
                Springs = _fastSpringBoneBuffers.Springs,
                ColliderTransforms = _fastSpringBoneBuffers.ColliderTransforms,
                JointTransforms = _fastSpringBoneBuffers.JointTransforms
            }.Schedule(_fastSpringBoneBuffers.Springs.Length, 1, handle);

            handle = JobHandle.CombineDependencies(
                new PushTransformJob
                {
                    Transforms = _fastSpringBoneBuffers.ColliderTransforms
                }.Schedule(_fastSpringBoneBuffers.ColliderTransformAccessArray, handle),
                new PushTransformJob
                {
                    Transforms = _fastSpringBoneBuffers.JointTransforms
                }.Schedule(_fastSpringBoneBuffers.JointsTransformAccessArray, handle));

            return handle;
        }

        public void Dispose()
        {
            _fastSpringBoneBuffers.Dispose();
        }
    }
}
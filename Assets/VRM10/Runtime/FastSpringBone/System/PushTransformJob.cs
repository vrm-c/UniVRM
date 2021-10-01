using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    [BurstCompile]
    public struct PushTransformJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<BlittableTransform> Transforms;

        public void Execute(int index, TransformAccess transform)
        {
            transform.localRotation = Transforms[index].localRotation;
        }
    }
}
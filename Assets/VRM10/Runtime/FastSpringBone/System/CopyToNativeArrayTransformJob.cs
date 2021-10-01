using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    [BurstCompile]
    public struct PullTransformJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<BlittableTransform> Transforms;

        public void Execute(int index, TransformAccess transform)
        {
            Transforms[index] = new BlittableTransform
            {
                position = transform.position,
                rotation = transform.rotation,
                localPosition = transform.localPosition,
                localRotation = transform.localRotation,
                localScale = transform.localScale,
                localToWorldMatrix = transform.localToWorldMatrix,
                worldToLocalMatrix = transform.worldToLocalMatrix
            };
        }
    }
}
    
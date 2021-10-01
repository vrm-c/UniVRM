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
                Position = transform.position,
                Rotation = transform.rotation,
                LocalPosition = transform.localPosition,
                LocalRotation = transform.localRotation,
                LocalScale = transform.localScale,
                LocalToWorldMatrix = transform.localToWorldMatrix,
                WorldToLocalMatrix = transform.worldToLocalMatrix
            };
        }
    }
}
    
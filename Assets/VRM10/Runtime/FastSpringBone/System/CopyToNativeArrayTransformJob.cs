using Unity.Collections;
using UnityEngine.Jobs;
using UniVRM10.FastSpringBones.Blittables;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif

namespace UniVRM10.FastSpringBones.System
{

#if ENABLE_SPRINGBONE_BURST
    [BurstCompile]
#endif
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
    
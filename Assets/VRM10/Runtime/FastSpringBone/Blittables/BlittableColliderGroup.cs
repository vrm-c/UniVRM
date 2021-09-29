using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// VRMSpringBoneのColliderGroupをBlittableにしたもの
    /// </summary>
    public readonly struct BlittableColliderGroup
    {
        public BlittableColliders Colliders { get; }
        public unsafe BlittableTransform* Transform { get; }

        public unsafe BlittableColliderGroup(NativeArray<BlittableCollider> colliders, BlittableTransform* transform)
        {
            Colliders = new BlittableColliders((BlittableCollider*)colliders.GetUnsafePtr(), colliders.Length);
            Transform = transform;
        }
    }
}
using System;
using Unity.Collections;
using VRM.FastSpringBones.Blittables;

namespace VRM.FastSpringBones.NativeWrappers
{
    /// <summary>
    /// BlittableColliderGroupのライフサイクルを管理するWrapper
    /// </summary>
    public sealed unsafe class NativeColliderGroup : IDisposable
    {
        private readonly NativePointer<BlittableColliderGroup> _nativePointer;
        
        private NativeArray<BlittableCollider> Colliders { get; }

        public BlittableColliderGroup* GetUnsafePtr() => _nativePointer.GetUnsafePtr();

        public NativeColliderGroup(BlittableCollider[] colliders, NativeTransform nativeTransform)
        {
            Colliders = new NativeArray<BlittableCollider>(colliders, Allocator.Persistent);
            _nativePointer = new NativePointer<BlittableColliderGroup>(new BlittableColliderGroup(Colliders, nativeTransform.GetUnsafePtr()));
        }

        public void Dispose()
        {
            if (Colliders.IsCreated) Colliders.Dispose();

            _nativePointer.Dispose();
        }
    }
}

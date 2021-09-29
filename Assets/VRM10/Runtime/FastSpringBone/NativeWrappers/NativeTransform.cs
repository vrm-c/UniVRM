using System;
using UnityEngine;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.Registries;

namespace VRM.FastSpringBones.NativeWrappers
{
    /// <summary>
    /// BlittableTransformのライフサイクルを管理するWrapper
    /// </summary>
    public sealed class NativeTransform : IDisposable
    {
        private readonly NativePointer<BlittableTransform> _nativePointer;
        public Transform Transform { get; }

        private readonly TransformRegistry _transformRegistry;

        public unsafe BlittableTransform* GetUnsafePtr() => _nativePointer.GetUnsafePtr();
        public BlittableTransform Value => _nativePointer.Value;

        public unsafe NativeTransform(
            TransformRegistry transformRegistry,
            TransformSynchronizationType transformSynchronizationType,
            Transform transform,
            NativeTransform parent = null
        )
        {
            _nativePointer = new NativePointer<BlittableTransform>(new BlittableTransform(parent != null ? parent.GetUnsafePtr() : null, transform));

            Transform = transform;

            _transformRegistry = transformRegistry;
            _transformRegistry.Register(this, transformSynchronizationType);
        }

        public void Dispose()
        {
            _transformRegistry.Unregister(this);

            _nativePointer.Dispose();
        }
    }
}

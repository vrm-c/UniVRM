using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace VRM.FastSpringBones.NativeWrappers
{
    public sealed class NativePointer<T> : IDisposable where T : unmanaged
    {
        private readonly unsafe T* _unsafePtr;

        public unsafe T* GetUnsafePtr() => _unsafePtr;

        public unsafe T Value
        {
            get => *_unsafePtr;
            set => *_unsafePtr = value;
        }

        public unsafe NativePointer()
        {
            _unsafePtr = (T*)UnsafeUtility.Malloc(sizeof(T), 16, Allocator.Persistent);
        }

        public unsafe NativePointer(T value)
        {
            _unsafePtr = (T*)UnsafeUtility.Malloc(sizeof(T), 16, Allocator.Persistent);
            Value = value;
        }

        public unsafe void Dispose()
        {
            UnsafeUtility.Free(_unsafePtr, Allocator.Persistent);
        }
    }
}

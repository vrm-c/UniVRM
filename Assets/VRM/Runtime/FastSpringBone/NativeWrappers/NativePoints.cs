using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using VRM.FastSpringBones.Blittables;

namespace VRM.FastSpringBones.NativeWrappers
{
    /// <summary>
    /// BlittablePointsGroupのライフサイクルを管理するWrapper
    /// </summary>
    public sealed unsafe class NativePoints : IDisposable
    {
        private readonly NativePointer<BlittablePoints> _nativePointer;
        private NativeArray<BlittablePoint> _buffer;

        public BlittablePoints* GetUnsafePtr() => _nativePointer.GetUnsafePtr();

        public NativePoints(IList<NativePointer<BlittablePoint>> points)
        {
            _buffer = new NativeArray<BlittablePoint>(points.Count, Allocator.Persistent);
            for (var i = 0; i < _buffer.Length; ++i)
            {
                _buffer[i] = points[i].Value;
            }
            
            _nativePointer = new NativePointer<BlittablePoints>(new BlittablePoints((BlittablePoint*) _buffer.GetUnsafePtr(), _buffer.Length));
        }

        public void Dispose()
        {
            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
            }
            _nativePointer.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.Components;

namespace VRM.FastSpringBones.NativeWrappers
{
    /// <summary>
    /// BlittableColliderGroupsのライフサイクルを管理するWrapper
    /// </summary>
    public sealed unsafe class NativeColliderGroups : IDisposable
    {
        private readonly NativePointer<BlittableColliderGroups> _nativePointer = new NativePointer<BlittableColliderGroups>();
        private NativeArray<BlittableColliderGroup> _colliderGroupArray;
        private IReadOnlyList<FastSpringBoneColliderGroup> _colliderGroups;

        //Disposeされた後にUpdateColliderGroupsが呼ばれるのを防ぐためのフラグ
        private bool _isDisposed;

        public BlittableColliderGroups* GetUnsafePtr() => _nativePointer.GetUnsafePtr(); 
        public void DrawGizmos() => _nativePointer.Value.DrawGizmos();

        public IReadOnlyList<FastSpringBoneColliderGroup> ColliderGroups
        {
            get => _colliderGroups;
            set
            {
                _colliderGroups = value;
                UpdateColliderGroups();
            }
        }

        private void UpdateColliderGroups()
        {
            if (_isDisposed) return;
            if (_colliderGroupArray.IsCreated)
            {
                _colliderGroupArray.Dispose();
            }
            CreateColliderGroupArray(_colliderGroups);
            UpdateData();
        }

        public NativeColliderGroups(IReadOnlyList<FastSpringBoneColliderGroup> colliderGroups) 
        {
            _colliderGroups = colliderGroups;
            UpdateColliderGroups();
        }

        public void Dispose()
        {
            if (_colliderGroupArray.IsCreated)
            {
                _colliderGroupArray.Dispose();
                _isDisposed = true;
            }
            _nativePointer.Dispose();
        }

        private void CreateColliderGroupArray(IReadOnlyList<FastSpringBoneColliderGroup> colliderGroups)
        {
            _colliderGroupArray = new NativeArray<BlittableColliderGroup>(colliderGroups.Count, Allocator.Persistent);
            for (var i = 0; i < _colliderGroupArray.Length; ++i)
            {
                _colliderGroupArray[i] = *colliderGroups[i].ColliderGroupPtr;
            }
        }

        private void UpdateData()
        {
            _nativePointer.Value = new BlittableColliderGroups(
                (BlittableColliderGroup*)_colliderGroupArray.GetUnsafePtr(),
                _colliderGroupArray.Length);
        }
    }
}

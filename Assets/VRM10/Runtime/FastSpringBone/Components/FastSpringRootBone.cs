using System;
using System.Collections.Generic;
using UnityEngine;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.NativeWrappers;
using VRM.FastSpringBones.Registries;

namespace VRM.FastSpringBones.Components
{
    /// <summary>
    /// SpringBoneの1本の毛束の処理を担当するクラス
    /// </summary>
    public sealed class FastSpringRootBone : IDisposable
    {
        private readonly TransformRegistry _transformRegistry;
        private readonly RootBoneRegistry _rootBoneRegistry;
        private readonly ColliderGroupRegistry _colliderGroupRegistry;
        private readonly Transform _transform;

        private float _radius;
        private NativeTransform _center;

        private IReadOnlyDictionary<Transform, int> _transformIndexMap;
        private NativeColliderGroups _nativeColliderGroups;

        private NativePoints _nativePoints;
        private NativePointer<BlittableRootBone> _rootBoneWrapper;

        private readonly IList<NativeTransform> _transformWrappers = new List<NativeTransform>();
        private readonly IList<NativePointer<BlittablePoint>> _points = new List<NativePointer<BlittablePoint>>();

        public FastSpringRootBone(
            TransformRegistry transformRegistry,
            Transform transform,
            RootBoneRegistry rootBoneRegistry,
            ColliderGroupRegistry colliderGroupRegistry
        )
        {
            _transformRegistry = transformRegistry;
            _transform = transform;
            _rootBoneRegistry = rootBoneRegistry;
            _colliderGroupRegistry = colliderGroupRegistry;
        }

        public IReadOnlyList<FastSpringBoneColliderGroup> ColliderGroups
        {
            get => _nativeColliderGroups.ColliderGroups;
            set => _nativeColliderGroups.ColliderGroups = value;
        }

        public unsafe void Initialize(
            float gravityPower,
            Vector3 gravityDir,
            float dragForce,
            float stiffnessForce,
            IReadOnlyList<FastSpringBoneColliderGroup> colliderGroups,
            float radius,
            Transform center)
        {
            _radius = radius;
            if (center != null)
            {
                _center = new NativeTransform(_transformRegistry, TransformSynchronizationType.PullOnly, center);
            }

            _nativeColliderGroups = new NativeColliderGroups(colliderGroups);

            NativeTransform parent = null;
            if (_transform.parent)
            {
                parent = new NativeTransform(_transformRegistry, TransformSynchronizationType.PullOnly, _transform.parent);
            }
            SetupRecursive(_transform, parent);

            _nativePoints = new NativePoints(_points);

            _rootBoneWrapper = new NativePointer<BlittableRootBone>(new BlittableRootBone(gravityPower, gravityDir, dragForce, stiffnessForce, _nativePoints.GetUnsafePtr()));
            _rootBoneRegistry.Register(_rootBoneWrapper);
            _colliderGroupRegistry.Register(_nativeColliderGroups);
        }

        public void Dispose()
        {
            _colliderGroupRegistry.Unregister(_nativeColliderGroups);
            _rootBoneRegistry.Unregister(_rootBoneWrapper);

            foreach (var transformWrapper in _transformWrappers)
            {
                transformWrapper.Dispose();
            }

            foreach (var point in _points)
            {
                point.Dispose();
            }

            _center?.Dispose();
            _nativeColliderGroups?.Dispose();
            _nativePoints.Dispose();
            _rootBoneWrapper.Dispose();
        }

        private unsafe void SetupRecursive(Transform trs, NativeTransform parent = null)
        {
            var transformWrapper = new NativeTransform(_transformRegistry, TransformSynchronizationType.PushOnly, trs, parent);
            _transformWrappers.Add(transformWrapper);

            var point = new NativePointer<BlittablePoint>(
                new BlittablePoint(
                    trs,
                    _radius,
                    _center != null ? _center.GetUnsafePtr() : null,
                    _nativeColliderGroups.GetUnsafePtr(),
                    transformWrapper.GetUnsafePtr())
            );
            _points.Add(point);

            for (var i = 0; i < trs.childCount; ++i)
            {
                SetupRecursive(trs.GetChild(i), transformWrapper);
            }
        }
    }
}

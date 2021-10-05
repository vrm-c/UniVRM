using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    /// <summary>
    /// FastSpringBoneの処理に利用するバッファを全て結合して持つクラス
    /// </summary>
    public sealed class FastSpringBoneBufferCombiner : IDisposable
    {
        private NativeArray<BlittableSpring> _springs;
        private NativeArray<BlittableJoint> _joints;
        private NativeArray<BlittableTransform> _transforms;
        private TransformAccessArray _transformAccessArray;
        private NativeArray<BlittableCollider> _colliders;
        private NativeArray<BlittableLogic> _logics;

        private readonly LinkedList<FastSpringBoneScope> _fastSpringBoneScopes = new LinkedList<FastSpringBoneScope>();

        private bool _isDirty;

        public NativeArray<BlittableSpring> Springs => _springs;
        public NativeArray<BlittableJoint> Joints => _joints;
        public NativeArray<BlittableTransform> Transforms => _transforms;
        public TransformAccessArray TransformAccessArray => _transformAccessArray;
        public NativeArray<BlittableCollider> Colliders => _colliders;
        public NativeArray<BlittableLogic> Logics => _logics;

        public void Register(FastSpringBoneScope scope)
        {
            _fastSpringBoneScopes.AddLast(scope);
            _isDirty = true;
        }

        public void Unregister(FastSpringBoneScope scope)
        {
            _fastSpringBoneScopes.Remove(scope);
            _isDirty = true;
        }

        /// <summary>
        /// 変更があったならばバッファを再構築する
        /// </summary>
        public void ReconstructIfDirty()
        {
            if (_isDirty)
            {
                ReconstructBuffers();
                _isDirty = false;
            }
        }

        /// <summary>
        /// バッファを再構築する
        /// TODO: 最適化
        /// </summary>
        private void ReconstructBuffers()
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers");
            DisposeAllBuffers();

            var springs = _fastSpringBoneScopes.SelectMany(scope => scope.Springs).ToArray();

            // Transformの列挙
            var transformHashSet = new HashSet<Transform>();
            foreach (var spring in springs)
            {
                foreach (var joint in spring.joints)
                {
                    transformHashSet.Add(joint.Transform);
                    if (joint.Transform.parent != null) transformHashSet.Add(joint.Transform.parent);
                }
                foreach (var collider in spring.colliders)
                {
                    transformHashSet.Add(collider.Transform);
                }

                if (spring.center != null) transformHashSet.Add(spring.center);
            }
            var transforms = transformHashSet.ToArray();
            var transformIndexDictionary = transforms.Select((trs, index) => (trs, index))
                .ToDictionary(tuple => tuple.trs, tuple => tuple.index);

            // 各種bufferの構築
            var blittableColliders = new List<BlittableCollider>();
            var blittableJoints = new List<BlittableJoint>();
            var blittableSprings = new List<BlittableSpring>();
            var blittableLogics = new List<BlittableLogic>();

            foreach (var spring in springs)
            {
                var blittableSpring = new BlittableSpring
                {
                    colliderSpan = new BlittableSpan
                    {
                        startIndex = blittableColliders.Count,
                        count = spring.colliders.Length,
                    },
                    logicSpan = new BlittableSpan
                    {
                        startIndex = blittableJoints.Count,
                        count = spring.joints.Length - 1,
                    },
                    centerTransformIndex = spring.center ? transformIndexDictionary[spring.center] : -1
                };
                blittableSprings.Add(blittableSpring);

                blittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = transformIndexDictionary[collider.Transform];
                    return blittable;
                }));
                blittableJoints.AddRange(spring.joints.Take(spring.joints.Length - 1).Select(joint =>
                {
                    var blittable = joint.Joint;
                    return blittable;
                }));

                for (var i = 0; i < spring.joints.Length - 1; ++i)
                {
                    var joint = spring.joints[i];
                    var tailJoint = spring.joints[i + 1];
                    var localPosition = tailJoint.Transform.localPosition;
                    var scale = tailJoint.Transform.lossyScale;
                    var localChildPosition =
                        new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                        );

                    var worldChildPosition = joint.Transform.TransformPoint(localChildPosition);
                    var currentTail = spring.center != null
                        ? spring.center.InverseTransformPoint(worldChildPosition)
                        : worldChildPosition;
                    var parent = joint.Transform.parent;
                    blittableLogics.Add(new BlittableLogic
                    {
                        headTransformIndex = transformIndexDictionary[joint.Transform],
                        parentTransformIndex = parent != null ? transformIndexDictionary[parent] : -1,
                        currentTail = currentTail,
                        prevTail = currentTail,
                        localRotation = joint.Transform.localRotation,
                        boneAxis = localChildPosition.normalized,
                        length = localChildPosition.magnitude
                    });
                }
            }

            // 各種bufferの初期化
            _springs = new NativeArray<BlittableSpring>(blittableSprings.ToArray(), Allocator.Persistent);

            _joints = new NativeArray<BlittableJoint>(blittableJoints.ToArray(), Allocator.Persistent);
            _colliders = new NativeArray<BlittableCollider>(blittableColliders.ToArray(), Allocator.Persistent);
            _logics = new NativeArray<BlittableLogic>(blittableLogics.ToArray(), Allocator.Persistent);

            _transforms = new NativeArray<BlittableTransform>(transforms.Length, Allocator.Persistent);
            _transformAccessArray = new TransformAccessArray(transforms.ToArray());

            Profiler.EndSample();
        }

        private void DisposeAllBuffers()
        {
            if (_springs.IsCreated) _springs.Dispose();
            if (_joints.IsCreated) _joints.Dispose();
            if (_transforms.IsCreated) _transforms.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_colliders.IsCreated) _colliders.Dispose();
            if (_logics.IsCreated) _logics.Dispose();
        }

        public void Dispose()
        {
            DisposeAllBuffers();
        }
    }
}
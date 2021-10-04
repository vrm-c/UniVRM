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
    /// FastSpringBoneの処理に利用するバッファを全て結合して持ち、必要に応じて返すクラス
    /// </summary>
    public sealed class FastSpringBoneBufferCombiner : IDisposable
    {
        private NativeArray<BlittableSpring> _springs;
        private NativeArray<BlittableJoint> _joints;
        private NativeArray<BlittableTransform> _transforms;
        private TransformAccessArray _transformAccessArray;
        private NativeArray<BlittableCollider> _colliders;

        private readonly LinkedList<FastSpringBoneScope> _fastSpringBoneScopes = new LinkedList<FastSpringBoneScope>();

        public NativeArray<BlittableSpring> Springs => _springs;
        public NativeArray<BlittableJoint> Joints => _joints;
        public NativeArray<BlittableTransform> Transforms => _transforms;
        public TransformAccessArray TransformAccessArray => _transformAccessArray;
        public NativeArray<BlittableCollider> Colliders => _colliders;

        public void Register(FastSpringBoneScope scope)
        {
            _fastSpringBoneScopes.AddLast(scope);
            ReconstructBuffers();
        }

        public void Unregister(FastSpringBoneScope scope)
        {
            _fastSpringBoneScopes.Remove(scope);
            ReconstructBuffers();
        }

        private void ReconstructBuffers()
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers");
            if (_springs.IsCreated) _springs.Dispose();
            if (_joints.IsCreated) _joints.Dispose();
            if (_transforms.IsCreated) _transforms.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_colliders.IsCreated) _colliders.Dispose();

            var springs = _fastSpringBoneScopes.SelectMany(scope => scope.Springs).ToArray();
            var transforms = springs.SelectMany(spring =>
                Enumerable.Concat(
                    spring.joints.Select(joint => joint.Transform),
                    spring.colliders.Select(collider => collider.Transform)
                )
            ).Distinct().ToArray();

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
                        count = spring.colliders.Length,
                        startIndex = blittableColliders.Count
                    },
                    logicSpan = new BlittableSpan
                    {
                        count = spring.joints.Length,
                        startIndex = blittableJoints.Count - 1
                    },
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
                    blittableLogics.Add(new BlittableLogic
                    {
                        headIndex = transformIndexDictionary[joint.Transform],
                        tailIndex = transformIndexDictionary[tailJoint.Transform],
                        currentTail = spring.center != null
                            ? spring.center.InverseTransformPoint(worldChildPosition)
                            : worldChildPosition,
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

            _transforms = new NativeArray<BlittableTransform>(transforms.Length, Allocator.Persistent);
            _transformAccessArray = new TransformAccessArray(transforms.Length);


            foreach (var trs in transforms)
            {
                _transformAccessArray.Add(trs);
            }

            Profiler.EndSample();
        }

        public void Dispose()
        {
            _springs.Dispose();
            _joints.Dispose();
            _colliders.Dispose();
            _transforms.Dispose();
            _transformAccessArray.Dispose();
        }
    }
}
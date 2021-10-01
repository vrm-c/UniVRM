using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.Jobs;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    public sealed class FastSpringBoneBuffers : IDisposable
    {
        private NativeArray<BlittableSpring> _springs;

        private NativeArray<BlittableJoint> _joints;
        private NativeArray<BlittableTransform> _jointTransforms;
        private TransformAccessArray _jointsTransformAccessArray;

        private NativeArray<BlittableCollider> _colliders;
        private NativeArray<BlittableTransform> _colliderTransforms;
        private TransformAccessArray _colliderTransformAccessArray;

        public NativeArray<BlittableSpring> Springs => _springs;
        
        public NativeArray<BlittableJoint> Joints => _joints;
        public NativeArray<BlittableTransform> JointTransforms => _jointTransforms;
        public TransformAccessArray JointsTransformAccessArray => _jointsTransformAccessArray;
        
        public NativeArray<BlittableCollider> Colliders => _colliders;
        public NativeArray<BlittableTransform> ColliderTransforms => _colliderTransforms;
        public TransformAccessArray ColliderTransformAccessArray => _colliderTransformAccessArray;

        public FastSpringBoneBuffers(IReadOnlyList<FastSpringBoneSpring> springs)
        {
            _springs = new NativeArray<BlittableSpring>(springs.Count, Allocator.Persistent);

            // _springsを構築しつつ、joints, collidersの総数を数える
            ConstructBlittableSprings(springs, out var jointsCount, out var collidersCount);

            // 各種bufferの初期化
            _joints = new NativeArray<BlittableJoint>(jointsCount, Allocator.Persistent);
            _jointTransforms = new NativeArray<BlittableTransform>(jointsCount, Allocator.Persistent);
            _jointsTransformAccessArray = new TransformAccessArray(jointsCount);

            _colliders = new NativeArray<BlittableCollider>(collidersCount, Allocator.Persistent);
            _colliderTransforms = new NativeArray<BlittableTransform>(collidersCount, Allocator.Persistent);
            _colliderTransformAccessArray = new TransformAccessArray(collidersCount);

            // 各種bufferの構築
            var jointIndex = 0;
            var colliderIndex = 0;
            foreach (var spring in springs)
            {
                foreach (var joint in spring.Joints)
                {
                    var parent = joint.Transform.parent;

                    _joints[jointIndex] = joint.Joint;
                    _jointsTransformAccessArray.Add(joint.Transform);
                    _jointTransforms[jointIndex] = new BlittableTransform();
                    ++jointIndex;
                }

                foreach (var collider in spring.Colliders)
                {
                    var parent = collider.Transform.parent;

                    _colliders[colliderIndex] = collider.Collider;
                    _colliderTransformAccessArray.Add(collider.Transform);
                    _colliderTransforms[colliderIndex] = new BlittableTransform();
                    ++colliderIndex;
                }
            }
        }

        /// <summary>
        /// _springsを構築しつつ、joints, collidersの総数を数える
        /// </summary>
        private void ConstructBlittableSprings(IReadOnlyList<FastSpringBoneSpring> springs, out int jointsCount,
            out int collidersCount)
        {
            jointsCount = 0;
            collidersCount = 0;
            for (var i = 0; i < springs.Count; i++)
            {
                var spring = springs[i];
                var blittableSpring = new BlittableSpring();

                blittableSpring.JointSpan.StartIndex = jointsCount;
                blittableSpring.ColliderSpan.StartIndex = jointsCount;
                blittableSpring.JointSpan.Count = spring.Joints.Length;
                blittableSpring.ColliderSpan.Count = spring.Colliders.Length;
                _springs[i] = blittableSpring;

                jointsCount += spring.Joints.Length;
                collidersCount += spring.Colliders.Length;
            }
        }
        
        public void Dispose()
        {
            _springs.Dispose();
            _joints.Dispose();
            _jointTransforms.Dispose();
            _jointsTransformAccessArray.Dispose();
            _colliders.Dispose();
            _colliderTransforms.Dispose();
            _colliderTransformAccessArray.Dispose();
        }
    }
}
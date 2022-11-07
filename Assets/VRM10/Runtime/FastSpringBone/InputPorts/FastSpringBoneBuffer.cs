using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    /// <summary>
    /// ひとつのVRMに紐づくFastSpringBoneに関連したバッファを保持するクラス
    /// </summary>
    public class FastSpringBoneBuffer : IDisposable
    {
        // NOTE: これらはFastSpringBoneBufferCombinerによってバッチングされる
        public NativeArray<BlittableSpring> Springs { get; }
        public NativeArray<BlittableJoint> Joints { get; }
        public NativeArray<BlittableCollider> Colliders { get; }
        public NativeArray<BlittableLogic> Logics { get; }
        public NativeArray<BlittableTransform> BlittableTransforms { get; }
        public Transform[] Transforms { get; }
        public bool IsDisposed { get; private set; }

        // NOTE: これは更新頻度が高くバッチングが難しいため、ランダムアクセスを許容してメモリへ直接アクセスする
        // 生のヒープ領域は扱いにくいので長さ1のNativeArrayで代用
        private NativeArray<BlittableExternalData> _externalData;

        public BlittableExternalData ExternalData
        {
            get => _externalData[0];
            set => _externalData[0] = value;
        }

        public unsafe FastSpringBoneBuffer(IReadOnlyList<FastSpringBoneSpring> springs,
            BlittableExternalData externalData,
            bool simulateLastBone = false)
        {
            Profiler.BeginSample("FastSpringBone.ConstructBuffers");

            _externalData = new NativeArray<BlittableExternalData>(1, Allocator.Persistent);
            ExternalData = externalData;

            // Transformの列挙
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.ConstructTransformBuffer");
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
            Profiler.EndSample();

            // 各種bufferの構築
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.ConstructBuffers");
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
                        count = simulateLastBone ? spring.joints.Length : spring.joints.Length - 1,
                    },
                    centerTransformIndex = spring.center ? transformIndexDictionary[spring.center] : -1,
                    ExternalData = (BlittableExternalData*) _externalData.GetUnsafePtr()
                };
                blittableSprings.Add(blittableSpring);

                blittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = transformIndexDictionary[collider.Transform];
                    return blittable;
                }));
                blittableJoints.AddRange(spring.joints
                    .Take(simulateLastBone ? spring.joints.Length : spring.joints.Length - 1).Select(joint =>
                    {
                        var blittable = joint.Joint;
                        return blittable;
                    }));

                for (var i = 0; i < (simulateLastBone ? spring.joints.Length : spring.joints.Length - 1); ++i)
                {
                    var joint = spring.joints[i];
                    var tailJoint = i + 1 < spring.joints.Length ? spring.joints[i + 1] : (FastSpringBoneJoint?) null;
                    var parentJoint = i - 1 >= 0 ? spring.joints[i - 1] : (FastSpringBoneJoint?) null;
                    var localPosition = Vector3.zero;
                    if (tailJoint.HasValue)
                    {
                        localPosition = tailJoint.Value.Transform.localPosition;
                    }
                    else
                    {
                        if (parentJoint.HasValue)
                        {
                            var delta = joint.Transform.position - parentJoint.Value.Transform.position;
                            localPosition =
                                joint.Transform.worldToLocalMatrix.MultiplyPoint(joint.Transform.position + delta);
                        }
                        else
                        {
                            localPosition = Vector3.down;
                        }
                    }

                    var scale = tailJoint.HasValue ? tailJoint.Value.Transform.lossyScale : joint.Transform.lossyScale;
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
                        localRotation = joint.DefaultLocalRotation,
                        boneAxis = localChildPosition.normalized,
                        length = localChildPosition.magnitude
                    });
                }
            }

            Profiler.EndSample();

            // 各種bufferの初期化
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.ConstructNativeArrays");
            Springs = new NativeArray<BlittableSpring>(blittableSprings.ToArray(), Allocator.Persistent);

            Joints = new NativeArray<BlittableJoint>(blittableJoints.ToArray(), Allocator.Persistent);
            Colliders = new NativeArray<BlittableCollider>(blittableColliders.ToArray(), Allocator.Persistent);
            Logics = new NativeArray<BlittableLogic>(blittableLogics.ToArray(), Allocator.Persistent);

            BlittableTransforms = new NativeArray<BlittableTransform>(transforms.Length, Allocator.Persistent);
            Transforms = transforms.ToArray();
            Profiler.EndSample();

            Profiler.EndSample();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Springs.Dispose();
            Joints.Dispose();
            BlittableTransforms.Dispose();
            Colliders.Dispose();
            Logics.Dispose();
            _externalData.Dispose();
        }
    }
}
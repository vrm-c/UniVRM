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
        public Vector3 ExternalForce
        {
            get => _externalData[0].ExternalForce;
            set
            {
                _externalData[0] = new BlittableExternalData
                {
                    ExternalForce = value,
                    IsSpringBoneEnabled = _externalData[0].IsSpringBoneEnabled,
                };
            }
        }
        public bool IsSpringBoneEnabled
        {
            get => _externalData[0].IsSpringBoneEnabled;
            set
            {
                _externalData[0] = new BlittableExternalData
                {
                    ExternalForce = _externalData[0].ExternalForce,
                    IsSpringBoneEnabled = value,
                };
            }
        }

        /// <summary>
        /// Joint, Collider, Center の Transform のリスト
        /// - 重複を除去
        /// </summary>
        /// <param name="springs"></param>
        /// <returns></returns>
        static Transform[] MakeFlattenTransformList(FastSpringBoneSpring[] springs)
        {
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
            var Transforms = transformHashSet.ToArray();
            return Transforms;
        }

        public unsafe FastSpringBoneBuffer(FastSpringBoneSpring[] springs)
        {
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.BufferBuilder");
            Transforms = MakeFlattenTransformList(springs);
            _externalData = new NativeArray<BlittableExternalData>(1, Allocator.Persistent);
            _externalData[0] = new BlittableExternalData
            {
                ExternalForce = Vector3.zero,
                IsSpringBoneEnabled = true,
            };

            var externalDataPtr = (BlittableExternalData*)_externalData.GetUnsafePtr();
            List<BlittableSpring> blittableSprings = new();
            List<BlittableJoint> blittableJoints = new();
            List<BlittableCollider> blittableColliders = new();
            List<BlittableLogic> blittableLogics = new();
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
                    centerTransformIndex = Array.IndexOf(Transforms, spring.center),
                    ExternalData = externalDataPtr,
                };
                blittableSprings.Add(blittableSpring);

                blittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = Array.IndexOf(Transforms, collider.Transform);
                    return blittable;
                }));
                blittableJoints.AddRange(spring.joints
                    .Take(spring.joints.Length - 1).Select(joint =>
                    {
                        var blittable = joint.Joint;
                        return blittable;
                    }));

                blittableLogics.AddRange(LogicFromTransform(Transforms, spring));
            }

            Springs = new NativeArray<BlittableSpring>(blittableSprings.ToArray(), Allocator.Persistent);
            Joints = new NativeArray<BlittableJoint>(blittableJoints.ToArray(), Allocator.Persistent);
            Colliders = new NativeArray<BlittableCollider>(blittableColliders.ToArray(), Allocator.Persistent);
            Logics = new NativeArray<BlittableLogic>(blittableLogics.ToArray(), Allocator.Persistent);
            BlittableTransforms = new NativeArray<BlittableTransform>(Transforms.Length, Allocator.Persistent);
            Profiler.EndSample();
        }

        /// <summary>
        /// Transform の現状から Logic を作成する。
        /// </summary>
        /// <param name="Transforms"></param>
        /// <param name="spring"></param>
        /// <param name="i">joint index</param>
        /// <returns></returns>
        public static IEnumerable<BlittableLogic> LogicFromTransform(Transform[] Transforms, FastSpringBoneSpring spring)
        {
            // vrm-1.0 では末端の joint は tail で処理対象でないのに注意!
            for (int i = 0; i < spring.joints.Length - 1; ++i)
            {
                var joint = spring.joints[i];
                var tailJoint = i + 1 < spring.joints.Length ? spring.joints[i + 1] : (FastSpringBoneJoint?)null;
                var parentJoint = i - 1 >= 0 ? spring.joints[i - 1] : (FastSpringBoneJoint?)null;
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
                var localChildPosition = new Vector3(
                        localPosition.x * scale.x,
                        localPosition.y * scale.y,
                        localPosition.z * scale.z
                    );

                var worldChildPosition = joint.Transform.TransformPoint(localChildPosition);
                var currentTail = spring.center != null
                    ? spring.center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition;
                var parent = joint.Transform.parent;

                yield return new BlittableLogic
                {
                    headTransformIndex = Array.IndexOf(Transforms, joint.Transform),
                    parentTransformIndex = Array.IndexOf(Transforms, parent),
                    currentTail = currentTail,
                    prevTail = currentTail, // same with currentTail. velocity zero.
                    localRotation = joint.DefaultLocalRotation,
                    boneAxis = localChildPosition.normalized,
                    length = localChildPosition.magnitude
                };
            }
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

        public void SyncAndZeroVelocity(IReadOnlyList<BlittableLogic> logics)
        {
            var dst = Logics;
            for (int i = 0; i < logics.Count; ++i)
            {
                var l = logics[i];
                l.prevTail = l.currentTail;
                dst[i] = l;
            }
        }
    }
}
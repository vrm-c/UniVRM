using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UniGLTF.SpringBoneJobs.Blittables;

namespace UniGLTF.SpringBoneJobs.InputPorts
{
    /// <summary>
    /// ひとつのVRMに紐づくFastSpringBoneに関連したバッファを保持するクラス
    /// </summary>
    public class FastSpringBoneBuffer : IDisposable
    {
        /// <summary>
        /// model root
        /// </summary>
        public Transform Model { get; }
        // NOTE: これらはFastSpringBoneBufferCombinerによってバッチングされる
        public NativeArray<BlittableSpring> Springs { get; }
        public NativeArray<BlittableJointMutable> Joints { get; }
        public NativeArray<BlittableCollider> Colliders { get; }
        public NativeArray<BlittableJointImmutable> Logics { get; }
        public NativeArray<BlittableTransform> BlittableTransforms { get; }
        public Transform[] Transforms { get; }
        public bool IsDisposed { get; private set; }

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

        public FastSpringBoneBuffer(Transform model, FastSpringBoneSpring[] springs)
        {
            Model = model;

            Profiler.BeginSample("FastSpringBone.ConstructBuffers.BufferBuilder");
            Transforms = MakeFlattenTransformList(springs);

            List<BlittableSpring> blittableSprings = new();
            List<BlittableJointMutable> blittableJoints = new();
            List<BlittableCollider> blittableColliders = new();
            List<BlittableJointImmutable> blittableLogics = new();
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
            Joints = new NativeArray<BlittableJointMutable>(blittableJoints.ToArray(), Allocator.Persistent);
            Colliders = new NativeArray<BlittableCollider>(blittableColliders.ToArray(), Allocator.Persistent);
            Logics = new NativeArray<BlittableJointImmutable>(blittableLogics.ToArray(), Allocator.Persistent);
            BlittableTransforms = new NativeArray<BlittableTransform>(Transforms.Select(transform => new BlittableTransform
            {
                position = transform.position,
                rotation = transform.rotation,
                localPosition = transform.localPosition,
                localRotation = transform.localRotation,
                localScale = transform.localScale,
                localToWorldMatrix = transform.localToWorldMatrix,
                worldToLocalMatrix = transform.worldToLocalMatrix
            }).ToArray(), Allocator.Persistent);
            Profiler.EndSample();
        }

        /// <summary>
        /// Transform の現状から Logic を作成する。
        /// </summary>
        /// <param name="Transforms"></param>
        /// <param name="spring"></param>
        /// <param name="i">joint index</param>
        /// <returns></returns>
        public static IEnumerable<BlittableJointImmutable> LogicFromTransform(Transform[] Transforms, FastSpringBoneSpring spring)
        {
            // vrm-1.0 では末端の joint は tail で処理対象でないのに注意!
            for (int i = 0; i < spring.joints.Length - 1; ++i)
            {
                var joint = spring.joints[i];
                var tailJoint = spring.joints[i + 1];
                var localPosition = tailJoint.Transform.localPosition;

                var scale = tailJoint.Transform.lossyScale;
                var localChildPosition = new Vector3(
                        localPosition.x * scale.x,
                        localPosition.y * scale.y,
                        localPosition.z * scale.z
                    );

                yield return new BlittableJointImmutable
                {
                    headTransformIndex = Array.IndexOf<Transform>(Transforms, joint.Transform),
                    parentTransformIndex = Array.IndexOf<Transform>(Transforms, joint.Transform.parent),
                    tailTransformIndex = Array.IndexOf<Transform>(Transforms, tailJoint.Transform),
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
        }
    }
}
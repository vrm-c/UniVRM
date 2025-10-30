using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.Profiling;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Mathematics;
using UnityEngine;

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
        private NativeArray<float3> _currentTailsBackup;
        private NativeArray<float3> _nextTailsBackup;
        public Transform[] Transforms { get; }

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
                var blittableSpring = new BlittableSpring(
                    centerTransformIndex: Array.IndexOf(Transforms, spring.center),
                    colliderSpan: new BlittableSpan(blittableColliders.Count, spring.colliders.Length),
                    logicSpan: new BlittableSpan(blittableJoints.Count, spring.joints.Length - 1));
                blittableSprings.Add(blittableSpring);

                blittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    var transformIndex = Array.IndexOf(Transforms, collider.Transform);
                    return blittable.SetTransformIndex(transformIndex);
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
                var localChildPosition = new float3(
                        localPosition.x * scale.x,
                        localPosition.y * scale.y,
                        localPosition.z * scale.z
                    );

                yield return new BlittableJointImmutable(
                    headTransformIndex: Array.IndexOf<Transform>(Transforms, joint.Transform),
                    parentTransformIndex: Array.IndexOf<Transform>(Transforms, joint.Transform.parent),
                    tailTransformIndex: Array.IndexOf<Transform>(Transforms, tailJoint.Transform),
                    localRotation: joint.DefaultLocalRotation,
                    boneAxis: math.normalize(localChildPosition),
                    length: math.length(localChildPosition));
            }
        }

        public void BackupCurrentTails(NativeArray<float3> currentTails, NativeArray<float3> nextTails, int offset)
        {
            if (!Logics.IsCreated || Logics.Length == 0)
            {
                return;
            }
            if (!_currentTailsBackup.IsCreated)
            {
                _currentTailsBackup = new(Logics.Length, Allocator.Persistent);
            }
            if (!_nextTailsBackup.IsCreated)
            {
                _nextTailsBackup = new(Logics.Length, Allocator.Persistent);
            }
            NativeArray<float3>.Copy(currentTails, offset, _currentTailsBackup, 0, Logics.Length);
            NativeArray<float3>.Copy(nextTails, offset, _nextTailsBackup, 0, Logics.Length);
        }

        public void RestoreCurrentTails(NativeArray<float3> currentTails, NativeArray<float3> nextTails, int offset)
        {
            if (_currentTailsBackup.IsCreated)
            {
                NativeArray<float3>.Copy(_currentTailsBackup, 0, currentTails, offset, Logics.Length);
                NativeArray<float3>.Copy(_nextTailsBackup, 0, nextTails, offset, Logics.Length);
            }
            else
            {
                var end = offset + Logics.Length;
                for (int i = offset; i < end; ++i)
                {
                    // mark velocity zero
#if UNITY_2022_2_OR_NEWER
                    currentTails.GetSubArray(offset, Logics.Length).AsSpan().Fill(new float3(float.NaN, float.NaN, float.NaN));
#else
                    var subArray = currentTails.GetSubArray(offset, Logics.Length);
                    var value = new Vector3(float.NaN, float.NaN, float.NaN);
                    for (int a = 0; a < subArray.Length; ++a)
                        subArray[a] = value;
#endif
                }
            }
        }

        public void Dispose()
        {
            if (Springs.IsCreated) Springs.Dispose();
            if (Joints.IsCreated) Joints.Dispose();
            if (Colliders.IsCreated) Colliders.Dispose();
            if (Logics.IsCreated) Logics.Dispose();
            if (_currentTailsBackup.IsCreated) _currentTailsBackup.Dispose();
            if (_nextTailsBackup.IsCreated) _nextTailsBackup.Dispose();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    public class FastSpringBoneBufferBuilder
    {
        public readonly List<BlittableSpring> BlittableSprings = new();
        public readonly List<BlittableJoint> BlittableJoints = new();
        public readonly List<BlittableCollider> BlittableColliders = new();
        public readonly List<BlittableLogic> BlittableLogics = new();
        public readonly Transform[] Transforms;
        public struct TransformInfo
        {
            public int Index;
            public Quaternion InitialLocalRotation;
        }
        public readonly IDictionary<Transform, TransformInfo> TransformIndexMap;
        public readonly FastSpringBoneSpring[] Springs;

        public FastSpringBoneBufferBuilder(FastSpringBoneSpring[] springs, bool simulateLastBone = false)
        {
            Springs = springs;
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.BufferBuilder");
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
            Transforms = transformHashSet.ToArray();
            TransformIndexMap = Transforms.Select((trs, index) => (trs, index))
                .ToDictionary(tuple => tuple.trs, tuple => new TransformInfo
                {
                    Index = tuple.index,
                    InitialLocalRotation = tuple.trs.localRotation,
                });

            foreach (var spring in springs)
            {
                var blittableSpring = new BlittableSpring
                {
                    colliderSpan = new BlittableSpan
                    {
                        startIndex = BlittableColliders.Count,
                        count = spring.colliders.Length,
                    },
                    logicSpan = new BlittableSpan
                    {
                        startIndex = BlittableJoints.Count,
                        count = simulateLastBone ? spring.joints.Length : spring.joints.Length - 1,
                    },
                    centerTransformIndex = spring.center ? TransformIndexMap[spring.center].Index : -1,
                };
                BlittableSprings.Add(blittableSpring);

                BlittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = TransformIndexMap[collider.Transform].Index;
                    return blittable;
                }));
                BlittableJoints.AddRange(spring.joints
                    .Take(simulateLastBone ? spring.joints.Length : spring.joints.Length - 1).Select(joint =>
                    {
                        var blittable = joint.Joint;
                        return blittable;
                    }));

                AddLogic(spring, simulateLastBone);

            }
            Profiler.EndSample();
        }

        public void AddLogic(FastSpringBoneSpring spring, bool simulateLastBone = false)
        {
            for (var i = 0; i < (simulateLastBone ? spring.joints.Length : spring.joints.Length - 1); ++i)
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
                BlittableLogics.Add(new BlittableLogic
                {
                    headTransformIndex = TransformIndexMap[joint.Transform].Index,
                    parentTransformIndex = parent != null ? TransformIndexMap[parent].Index : -1,
                    currentTail = currentTail,
                    prevTail = currentTail,
                    localRotation = joint.DefaultLocalRotation,
                    boneAxis = localChildPosition.normalized,
                    length = localChildPosition.magnitude
                });
            }
        }

        public unsafe void SetExternalDataPtr(BlittableExternalData* externalData)
        {
            for (int i = 0; i < BlittableSprings.Count; ++i)
            {
                // blittableSprings[i] = blittableSprings[i] with
                // {
                //     ExternalData = externalData,
                // };
                var b = BlittableSprings[i];
                b.ExternalData = externalData;
                BlittableSprings[i] = b;
            }
        }

        public unsafe void SyncAndZeroVelocity(NativeArray<BlittableLogic> logics)
        {
            for (int i = 0; i < BlittableLogics.Count; ++i)
            {
                var l = BlittableLogics[i];
                l.prevTail = l.currentTail;
                logics[i] = BlittableLogics[i];
            }
        }
    }
}
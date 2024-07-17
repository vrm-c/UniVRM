using System.Collections.Generic;
using System.Linq;
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

        public FastSpringBoneBufferBuilder(IReadOnlyList<FastSpringBoneSpring> springs, bool simulateLastBone = false)
        {
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
            var transformIndexDictionary = Transforms.Select((trs, index) => (trs, index))
                .ToDictionary(tuple => tuple.trs, tuple => tuple.index);

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
                    centerTransformIndex = spring.center ? transformIndexDictionary[spring.center] : -1,
                };
                BlittableSprings.Add(blittableSpring);

                BlittableColliders.AddRange(spring.colliders.Select(collider =>
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = transformIndexDictionary[collider.Transform];
                    return blittable;
                }));
                BlittableJoints.AddRange(spring.joints
                    .Take(simulateLastBone ? spring.joints.Length : spring.joints.Length - 1).Select(joint =>
                    {
                        var blittable = joint.Joint;
                        return blittable;
                    }));

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
                BlittableSprings[i] = new BlittableSpring
                {
                    centerTransformIndex = b.centerTransformIndex,
                    colliderSpan = b.colliderSpan,
                    logicSpan = b.logicSpan,
                    transformIndexOffset = b.transformIndexOffset,
                    ExternalData = externalData,
                };
            }
        }
    }
}
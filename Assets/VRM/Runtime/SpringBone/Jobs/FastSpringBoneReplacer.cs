using System.Collections.Generic;
using UnityEngine;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UniGLTF.Utils;


namespace VRM.SpringBoneJobs
{
    public static class FastSpringBoneReplacer
    {
        public static async Task<FastSpringBoneBuffer> MakeBufferAsync(GameObject root, IAwaitCaller awaitCaller = null, CancellationToken token = default)
        {
            var components = root.GetComponentsInChildren<VRMSpringBone>();

            var springs = new FastSpringBoneSpring[components.Length];
            for (int i = 0; i < components.Length; ++i)
            {
                var component = components[i];
                var colliders = new List<FastSpringBoneCollider>();
                if (component.ColliderGroups != null)
                {
                    foreach (var group in component.ColliderGroups)
                    {
                        if (group == null) continue;
                        if (awaitCaller != null)
                        {
                            await awaitCaller.NextFrame();
                            token.ThrowIfCancellationRequested();
                        }

                        foreach (var collider in group.Colliders)
                        {
                            if (collider == null) continue;

                            var c = new FastSpringBoneCollider
                            {
                                Transform = group.transform,
                                Collider = new BlittableCollider
                                {
                                    offset = collider.Offset,
                                    radius = collider.Radius,
                                    tailOrNormal = default,
                                    colliderType = BlittableColliderType.Sphere
                                }
                            };
                            colliders.Add(c);
                        }
                    }
                }

                var initMap = RuntimeGltfInstance.SafeGetInitialPose(root.transform);

                var joints = new List<FastSpringBoneJoint>();
                foreach (var springRoot in component.RootBones)
                {
                    if (springRoot == null) continue;
                    if (awaitCaller != null)
                    {
                        await awaitCaller.NextFrame();
                        token.ThrowIfCancellationRequested();
                    }

                    Traverse(joints, component, springRoot, initMap);
                }

                var spring = new FastSpringBoneSpring
                {
                    center = component.m_center,
                    colliders = colliders.ToArray(),
                    joints = joints.ToArray(),
                };
                springs[i] = spring;
            }

            return new FastSpringBoneBuffer(root.transform, springs);
        }

        static void Traverse(List<FastSpringBoneJoint> joints, VRMSpringBone spring, Transform joint, IReadOnlyDictionary<Transform, TransformState> initMap)
        {
            joints.Add(new FastSpringBoneJoint
            {
                Transform = joint,
                Joint = new BlittableJointMutable
                {
                    radius = spring.m_hitRadius,
                    dragForce = spring.m_dragForce,
                    gravityDir = spring.m_gravityDir,
                    gravityPower = spring.m_gravityPower,
                    stiffnessForce = spring.m_stiffnessForce
                },
                DefaultLocalRotation = initMap[joint.transform].LocalRotation,
            });
            foreach (Transform child in joint)
            {
                Traverse(joints, spring, child, initMap);
            }
        }
    }
}
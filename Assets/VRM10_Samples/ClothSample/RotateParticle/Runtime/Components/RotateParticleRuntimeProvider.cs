using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;

namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/RotateParticleRuntimeProvider")]
    [DisallowMultipleComponent]
    public class RotateParticleRuntimeProvider : MonoBehaviour, IVrm10SpringBoneRuntimeProvider
    {
        [SerializeField]
        public List<Warp> Warps = new();

        [SerializeField]
        public List<RectCloth> Cloths = new();

        IVrm10SpringBoneRuntime m_runtime;
        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            m_runtime = new RotateParticleSpringboneRuntime();
            return m_runtime;
        }

        public void Reset()
        {
            Warps = GetComponentsInChildren<Warp>().ToList();
            Cloths = GetComponentsInChildren<RectCloth>().ToList();
        }

        void OnDrawGizmos()
        {
            if (m_runtime == null)
            {
                return;
            }
            m_runtime.DrawGizmos();
        }

        public static void FromVrm10(Vrm10Instance instance,
            Func<GameObject, Warp> addWarp,
            Action<UnityEngine.Object> deleteObject)
        {
            foreach (var spring in instance.SpringBone.Springs)
            {
                if (spring.Joints == null || spring.Joints[0] == null)
                {
                    continue;
                }

                var root_joint = spring.Joints[0].gameObject;
                if (root_joint == null)
                {
                    continue;
                }

                var warp = root_joint.GetComponent<Warp>();
                if (warp == null)
                {
                    // var warp = Undo.AddComponent<Warp>(root_joint);
                    warp = addWarp(root_joint);
                    var joints = spring.Joints.Where(x => x != null).ToArray();
                    for (int i = 0; i < joints.Length; ++i)
                    {
                        var joint = joints[i];
                        var settings = new UniGLTF.SpringBoneJobs.Blittables.BlittableJointMutable
                        {
                            dragForce = joint.m_dragForce,
                            gravityDir = joint.m_gravityDir,
                            gravityPower = joint.m_gravityPower,
                            // mod
                            stiffnessForce = joint.m_stiffnessForce * 6,
                        };
                        if (i == 0)
                        {
                            settings.radius = joints[0].m_jointRadius;
                            warp.BaseSettings = settings;
                        }
                        else
                        {
                            // breaking change from vrm-1.0
                            settings.radius = joints[i - 1].m_jointRadius;
                            var useInheritSettings = warp.BaseSettings.Equals(settings);
                            warp.Particles.Add(new Warp.Particle
                            {
                                useInheritSettings = useInheritSettings,
                                OverrideSettings = settings,
                                Transform = joint.transform,
                            });
                        }
                        // Undo.DestroyObjectImmediate(joint);
                        deleteObject(joint);
                    }
                    spring.Joints.Clear();
                    warp.ColliderGroups = spring.ColliderGroups.ToList();
                }
            }
            instance.SpringBone.Springs.Clear();
        }
    }
}
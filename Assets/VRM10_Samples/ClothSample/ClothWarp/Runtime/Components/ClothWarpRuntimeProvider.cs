using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniVRM10.ClothWarp.Components
{
    [AddComponentMenu("ClothWarp/ClothWarpRuntimeProvider")]
    [DisallowMultipleComponent]
    public class ClothWarpRuntimeProvider : MonoBehaviour, IVrm10SpringBoneRuntimeProvider
    {
        [SerializeField]
        public List<ClothWarpRoot> Warps = new();

        [SerializeField]
        public List<ClothGrid> Cloths = new();

        [SerializeField]
        public bool UseJob = true;

        IVrm10SpringBoneRuntime m_runtime;
        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            m_runtime = UseJob
                ? new Jobs.ClothWarpJobRuntime()
                : new ClothWarpRuntime()
                ;
            return m_runtime;
        }

        public void Reset()
        {
            Warps = GetComponentsInChildren<ClothWarpRoot>().ToList();
            Cloths = GetComponentsInChildren<ClothGrid>().ToList();
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
            Func<GameObject, ClothWarpRoot> addWarp)
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

                var warp = root_joint.GetComponent<ClothWarpRoot>();
                if (warp == null)
                {
                    warp = addWarp(root_joint);
                    var joints = spring.Joints.Where(x => x != null).ToArray();
                    for (int i = 0; i < joints.Length; ++i)
                    {
                        var joint = joints[i];

                        // mod ?
                        var stiffness = joint.m_stiffnessForce * 6;

                        var settings = new UniGLTF.SpringBoneJobs.Blittables.BlittableJointMutable
                        {
                            dragForce = joint.m_dragForce,
                            gravityDir = joint.m_gravityDir,
                            gravityPower = joint.m_gravityPower,
                            stiffnessForce = stiffness,
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
                            if (useInheritSettings)
                            {
                                warp.UseBaseSettings(joint.transform);
                            }
                            else
                            {
                                warp.SetSettings(joint.transform, settings);
                            }
                        }
                    }
                    warp.ColliderGroups = spring.ColliderGroups.ToList();
                }
            }
        }
    }
}
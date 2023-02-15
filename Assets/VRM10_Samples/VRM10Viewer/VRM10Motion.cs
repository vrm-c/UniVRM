using System;
using System.IO;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Motion
    {
        Animator m_src = default;

        UniHumanoid.BvhImporterContext m_context;

        public Transform Root => m_context?.Root.transform;

        public VRM10Motion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            m_src = m_context.Root.GetComponent<Animator>();
        }

        public void ShowBoxMan(bool showBoxMan)
        {
            if (m_context != null)
            {
                m_context.Root.GetComponent<SkinnedMeshRenderer>().enabled = showBoxMan;
            }
        }

        public static VRM10Motion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new VRM10Motion(context);
        }

        public static VRM10Motion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        // TODO: vrm-animation
        // https://github.com/vrm-c/vrm-animation
        public static VRM10Motion LoadVrmAnimationFromPath(string path)
        {
            throw new NotImplementedException();
        }

        public void Retarget(Animator dst)
        {
            UpdateControlRigImplicit(m_src, dst);
        }

        /// <summary>
        /// from v0.104
        /// </summary>
        public static void UpdateControlRigImplicit(Animator src, Animator dst)
        {
            // var dst = m_controller.GetComponent<Animator>();

            foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var boneTransform = dst.GetBoneTransform(bone);
                if (boneTransform == null)
                {
                    continue;
                }

                var bvhBone = src.GetBoneTransform(bone);
                if (bvhBone != null)
                {
                    // set normalized pose
                    boneTransform.localRotation = bvhBone.localRotation;
                }

                if (bone == HumanBodyBones.Hips)
                {
                    // TODO: hips position scaling ?
                    boneTransform.localPosition = bvhBone.localPosition;
                }
            }
        }

        /// <summary>
        /// from v0.108
        /// </summary>
        public static void UpdateControlRigImplicit(UniHumanoid.Humanoid src, Animator dst)
        {
            foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var boneTransform = dst.GetBoneTransform(bone);
                if (boneTransform == null)
                {
                    continue;
                }

                var bvhBone = src.GetBoneTransform(bone);
                if (bvhBone != null)
                {
                    // set normalized pose
                    boneTransform.localRotation = bvhBone.localRotation;
                    if (bone == HumanBodyBones.Hips)
                    {
                        // TODO: hips position scaling ?
                        boneTransform.localPosition = bvhBone.localPosition;
                    }
                }
            }
        }
    }
}

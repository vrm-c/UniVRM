using System;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    class Loaded : IDisposable
    {
        RuntimeGltfInstance m_instance;
        Vrm10Instance m_controller;

        VRM10AIUEO m_lipSync;
        bool m_enableLipSyncValue;
        public bool EnableLipSyncValue
        {
            set
            {
                if (m_enableLipSyncValue == value) return;
                m_enableLipSyncValue = value;
                if (m_lipSync != null)
                {
                    m_lipSync.enabled = m_enableLipSyncValue;
                }
            }
        }

        VRM10AutoExpression m_autoExpression;
        bool m_enableAutoExpressionValue;
        public bool EnableAutoExpressionValue
        {
            set
            {
                if (m_enableAutoExpressionValue == value) return;
                m_enableAutoExpressionValue = value;
                if (m_autoExpression != null)
                {
                    m_autoExpression.enabled = m_enableAutoExpressionValue;
                }
            }
        }

        VRM10Blinker m_blink;
        bool m_enableBlinkValue;
        public bool EnableBlinkValue
        {
            set
            {
                if (m_blink == value) return;
                m_enableBlinkValue = value;
                if (m_blink != null)
                {
                    m_blink.enabled = m_enableBlinkValue;
                }
            }
        }

        public Loaded(RuntimeGltfInstance instance, Transform lookAtTarget)
        {
            m_instance = instance;

            m_controller = instance.GetComponent<Vrm10Instance>();
            if (m_controller != null)
            {
                // VRM
                m_controller.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                {
                    m_lipSync = instance.gameObject.AddComponent<VRM10AIUEO>();
                    m_blink = instance.gameObject.AddComponent<VRM10Blinker>();
                    m_autoExpression = instance.gameObject.AddComponent<VRM10AutoExpression>();

                    m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze;
                    m_controller.Gaze = lookAtTarget;
                }
            }

            var animation = instance.GetComponent<Animation>();
            if (animation && animation.clip != null)
            {
                // GLTF animation
                animation.Play(animation.clip.name);
            }
        }

        public void Dispose()
        {
            // destroy GameObject
            GameObject.Destroy(m_instance.gameObject);
        }

        /// <summary>
        /// from v0.103
        /// </summary>
        /// <param name="src"></param>
        public void UpdateControlRigExplicit(Animator src)
        {
            var controlRig = m_controller.Runtime.ControlRig;

            foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var controlRigBone = controlRig.GetBoneTransform(bone);
                if (controlRigBone == null)
                {
                    continue;
                }

                var bvhBone = src.GetBoneTransform(bone);
                if (bvhBone != null)
                {
                    // set normalized pose
                    controlRigBone.localRotation = bvhBone.localRotation;
                }

                if (bone == HumanBodyBones.Hips)
                {
                    controlRigBone.localPosition = bvhBone.localPosition * controlRig.InitialHipsHeight;
                }
            }
        }

        /// <summary>
        /// from v0.104
        /// </summary>
        /// <param name="src"></param>
        public void UpdateControlRigImplicit(Animator src)
        {
            var dst = m_controller.GetComponent<Animator>();

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

        public void TPoseControlRig()
        {
            var controlRig = m_controller.Runtime.ControlRig;
            controlRig.EnforceTPose();
        }
    }
}

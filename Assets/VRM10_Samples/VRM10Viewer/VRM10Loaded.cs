using System;
using System.Collections.Generic;
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
        /// from v0.109
        /// </summary>
        /// <param name="src"></param>
        public void UpdateControlRig(IControlRigInput src)
        {
            // recursive from root
            UpdateControlRigRecursive(src, HumanBodyBones.Hips, Quaternion.identity);
        }

        static Dictionary<HumanBodyBones, HumanBodyBones[]> s_childrenMap = new Dictionary<HumanBodyBones, HumanBodyBones[]>
        {
            {HumanBodyBones.Hips, new HumanBodyBones[]{HumanBodyBones.Spine, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightLowerLeg }},
            {HumanBodyBones.Spine, new HumanBodyBones[]{HumanBodyBones.Chest}},
            {HumanBodyBones.Chest, new HumanBodyBones[]{HumanBodyBones.UpperChest}},
            {HumanBodyBones.UpperChest, new HumanBodyBones[]{HumanBodyBones.Neck, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder}},
            {HumanBodyBones.Neck, new HumanBodyBones[]{HumanBodyBones.Head}},
            {HumanBodyBones.Head, new HumanBodyBones[]{}},
            {HumanBodyBones.LeftShoulder, new HumanBodyBones[]{HumanBodyBones.LeftUpperArm}},
            {HumanBodyBones.LeftUpperArm, new HumanBodyBones[]{HumanBodyBones.LeftLowerArm}},
            {HumanBodyBones.LeftLowerArm, new HumanBodyBones[]{HumanBodyBones.LeftHand}},
            {HumanBodyBones.LeftHand, new HumanBodyBones[]{}},
            {HumanBodyBones.RightShoulder, new HumanBodyBones[]{HumanBodyBones.RightUpperArm}},
            {HumanBodyBones.RightUpperArm, new HumanBodyBones[]{HumanBodyBones.RightLowerArm}},
            {HumanBodyBones.RightLowerArm, new HumanBodyBones[]{HumanBodyBones.RightHand}},
            {HumanBodyBones.RightHand, new HumanBodyBones[]{}},
            {HumanBodyBones.LeftUpperLeg, new HumanBodyBones[]{HumanBodyBones.LeftLowerLeg}},
            {HumanBodyBones.LeftLowerLeg, new HumanBodyBones[]{HumanBodyBones.LeftFoot}},
            {HumanBodyBones.LeftFoot, new HumanBodyBones[]{}},
            {HumanBodyBones.RightUpperLeg, new HumanBodyBones[]{HumanBodyBones.RightLowerLeg}},
            {HumanBodyBones.RightLowerLeg, new HumanBodyBones[]{HumanBodyBones.RightFoot}},
            {HumanBodyBones.RightFoot, new HumanBodyBones[]{}},
        };

        void UpdateControlRigRecursive(IControlRigInput src, HumanBodyBones bone, Quaternion parent)
        {
            var controlRig = m_controller.Runtime.ControlRig;

            var controlRigBone = controlRig.GetBoneTransform(bone);

            var local = Quaternion.identity;
            if (controlRigBone != null)
            {
                if (src.TryGetBoneLocalRotation(bone, parent, out local))
                {
                    // set normalized pose
                    controlRigBone.localRotation = local;
                }
                else
                {
                    local = Quaternion.identity;
                }

                if (bone == HumanBodyBones.Hips)
                {
                    controlRigBone.localPosition = src.RootPosition * controlRig.InitialHipsHeight;
                }
            }

            // children
            var current = parent * local;
            foreach (var child in s_childrenMap[bone])
            {
                UpdateControlRigRecursive(src, child, current);
            }
        }

        public void TPoseControlRig()
        {
            var controlRig = m_controller.Runtime.ControlRig;
            controlRig.EnforceTPose();
        }
    }
}

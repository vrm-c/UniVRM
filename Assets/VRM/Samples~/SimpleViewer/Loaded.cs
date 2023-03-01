using System;
using UniGLTF;
using UniHumanoid;
using UnityEngine;

namespace VRM.SimpleViewer
{
    class Loaded : IDisposable
    {
        RuntimeGltfInstance _instance;
        VRMBlendShapeProxy m_proxy;

        Blinker m_blink;
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

        AIUEO m_lipSync;
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

        public Loaded(RuntimeGltfInstance instance, HumanPoseTransfer src, Transform lookAtTarget)
        {
            _instance = instance;

            var lookAt = instance.GetComponent<VRMLookAtHead>();
            if (lookAt != null)
            {
                // vrm
                _pose = _instance.gameObject.AddComponent<HumanPoseTransfer>();
                _pose.Source = src;
                _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

                m_lipSync = instance.gameObject.AddComponent<AIUEO>();
                m_blink = instance.gameObject.AddComponent<Blinker>();

                lookAt.Target = lookAtTarget;
                lookAt.UpdateType = UpdateType.LateUpdate; // after HumanPoseTransfer's setPose

                m_proxy = instance.GetComponent<VRMBlendShapeProxy>();
            }

            // not vrm
            var animation = instance.GetComponent<Animation>();
            if (animation && animation.clip != null)
            {
                animation.Play(animation.clip.name);
            }
        }

        public void Dispose()
        {
            // Destroy game object. not RuntimeGltfInstance
            GameObject.Destroy(_instance.gameObject);
        }

        public void Update()
        {
            if (m_proxy != null)
            {
                m_proxy.Apply();
            }
        }

        HumanPoseTransfer _pose;


        public void EnableBvh(HumanPoseTransfer src)
        {
            if (_pose != null)
            {
                _pose.Source = src;
                _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
            }
        }

        public void EnableTPose(HumanPoseClip pose)
        {
            if (_pose != null)
            {
                _pose.PoseClip = pose;
                _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseClip;
            }
        }

        public void ResetSpring()
        {
            if (_pose != null)
            {
                foreach (var spring in _pose.GetComponentsInChildren<VRMSpringBone>())
                {
                    spring.Setup();
                }
            }
        }
    }
}

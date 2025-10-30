using System;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.Cloth.Viewer
{
    class Loaded : IDisposable
    {
        RuntimeGltfInstance m_instance;
        public RuntimeGltfInstance Instance => m_instance;
        Vrm10Instance m_controller;
        public Vrm10RuntimeControlRig ControlRig => m_controller.Runtime.ControlRig;
        public Vrm10Runtime Runtime => m_controller.Runtime;

        ClothAIUEO m_lipSync;
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

        ClothAutoExpression m_autoExpression;
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

        ClothBlinker m_blink;
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
                    m_lipSync = instance.gameObject.AddComponent<ClothAIUEO>();
                    m_blink = instance.gameObject.AddComponent<ClothBlinker>();
                    m_autoExpression = instance.gameObject.AddComponent<ClothAutoExpression>();

                    m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform;
                    m_controller.LookAtTarget = lookAtTarget;
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
    }
}

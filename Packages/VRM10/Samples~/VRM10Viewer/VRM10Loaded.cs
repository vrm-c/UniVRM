using System;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    class Loaded : IDisposable
    {
        RuntimeGltfInstance m_instance;
        Vrm10Instance m_vrm;
        public Vrm10Instance Instance => m_vrm;
        public Vrm10RuntimeControlRig ControlRig => m_vrm.Runtime.ControlRig;
        public Vrm10Runtime Runtime => m_vrm?.Runtime;

        public Loaded(RuntimeGltfInstance instance)
        {
            m_instance = instance;

            m_vrm = instance.GetComponent<Vrm10Instance>();
            if (m_vrm != null)
            {
                m_vrm.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                m_vrm.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.YawPitchValue;
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
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// デフォルトの VRMSPringBone 実装です。
    /// 
    ///   VRMSpringBone.m_updateType = LateUpdate
    /// 
    /// により、各VRMSpringBoneが自力で LateUpdate に動作します。
    /// </summary>
    public class Vrm0XSpringBoneDefaultRuntime : IVrm0XSpringBoneRuntime
    {
        GameObject m_vrm;
        public async Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller)
        {
            m_vrm = vrm;

            foreach (VRMSpringBone sb in vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.LateUpdate;
            }
            await awaitCaller.NextFrame();
        }

        public void ReconstructSpringBone()
        {
            foreach (VRMSpringBone sb in m_vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.Setup();
            }
        }

        public void RestoreInitialTransform()
        {
            foreach (VRMSpringBone sb in m_vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.ReinitializeRotation();
            }
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            // no impl
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
            foreach (VRMSpringBone sb in m_vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.SetModelLevel(modelSettings);
                sb.m_updateType = modelSettings.StopSpringBoneWriteback ? VRMSpringBone.SpringBoneUpdateType.Manual : VRMSpringBone.SpringBoneUpdateType.LateUpdate;
            }
        }
    }
}
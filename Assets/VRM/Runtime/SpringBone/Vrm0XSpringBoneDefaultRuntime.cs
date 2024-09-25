using System.Threading.Tasks;
using UniGLTF;
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
        VRMSpringBone[] m_springs;

        public async Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller)
        {
            m_vrm = vrm;
            m_springs = vrm.GetComponentsInChildren<VRMSpringBone>();

#if VRM0X_SPRING_UPDATE_SELF
            foreach (VRMSpringBone sb in m_springs)
            {
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.LateUpdate;
            }
#endif

            await awaitCaller.NextFrame();
        }

        public void Reset()
        {
            foreach (var spring in m_vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                spring.Setup();
            }
        }

        public void Process(float deltaTime)
        {
#if VRM0X_SPRING_UPDATE_SELF
            // 各 VrmSpringBone が自力で Update するので何もしない
#else
            foreach (VRMSpringBone sb in m_springs)
            {
                sb.ManualUpdate(deltaTime);
            }
#endif
        }
    }
}
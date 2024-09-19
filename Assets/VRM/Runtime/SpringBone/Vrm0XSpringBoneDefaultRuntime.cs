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
        public async Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller)
        {
            foreach (VRMSpringBone sb in vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.LateUpdate;
            }
            await awaitCaller.NextFrame();
        }
    }
}
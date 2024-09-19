using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// FastSpringbone(job + singleton) で動作します。
    /// 
    ///   VRMSpringBone.m_updateType = Manual
    /// 
    /// により、各VRMSpringBoneの自力Updateは停止します。
    /// FastSpringBoneService に登録します。
    /// FastSpringBoneService.LateUpdate[DefaultExecutionOrder(11000)] で動作します。
    /// </summary>
    public class Vrm0XFastSpringboneRuntime : IVrm0XSpringBoneRuntime
    {
        public async Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller)
        {
            // default update の停止
            foreach (VRMSpringBone sb in vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.Manual;
            }

            // create
            var buffer = await SpringBoneJobs.FastSpringBoneReplacer.MakeBufferAsync(vrm, awaitCaller);
            SpringBoneJobs.FastSpringBoneService.Instance.BufferCombiner.Register(buffer);

            // disposer
            var disposer = vrm.AddComponent<FastSpringBoneDisposer>()
                .AddAction(() =>
                {
                    SpringBoneJobs.FastSpringBoneService.Instance.BufferCombiner.Unregister(buffer);
                    buffer.Dispose();
                })
                ;
        }
    }
}
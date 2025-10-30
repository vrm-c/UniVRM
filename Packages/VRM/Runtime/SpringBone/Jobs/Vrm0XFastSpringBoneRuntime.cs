using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF.SpringBoneJobs.InputPorts;
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
        GameObject m_vrm;
        SpringBoneJobs.FastSpringBoneService m_service;
        FastSpringBoneBuffer m_buffer;

        public Vrm0XFastSpringboneRuntime()
        {
            m_service = SpringBoneJobs.FastSpringBoneService.Instance;
        }

        public async Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller)
        {
            m_vrm = vrm;

            // default update の停止
            foreach (VRMSpringBone sb in vrm.GetComponentsInChildren<VRMSpringBone>())
            {
                sb.m_updateType = VRMSpringBone.SpringBoneUpdateType.Manual;
            }

            // disposer
            var disposer = m_vrm.AddComponent<FastSpringBoneDisposer>()
                .AddAction(() =>
                {
                    Unregister();
                })
                ;

            // create
            await RegisterAsync(awaitCaller);
        }

        void Unregister()
        {
            UniGLTFLogger.Log("Vrm0XFastSpringboneRuntime.Unregister");
            if (m_buffer == null)
            {
                return;
            }

            m_service.BufferCombiner.Register(add: null, remove: m_buffer);
            m_buffer.Dispose();
            m_buffer = null;
        }

        async Task RegisterAsync(IAwaitCaller awaitCaller)
        {
            Debug.Assert(m_buffer == null);
            var buffer = await SpringBoneJobs.FastSpringBoneReplacer.MakeBufferAsync(m_vrm, awaitCaller);
            m_buffer = buffer;
            SpringBoneJobs.FastSpringBoneService.Instance.BufferCombiner.Register(add: buffer, remove: null);

        }

        public void ReconstructSpringBone()
        {
            Unregister();
            var _ = RegisterAsync(new ImmediateCaller());
        }

        public void RestoreInitialTransform()
        {
            if (m_buffer != null)
            {
                m_service.BufferCombiner.InitializeJointsLocalRotation(m_buffer);
            }
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            m_service.BufferCombiner.Combined.SetJointLevel(joint, jointSettings);
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
            m_service.BufferCombiner.Combined?.SetModelLevel(modelRoot, modelSettings);
        }
    }
}
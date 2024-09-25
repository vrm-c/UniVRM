using UniGLTF;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Vrm-0.x モデルの root にアタッチする。
    /// 
    /// IVrm0XSpringBoneRuntime を保持し、update を管理する。
    /// </summary>
    [DefaultExecutionOrder(11000)]
    public class Vrm0XSpringBoneRuntimeController : MonoBehaviour
    {
        IVrm0XSpringBoneRuntime m_springboneRuntime;
        public void SetSpringRuntime(IVrm0XSpringBoneRuntime runtime)
        {
            m_springboneRuntime = runtime;
        }
        public IVrm0XSpringBoneRuntime Runtime
        {
            get
            {
                if (m_springboneRuntime == null)
                {
                    // Scene に 配置された vrm がここに来る。
                    var provider = GetComponent<IVrm0XSpringBoneRuntimeProvider>();
                    if (provider != null)
                    {
                        // IVrm0XSpringBoneRuntimeProvider で SpringBoneRuntime をカスタマイズできる v0.127.0
                        m_springboneRuntime = provider.CreateSpringBoneRuntime();
                    }
                    else
                    {
                        // fallback
                        m_springboneRuntime = new Vrm0XSpringBoneDefaultRuntime();
                    }

                    // initialize immediate
                    // runtime は importer 内で初期化
                    m_springboneRuntime.InitializeAsync(gameObject, new ImmediateCaller());
                }
                return m_springboneRuntime;
            }
        }

        public enum SpringBoneUpdateType
        {
            LateUpdate,
            FixedUpdate,
            Manual,
        }
        public SpringBoneUpdateType UpdateType = SpringBoneUpdateType.LateUpdate;

        void LateUpdate()
        {
            if (UpdateType == SpringBoneUpdateType.LateUpdate)
            {
                Runtime.Process(Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            if (UpdateType == SpringBoneUpdateType.FixedUpdate)
            {
                Runtime.Process(Time.fixedDeltaTime);
            }
        }

        public void ManualUpdate(float deltaTime)
        {
            if (UpdateType == SpringBoneUpdateType.Manual)
            {
                Runtime.Process(deltaTime);
            }
            else
            {
                throw new System.ArgumentException("require SpringBoneUpdateType.Manual");
            }
        }
    }
}
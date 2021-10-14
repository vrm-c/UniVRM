using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRM.AnimationBridgeSample
{
    /// <summary>
    /// VRMからAnimatorを取り外してからアタッチしてください。
    ///  VRM.Samples/Scenes/AnimationBridgeSample でテストできます。
    /// 
    /// Animatorがアタッチされていると、Animatorに負けて？動作しません。
    /// </summary>
    [RequireComponent(typeof(Animation))]
    public class AnimationBridge : MonoBehaviour
    {
        VRMBlendShapeProxy m_proxy;

        void OnEnable()
        {
            m_proxy = GetComponent<VRMBlendShapeProxy>();
            if (!m_proxy)
            {
                this.enabled = false;
            }
        }

        public float Lip_A;
        public float Lip_I;
        public float Lip_U;
        public float Lip_E;
        public float Lip_O;
        public float Blink;
        public float Expression_Joy;
        public float Expression_Angry;
        public float Expression_Sorrow;
        public float Expression_Fun;

        void Update()
        {
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), Lip_A);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.I), Lip_I);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.U), Lip_U);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.E), Lip_E);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), Lip_O);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), Blink);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), Expression_Joy);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), Expression_Angry);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), Expression_Sorrow);
            m_proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), Expression_Fun);
        }

        void LateUpdate()
        {
            m_proxy.Apply();
        }
    }
}
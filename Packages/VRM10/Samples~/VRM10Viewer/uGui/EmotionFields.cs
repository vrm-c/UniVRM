using System;
using UnityEngine.UI;

namespace UniVRM10.VRM10Viewer
{
    [Serializable]
    class EmotionFields
    {
        public Slider m_expression;
        public Toggle m_binary;
        public bool m_useOverride;
        public Dropdown m_overrideMouth;
        public Dropdown m_overrideBlink;
        public Dropdown m_overrideLookAt;

        public void Reset(ObjectMap map, string name, bool useOveride)
        {
            m_expression = map.Get<Slider>($"Slider{name}");
            m_binary = map.Get<Toggle>($"Binary{name}");
            m_useOverride = useOveride;
            if (useOveride)
            {
                m_overrideMouth = map.Get<Dropdown>($"Override{name}Mouth");
                m_overrideBlink = map.Get<Dropdown>($"Override{name}Blink");
                m_overrideLookAt = map.Get<Dropdown>($"Override{name}LookAt");
            }
        }

        static int GetOverrideIndex(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType value)
        {
            switch (value)
            {
                case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none: return 0;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block: return 1;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend: return 2;
                default: return -1;
            }
        }

        static UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType ToOverrideType(int index)
        {
            switch (index)
            {
                case 0: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none;
                case 1: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block;
                case 2: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend;
                default: throw new ArgumentException();
            }
        }

        public void OnLoad(VRM10Expression expression)
        {
            m_binary.isOn = expression.IsBinary;
            if (m_useOverride)
            {
                m_overrideMouth.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideMouth));
                m_overrideBlink.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideBlink));
                m_overrideLookAt.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideLookAt));
            }
        }

        public void ApplyRuntime(VRM10Expression expression)
        {
            expression.IsBinary = m_binary.isOn;
            if (m_useOverride)
            {
                expression.OverrideMouth = ToOverrideType(m_overrideMouth.value);
                expression.OverrideBlink = ToOverrideType(m_overrideBlink.value);
                expression.OverrideLookAt = ToOverrideType(m_overrideLookAt.value);
            }
        }
    }
}
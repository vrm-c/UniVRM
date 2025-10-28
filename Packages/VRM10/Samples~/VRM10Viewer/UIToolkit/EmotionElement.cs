using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UniVRM10;

public class EmotionElement : VisualElement
{
    Slider _slider;
    Toggle m_binary;
    DropdownField _overrideMouth;
    DropdownField _overrideBlink;
    DropdownField _overrideLookat;
    public bool m_useOverride;

    public EmotionElement()
    {
    }

    public EmotionElement Init()
    {
        _slider = this.Q<Slider>("Slider");
        Debug.Assert(_slider != null);
        m_binary = this.Q<Toggle>("Toggle");
        Debug.Assert(m_binary != null);
        _overrideMouth = this.Q<DropdownField>("OverrideMouth");
        Debug.Assert(_overrideMouth != null);
        _overrideBlink = this.Q<DropdownField>("OverrideBlink");
        Debug.Assert(_overrideBlink != null);
        _overrideLookat = this.Q<DropdownField>("OverrideLookat");
        Debug.Assert(_overrideLookat != null);

        return this;
    }

    public void SetValueWithoutNotify(float value)
    {
        _slider.value = value;
    }

    public float Value => _slider.value;

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

    public void ApplyRuntime(VRM10Expression expression)
    {
        expression.IsBinary = m_binary.value;
        if (m_useOverride)
        {
            expression.OverrideMouth = ToOverrideType(_overrideMouth.index);
            expression.OverrideBlink = ToOverrideType(_overrideBlink.index);
            expression.OverrideLookAt = ToOverrideType(_overrideLookat.index);
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

    public void OnLoad(VRM10Expression expression)
    {
        m_binary.value = expression.IsBinary;
        if (m_useOverride)
        {
            _overrideMouth.index = GetOverrideIndex(expression.OverrideMouth);
            _overrideBlink.index = GetOverrideIndex(expression.OverrideBlink);
            _overrideLookat.index = GetOverrideIndex(expression.OverrideLookAt);
        }
    }

    public new class UxmlFactory : UxmlFactory<EmotionElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }
}

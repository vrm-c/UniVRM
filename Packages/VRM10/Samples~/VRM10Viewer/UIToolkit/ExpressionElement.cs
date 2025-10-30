using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UniVRM10;

public class ExpressionElement : VisualElement
{
    Slider _slider;
    Toggle _toggle;

    public ExpressionElement() { }

    public ExpressionElement Init()
    {
        _slider = this.Q<Slider>("Slider");
        Debug.Assert(_slider != null);
        _toggle = this.Q<Toggle>("Toggle");
        Debug.Assert(_toggle != null);

        return this;
    }

    public void SetValueWithoutNotify(float value)
    {
        _slider.value = value;
    }

    public float Value => _slider.value;

    public void ApplyRuntime(VRM10Expression expression)
    {
        expression.IsBinary = _toggle.value;
    }

    public void OnLoad(VRM10Expression expression)
    {
        _toggle.value = expression.IsBinary;
    }

    public new class UxmlFactory : UxmlFactory<ExpressionElement, UxmlTraits> { }

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

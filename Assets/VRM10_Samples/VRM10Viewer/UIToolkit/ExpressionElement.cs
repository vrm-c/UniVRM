using System.Collections.Generic;
using UnityEngine.UIElements;

public class ExpressionElement : VisualElement
{
    public ExpressionElement() { }

    public new class UxmlFactory : UxmlFactory<ExpressionElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_label = new UxmlStringAttributeDescription { name = "label" };

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

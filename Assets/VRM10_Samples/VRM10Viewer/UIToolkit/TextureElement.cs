using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class TextureElement : VisualElement
{
    public TextureElement()
    {
    }

    public new class UxmlFactory : UxmlFactory<TextureElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_texture = new UxmlStringAttributeDescription { name = "texture" };

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
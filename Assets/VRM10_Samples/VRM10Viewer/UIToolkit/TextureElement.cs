using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_6000_6_OR_NEWER
[UxmlElement]
partial class TextureElement : VisualElement
#else
class TextureElement : VisualElement
#endif
{
    public TextureElement()
    {
    }

#if !UNITY_6000_6_OR_NEWER
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
#endif
}
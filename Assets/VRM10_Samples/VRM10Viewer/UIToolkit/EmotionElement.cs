using System.Collections.Generic;
using UnityEngine.UIElements;

    public class EmotionElement : VisualElement
    {
        public EmotionElement() { }

        public new class UxmlFactory : UxmlFactory<EmotionElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
            }
        }
    }

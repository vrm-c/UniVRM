using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRM10.Samples.MToon10Showcase
{
    public sealed class GraphicScrollHandler : Graphic , IScrollHandler
    {
        public Action<PointerEventData> OnScrollAction { get; set; }

        public void OnScroll(PointerEventData eventData)
        {
            OnScrollAction?.Invoke(eventData);
        }
    }
}
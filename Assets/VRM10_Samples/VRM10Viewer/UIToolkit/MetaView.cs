using System.Collections.Generic;
using UnityEngine.UIElements;

#if UNITY_6000_6_OR_NEWER
[UxmlElement]
public partial class MetaView : VisualElement
#else
public class MetaView : VisualElement
#endif
{
    public void Init()
    {
    }

#if !UNITY_6000_6_OR_NEWER
    public new class UxmlFactory : UxmlFactory<MetaView> { }
#endif
}

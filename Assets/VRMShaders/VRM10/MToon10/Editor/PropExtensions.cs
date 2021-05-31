using System.Collections.Generic;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public static class PropExtensions
    {
        private static readonly Dictionary<Prop, string> _propertyNames = new Dictionary<Prop, string>
        {
            [Prop.AlphaMode] = "_AlphaMode",
            [Prop.TransparentWithZWrite] = "_TransparentWithZWrite",
            [Prop.AlphaCutoff] = "_Cutoff",
            [Prop.RenderQueueOffsetNumber] = "_RenderQueueOffset",
            [Prop.DoubleSided] = "_DoubleSided",
            [Prop.BaseColorFactor] = "_Color",
            [Prop.UnityCullMode] = "_M_CullMode",
            [Prop.UnitySrcBlend] = "_M_SrcBlend",
            [Prop.UnityDstBlend] = "_M_DstBlend",
            [Prop.UnityZWrite] = "_M_ZWrite",
            [Prop.UnityAlphaToMask] = "_M_AlphaToMask",
        };

        public static IReadOnlyDictionary<Prop, string> PropertyNames => _propertyNames;

        public static string ToName(this Prop prop)
        {
            return PropertyNames[prop];
        }
    }
}
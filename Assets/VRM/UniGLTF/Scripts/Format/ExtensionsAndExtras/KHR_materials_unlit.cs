using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_materials_unlit
    {
        public const string ExtensionName = "KHR_materials_unlit";

        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        public static glTFMaterial CreateDefault()
        {
            return new glTFMaterial
            {
                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f },
                    roughnessFactor = 0.9f,
                    metallicFactor = 0.0f,
                },
                extensions = glTFExtension.Create(ExtensionName, "{}")
            };
        }

        public static bool IsEnable(glTFMaterial m)
        {
            if (m.extensions == null)
            {
                return false;
            }

            foreach (var kv in m.extensions.ObjectItems())
            {
                if (kv.Key.GetUtf8String() == ExtensionNameUtf8)
                {
                    return kv.Value.Value.ValueType == ValueNodeType.Object;
                }
            }

            return false;
        }

        public static glTFExtension Serialize()
        {
            return glTFExtension.Create(ExtensionName, "{}");
        }
    }
}

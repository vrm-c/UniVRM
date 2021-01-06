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

        public static readonly byte[] Raw = new byte[] { (byte)'{', (byte)'}' };

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
                extensions = new glTFExtensionExport().Add(ExtensionName, new ArraySegment<byte>(Raw))
            };
        }

        public static bool IsEnable(glTFMaterial m)
        {
            if (m.extensions is glTFExtensionImport imported)
            {
                foreach (var kv in imported.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtensionNameUtf8)
                    {
                        return kv.Value.Value.ValueType == ValueNodeType.Object;
                    }
                }
            }

            return false;
        }

        public static glTFExtension Serialize()
        {
            return new glTFExtensionExport().Add(ExtensionName, new ArraySegment<byte>(Raw));
        }
    }
}

using System;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_emissive_strength
    /// </summary>
    [Serializable]
    public class glTF_KHR_materials_emissive_strength
    {
        public const string ExtensionName = "KHR_materials_emissive_strength";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);
        public float emissiveStrength = 1.0f;

        static glTF_KHR_materials_emissive_strength Deserialize(JsonNode json)
        {
            var extension = new glTF_KHR_materials_emissive_strength();
            if (json.TryGet(nameof(emissiveStrength), out JsonNode found))
            {
                extension.emissiveStrength = found.GetSingle();
            }
            return extension;
        }

        public static bool TryGet(glTFExtension src, out glTF_KHR_materials_emissive_strength extension)
        {
            if (src is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtensionNameUtf8)
                    {
                        extension = Deserialize(kv.Value);
                        return true;
                    }
                }
            }
            extension = default;
            return false;
        }

        public static void Serialize(ref glTFExtension materialExtensions, float value)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key(nameof(emissiveStrength));
            f.Value(value);
            f.EndMap();
            glTFExtensionExport.GetOrCreate(ref materialExtensions).Add(ExtensionName, f.GetStore().Bytes);
        }
    }
}

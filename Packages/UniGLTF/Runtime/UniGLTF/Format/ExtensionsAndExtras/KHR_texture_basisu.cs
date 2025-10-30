using System;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// `texture` に対する extension.
    ///
    /// https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_texture_basisu
    /// </summary>
    [Serializable]
    public sealed class glTF_KHR_texture_basisu
    {
        public const string ExtensionName = "KHR_texture_basisu";

        private static readonly Utf8String ExtensionNameUt8 = Utf8String.From(ExtensionName);

        /// <summary>
        /// glTF Id
        /// </summary>
        [JsonSchema(Minimum = 0)]
        public int source = -1;

        public static bool TryGet(glTFTexture texture, out glTF_KHR_texture_basisu basisuExtension)
        {
            if (texture?.extensions is glTFExtensionImport importedExtensions)
            {
                foreach (var kv in importedExtensions.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtensionNameUt8)
                    {
                        basisuExtension = Deserialize(kv.Value);
                        return basisuExtension != null;
                    }
                }
            }

            basisuExtension = default;
            return false;
        }

        private static glTF_KHR_texture_basisu Deserialize(JsonNode json)
        {
            if (json.Value.ValueType != ValueNodeType.Object) return null;

            foreach (var kv in json.ObjectItems())
            {
                var key = kv.Key.GetString();
                switch (key)
                {
                    case nameof(source):
                        var value = kv.Value.GetInt32();
                        return new glTF_KHR_texture_basisu
                        {
                            source = value,
                        };
                }
            }

            return null;
        }
    }
}
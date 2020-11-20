using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_texture_transform
    {
        public const string ExtensionName = "KHR_texture_transform";

        public static readonly Utf8String ExtensionNameUt8 = Utf8String.From(ExtensionName);


        [JsonSchema(MinItems = 2, MaxItems = 2)]
        public float[] offset = new float[2] { 0.0f, 0.0f };

        public float rotation;

        [JsonSchema(MinItems = 2, MaxItems = 2)]
        public float[] scale = new float[2] { 1.0f, 1.0f };

        [ItemJsonSchema(Minimum = 0)]
        public int texCoord;

        static glTF_KHR_texture_transform Deserialize(ListTreeNode<JsonValue> json)
        {
            var t = new glTF_KHR_texture_transform();

            if (json.Value.ValueType == ValueNodeType.Object)
            {
                foreach (var kv in json.ObjectItems())
                {
                    var key = kv.Key.GetString();
                    switch (key)
                    {
                        case nameof(offset):
                            throw new NotImplementedException();
                            break;
                        case nameof(rotation):
                            throw new NotImplementedException();
                            break;
                        case nameof(scale):
                            throw new NotImplementedException();
                            break;
                        case nameof(texCoord):
                            throw new NotImplementedException();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

            }

            return t;
        }

        public static bool TryGet(glTFTextureInfo info, out glTF_KHR_texture_transform t)
        {
            if (info.extensions is ListTreeNode<JsonValue> json)
            {
                if (json.Value.ValueType == ValueNodeType.Object)
                {
                    foreach (var kv in json.ObjectItems())
                    {
                        if (kv.Key.GetUtf8String() == ExtensionNameUt8)
                        {
                            t = Deserialize(kv.Value);
                            return true;
                        }
                    }
                }
            }

            t = default;
            return false;
        }
    }
}

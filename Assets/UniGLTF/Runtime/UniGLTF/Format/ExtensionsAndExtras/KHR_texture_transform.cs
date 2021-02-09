using System;
using System.Collections.Generic;
using System.Linq;
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

        static IEnumerable<float> DeserializeFloat2(JsonNode json)
        {
            if (json.Value.ValueType == ValueNodeType.Array)
            {
                foreach (var a in json.ArrayItems())
                {
                    yield return a.GetSingle();
                }
            }
        }

        static glTF_KHR_texture_transform Deserialize(JsonNode json)
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
                            t.offset = DeserializeFloat2(kv.Value).ToArray();
                            break;

                        case nameof(rotation):
                            t.rotation = kv.Value.GetSingle();
                            break;

                        case nameof(scale):
                            t.scale = DeserializeFloat2(kv.Value).ToArray();
                            break;

                        case nameof(texCoord):
                            t.texCoord = kv.Value.GetInt32();
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
            if (info != null && info.extensions is glTFExtensionImport imported)
            {
                foreach (var kv in imported.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtensionNameUt8)
                    {
                        t = Deserialize(kv.Value);
                        return true;
                    }
                }
            }

            t = default;
            return false;
        }

        public static void Serialize(glTFTextureInfo info, (float, float) offset, (float, float) scale)
        {
            var f = new JsonFormatter();
            f.BeginMap();

            f.Key("offset");
            f.BeginList();
            f.Value(offset.Item1);
            f.Value(offset.Item2);
            f.EndList();

            f.Key("scale");
            f.BeginList();
            f.Value(scale.Item1);
            f.Value(scale.Item2);
            f.EndList();

            f.EndMap();

            glTFExtensionExport.GetOrCreate(ref info.extensions).Add(ExtensionName, f.GetStore().Bytes);
        }
    }
}

using System;
using System.IO;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFTextureSampler : JsonSerializableBase
    {
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt,
            EnumExcludes = new object[] {
                glFilter.NONE,
                glFilter.NEAREST_MIPMAP_NEAREST,
                glFilter.LINEAR_MIPMAP_NEAREST,
                glFilter.NEAREST_MIPMAP_LINEAR,
                glFilter.LINEAR_MIPMAP_LINEAR,
            })]
        public glFilter magFilter = glFilter.NEAREST;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt,
            EnumExcludes = new object[] { glFilter.NONE })]
        public glFilter minFilter = glFilter.NEAREST;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt,
            EnumExcludes = new object[] { glWrap.NONE })]
        public glWrap wrapS = glWrap.REPEAT;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt,
            EnumExcludes = new object[] { glWrap.NONE })]
        public glWrap wrapT = glWrap.REPEAT;

        // empty schemas
        public object extensions;
        public object extras;
        public string name;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.Key("magFilter"); f.Value((int)magFilter);
            f.Key("minFilter"); f.Value((int)minFilter);
            f.Key("wrapS"); f.Value((int)wrapS);
            f.Key("wrapT"); f.Value((int)wrapT);
        }
    }

    [Serializable]
    public class glTFImage : JsonSerializableBase
    {
        public string name;
        public string uri;

        [JsonSchema(Dependencies = new string[] { "mimeType" }, Minimum = 0)]
        public int bufferView;

        [JsonSchema(EnumValues = new object[] { "image/jpeg", "image/png" }, EnumSerializationType =EnumSerializationType.AsString)]
        public string mimeType;

        public string GetExt()
        {
            switch (mimeType)
            {
                case "image/png":
                    return ".png";

                case "image/jpeg":
                    return ".jpg";

                default:
                    if (uri.StartsWith("data:image/jpeg;"))
                    {
                        return ".jpg";
                    }
                    else if (uri.StartsWith("data:image/png;"))
                    {
                        return ".png";
                    }
                    else
                    {
                        return Path.GetExtension(uri).ToLower();
                    }
            }
        }

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => name);
            if (!string.IsNullOrEmpty(uri))
            {
                f.KeyValue(() => uri);
            }
            else
            {
                f.KeyValue(() => bufferView);
                f.KeyValue(() => mimeType);
            }
        }
    }

    [Serializable]
    public class glTFTexture : JsonSerializableBase
    {
        [JsonSchema(Minimum = 0)]
        public int sampler;

        [JsonSchema(Minimum = 0)]
        public int source;

        // empty schemas
        public object extensions;
        public object extras;
        public string name;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => sampler);
            f.KeyValue(() => source);
        }
    }
}

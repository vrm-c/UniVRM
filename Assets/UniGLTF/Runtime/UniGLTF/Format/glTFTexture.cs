using System;
using System.IO;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFTextureSampler
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
        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }

    [Serializable]
    public class glTFImage
    {
        public string name;
        public string uri;

        [JsonSchema(Dependencies = new string[] { "mimeType" }, Minimum = 0)]
        public int bufferView;

        [JsonSchema(EnumValues = new object[] { "image/jpeg", "image/png" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string mimeType;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFTexture
    {
        [JsonSchema(Minimum = 0)]
        public int sampler;

        [JsonSchema(Minimum = 0)]
        public int source;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }
}

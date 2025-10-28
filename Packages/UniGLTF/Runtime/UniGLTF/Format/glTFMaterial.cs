using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public abstract class glTFTextureInfo
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int index = -1;

        [JsonSchema(Minimum = 0)]
        public int texCoord;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFMaterialBaseColorTextureInfo : glTFTextureInfo
    {
    }

    [Serializable]
    public class glTFMaterialMetallicRoughnessTextureInfo : glTFTextureInfo
    {
    }

    [Serializable]
    public class glTFMaterialNormalTextureInfo : glTFTextureInfo
    {
        public float scale = 1.0f;
    }

    [Serializable]
    public class glTFMaterialOcclusionTextureInfo : glTFTextureInfo
    {
        [JsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float strength = 1.0f;
    }

    [Serializable]
    public class glTFMaterialEmissiveTextureInfo : glTFTextureInfo
    {
    }

    [Serializable]
    public class glTFPbrMetallicRoughness
    {
        public glTFMaterialBaseColorTextureInfo baseColorTexture = null;

        [JsonSchema(MinItems = 4, MaxItems = 4)]
        [ItemJsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float[] baseColorFactor;

        public glTFMaterialMetallicRoughnessTextureInfo metallicRoughnessTexture = null;

        [JsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float metallicFactor = 1.0f;

        [JsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float roughnessFactor = 1.0f;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFMaterial
    {
        public string name;
        public glTFPbrMetallicRoughness pbrMetallicRoughness = new glTFPbrMetallicRoughness
        {
            baseColorFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f },
        };
        public glTFMaterialNormalTextureInfo normalTexture = null;

        public glTFMaterialOcclusionTextureInfo occlusionTexture = null;

        public glTFMaterialEmissiveTextureInfo emissiveTexture = null;

        [JsonSchema(MinItems = 3, MaxItems = 3)]
        [ItemJsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float[] emissiveFactor;

        [JsonSchema(EnumValues = new object[] { "OPAQUE", "MASK", "BLEND" }, EnumSerializationType = EnumSerializationType.AsUpperString)]
        public string alphaMode;

        [JsonSchema(Dependencies = new string[] { "alphaMode" }, Minimum = 0.0, SerializationConditions = new[] { "value.alphaMode==\"MASK\"" })]
        public float alphaCutoff = 0.5f;

        public bool doubleSided;

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFExtension extensions;
        public glTFExtension extras;

        public glTFTextureInfo[] GetTextures()
        {
            return new glTFTextureInfo[]
            {
                (pbrMetallicRoughness != null)?pbrMetallicRoughness.baseColorTexture:null,
                (pbrMetallicRoughness != null)?pbrMetallicRoughness.metallicRoughnessTexture:null,
                normalTexture,
                occlusionTexture,
                emissiveTexture
            };
        }
    }
}

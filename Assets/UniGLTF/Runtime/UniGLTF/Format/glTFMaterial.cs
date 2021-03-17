using System;
using UniJSON;

namespace UniGLTF
{
    public enum glTFTextureTypes
    {
        OcclusionMetallicRoughness,
        Normal,
        SRGB,
        Linear,
    }

    public interface IglTFTextureinfo
    {
        glTFTextureTypes TextureType { get; }
    }

    [Serializable]
    public abstract class glTFTextureInfo : IglTFTextureinfo
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int index = -1;

        [JsonSchema(Minimum = 0)]
        public int texCoord;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;

        public abstract glTFTextureTypes TextureType { get; }
    }


    [Serializable]
    public class glTFMaterialBaseColorTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType => glTFTextureTypes.SRGB;
    }

    [Serializable]
    public class glTFMaterialMetallicRoughnessTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType => glTFTextureTypes.OcclusionMetallicRoughness;
    }

    [Serializable]
    public class glTFMaterialNormalTextureInfo : glTFTextureInfo
    {
        public float scale = 1.0f;

        public override glTFTextureTypes TextureType
        {
            get { return glTFTextureTypes.Normal; }
        }
    }

    [Serializable]
    public class glTFMaterialOcclusionTextureInfo : glTFTextureInfo
    {
        [JsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float strength = 1.0f;

        public override glTFTextureTypes TextureType => glTFTextureTypes.OcclusionMetallicRoughness;
    }

    [Serializable]
    public class glTFMaterialEmissiveTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType => glTFTextureTypes.SRGB;
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

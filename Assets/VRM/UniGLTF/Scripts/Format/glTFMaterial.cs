using System;
using UniJSON;

namespace UniGLTF
{
    public enum glTFTextureTypes
    {
        BaseColor,
        Metallic,
        Normal,
        Occlusion,
        Emissive,
        Unknown
    }

    public interface IglTFTextureinfo
    {
        glTFTextureTypes TextureType { get; }
    }

    [Serializable]
    public abstract class glTFTextureInfo : JsonSerializableBase, IglTFTextureinfo
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int index = -1;

        [JsonSchema(Minimum = 0)]
        public int texCoord;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => index);
            f.KeyValue(() => texCoord);
        }

        public abstract glTFTextureTypes TextureType { get; }
    }


    [Serializable]
    public class glTFMaterialBaseColorTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType
        {
            get { return glTFTextureTypes.BaseColor; }
        }
    }

    [Serializable]
    public class glTFMaterialMetallicRoughnessTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType
        {
            get { return glTFTextureTypes.Metallic; }
        }
    }

    [Serializable]
    public class glTFMaterialNormalTextureInfo : glTFTextureInfo
    {
        public float scale = 1.0f;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => scale);
            base.SerializeMembers(f);
        }

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

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => strength);
            base.SerializeMembers(f);
        }

        public override glTFTextureTypes TextureType
        {
            get { return glTFTextureTypes.Occlusion; }
        }
    }

    [Serializable]
    public class glTFMaterialEmissiveTextureInfo : glTFTextureInfo
    {
        public override glTFTextureTypes TextureType
        {
            get { return glTFTextureTypes.Emissive; }
        }
    }

    [Serializable]
    public class glTFPbrMetallicRoughness : JsonSerializableBase
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
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (baseColorTexture != null)
            {
                f.Key("baseColorTexture"); f.GLTFValue(baseColorTexture);
            }
            if (baseColorFactor != null)
            {
                f.KeyValue(() => baseColorFactor);
            }
            if (metallicRoughnessTexture != null)
            {
                f.Key("metallicRoughnessTexture"); f.GLTFValue(metallicRoughnessTexture);
            }
            f.KeyValue(() => metallicFactor);
            f.KeyValue(() => roughnessFactor);
        }
    }

    [Serializable]
    public class glTFMaterial : JsonSerializableBase
    {
        public string name;
        public glTFPbrMetallicRoughness pbrMetallicRoughness;
        public glTFMaterialNormalTextureInfo normalTexture = null;

        public glTFMaterialOcclusionTextureInfo occlusionTexture = null;

        public glTFMaterialEmissiveTextureInfo emissiveTexture = null;

        [JsonSchema(MinItems = 3, MaxItems = 3)]
        [ItemJsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float[] emissiveFactor;

        [JsonSchema(EnumValues = new object[] { "OPAQUE", "MASK", "BLEND" }, EnumSerializationType = EnumSerializationType.AsUpperString)]
        public string alphaMode;

        [JsonSchema(Dependencies = new string[] { "alphaMode" }, Minimum = 0.0)]
        public float alphaCutoff = 0.5f;

        public bool doubleSided;

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFMaterial_extensions extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (!String.IsNullOrEmpty(name))
            {
                f.Key("name"); f.Value(name);
            }
            if (pbrMetallicRoughness != null)
            {
                f.Key("pbrMetallicRoughness"); f.GLTFValue(pbrMetallicRoughness);
            }
            if (normalTexture != null)
            {
                f.Key("normalTexture"); f.GLTFValue(normalTexture);
            }
            if (occlusionTexture != null)
            {
                f.Key("occlusionTexture"); f.GLTFValue(occlusionTexture);
            }
            if (emissiveTexture != null)
            {
                f.Key("emissiveTexture"); f.GLTFValue(emissiveTexture);
            }
            if (emissiveFactor != null)
            {
                f.Key("emissiveFactor"); f.Serialize(emissiveFactor);
            }

            f.KeyValue(() => doubleSided);

            if (!string.IsNullOrEmpty(alphaMode))
            {
                f.KeyValue(() => alphaMode);
            }

            if (extensions != null)
            {
                f.Key("extensions"); f.GLTFValue(extensions);
            }
        }

        public glTFTextureInfo[] GetTextures()
        {
            return new glTFTextureInfo[]
            {
                pbrMetallicRoughness.baseColorTexture,
                pbrMetallicRoughness.metallicRoughnessTexture,
                normalTexture,
                occlusionTexture,
                emissiveTexture
            };
        }
    }
}

using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFBuffer
    {
        public string uri;

        [JsonSchema(Required = true, Minimum = 1)]
        public int byteLength;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }

    [Serializable]
    public class glTFBufferView
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int buffer;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        [JsonSchema(Required = true, Minimum = 1)]
        public int byteLength;

        [JsonSchema(Minimum = 4, Maximum = 252, MultipleOf = 4, SerializationConditions = new string[] { "false" })]
        public int byteStride;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt, EnumExcludes = new object[] { glBufferTarget.NONE }, SerializationConditions = new string[] { "value.target!=0" })]
        public glBufferTarget target;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }

    [Serializable]
    public class glTFSparseIndices
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int bufferView = -1;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        [JsonSchema(Required = true, EnumValues = new object[] { 5121, 5123, 5125 })]
        public glComponentType componentType;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFSparseValues
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int bufferView = -1;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFSparse
    {
        [JsonSchema(Required = true, Minimum = 1)]
        public int count;

        [JsonSchema(Required = true)]
        public glTFSparseIndices indices;

        [JsonSchema(Required = true)]
        public glTFSparseValues values;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFAccessor
    {
        [JsonSchema(Minimum = 0)]
        public int? bufferView;

        [JsonSchema(Minimum = 0, Dependencies = new string[] { "bufferView" })]
        public int? byteOffset;

        [JsonSchema(Required = true, EnumValues = new object[] { "SCALAR", "VEC2", "VEC3", "VEC4", "MAT2", "MAT3", "MAT4" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string type;

        public int TypeCount
        {
            get
            {
                switch (type)
                {
                    case "SCALAR":
                        return 1;
                    case "VEC2":
                        return 2;
                    case "VEC3":
                        return 3;
                    case "VEC4":
                    case "MAT2":
                        return 4;
                    case "MAT3":
                        return 9;
                    case "MAT4":
                        return 16;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [JsonSchema(Required = true, EnumSerializationType = EnumSerializationType.AsInt)]
        public glComponentType componentType;

        [JsonSchema(Required = true, Minimum = 1)]
        public int count;

        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] max;

        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] min;

        public bool normalized;
        public glTFSparse sparse;

        // empty schemas
        public string name;

        public glTFExtension extensions;

        public glTFExtension extras;

        public int GetStride()
        {
            return componentType.GetByteSize() * TypeCount;
        }

        public int CalcByteSize()
        {
            return GetStride() * count;
        }
    }
}

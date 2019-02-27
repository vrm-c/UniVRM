using System;
using System.Linq;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFBuffer : JsonSerializableBase
    {
        IBytesBuffer Storage;

        public void OpenStorage(IStorage storage)
        {
            Storage = new ArraySegmentByteBuffer(storage.Get(uri));
            /*
            if (string.IsNullOrEmpty(uri))
            {
                Storage = (glbDataBytes);
            }
            else
            {
                Storage = new UriByteBuffer(baseDir, uri);
            }
            */
        }

        public glTFBuffer()
        {

        }

        public glTFBuffer(IBytesBuffer storage)
        {
            Storage = storage;
        }

        public string uri;

        [JsonSchema(Required = true, Minimum = 1)]
        public int byteLength;

        // empty schemas
        public object extensions;
        public object extras;
        public string name;

        public glTFBufferView Append<T>(T[] array, glBufferTarget target) where T : struct
        {
            return Append(new ArraySegment<T>(array), target);
        }
        public glTFBufferView Append<T>(ArraySegment<T> segment, glBufferTarget target) where T : struct
        {
            var view = Storage.Extend(segment, target);
            byteLength = Storage.GetBytes().Count;
            return view;
        }

        public ArraySegment<Byte> GetBytes()
        {
            return Storage.GetBytes();
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                f.KeyValue(() => uri);
            }
            f.KeyValue(() => byteLength);
        }
    }

    [Serializable]
    public class glTFBufferView : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int buffer;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        [JsonSchema(Required = true, Minimum = 1)]
        public int byteLength;

        [JsonSchema(Minimum = 4, Maximum = 252, MultipleOf = 4)]
        public int byteStride;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt, EnumExcludes = new object[] { glBufferTarget.NONE })]
        public glBufferTarget target;

        // empty schemas
        public object extensions;
        public object extras;
        public string name;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => buffer);
            f.KeyValue(() => byteOffset);
            f.KeyValue(() => byteLength);
            if (target != glBufferTarget.NONE)
            {
                f.Key("target"); f.Value((int)target);
            }
            /* When this is not defined, data is tightly packed. When two or more accessors use the same bufferView, this field must be defined.
            if (byteStride >= 4)
            {
                f.KeyValue(() => byteStride);
            }
            */
        }
    }

    [Serializable]
    public class glTFSparseIndices : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int bufferView = -1;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        [JsonSchema(Required = true, EnumValues = new object[] { 5121, 5123, 5125 })]
        public glComponentType componentType;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => bufferView);
            f.KeyValue(() => byteOffset);
            f.Key("componentType"); f.Value((int)componentType);
        }
    }

    [Serializable]
    public class glTFSparseValues : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int bufferView = -1;

        [JsonSchema(Minimum = 0)]
        public int byteOffset;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => bufferView);
            f.KeyValue(() => byteOffset);
        }
    }

    [Serializable]
    public class glTFSparse : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 1)]
        public int count;

        [JsonSchema(Required = true)]
        public glTFSparseIndices indices;

        [JsonSchema(Required = true)]
        public glTFSparseValues values;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => count);
            f.Key("indices"); f.GLTFValue(indices);
            f.Key("values"); f.GLTFValue(values);
        }
    }

    [Serializable]
    public class glTFAccessor : JsonSerializableBase
    {
        [JsonSchema(Minimum = 0)]
        public int bufferView = -1;

        [JsonSchema(Minimum = 0, Dependencies = new string[] { "bufferView" })]
        public int byteOffset;

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

        public object extensions;

        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => bufferView);
            f.KeyValue(() => byteOffset);
            f.KeyValue(() => type);
            f.Key("componentType"); f.Value((int)componentType);
            f.KeyValue(() => count);
            if (max != null && max.Any())
            {
                f.KeyValue(() => max);
            }
            if (min != null && min.Any())
            {
                f.KeyValue(() => min);
            }

            if (sparse != null && sparse.count > 0)
            {
                f.Key("sparse"); f.GLTFValue(sparse);
            }

            f.KeyValue(() => normalized);
            f.KeyValue(() => name);
        }
    }
}

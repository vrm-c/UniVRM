using System;
using System.IO;

namespace UniGLTF
{
    public class NullableSerialization : IValueSerialization
    {
        public NullableSerialization(Type t, string path, JsonSchemaAttribute attr, string prefix)
        {
            if (t != typeof(int))
            {
                throw new NotImplementedException();
            }
        }

        public Type ValueType => typeof(Int32);
        public bool IsInline => true;

        public string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            return $"{argName}.HasValue";
        }

        public string GenerateSerializerCall(string callName, string argName)
        {
            return $"f.Value({argName}.Value)";
        }

        public void GenerateSerializer(StreamWriter writer, string callName)
        {
            throw new NotImplementedException();
        }

        public string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetInt32()";
        }

        public void GenerateDeserializer(StreamWriter writer, string callName)
        {
            throw new NotImplementedException();
        }
    }
}

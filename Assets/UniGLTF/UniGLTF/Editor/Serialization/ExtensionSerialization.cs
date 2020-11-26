using System;
using System.IO;

namespace UniGLTF
{
    public class ExtensionSerialization : IValueSerialization
    {
        public Type ValueType => typeof(object);

        public bool IsInline => true;

        public void GenerateDeserializer(StreamWriter writer, string callName)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateDeserializerCall(string callName, string argName)
        {
            return $"new glTFExtensionImport({argName})";
        }

        public string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            return $"{argName}!=null";
        }

        public void GenerateSerializer(StreamWriter writer, string callName)
        {
            throw new NotImplementedException();
        }

        public string GenerateSerializerCall(string callName, string argName)
        {
            return $"{argName}.Serialize(f)";
        }
    }
}

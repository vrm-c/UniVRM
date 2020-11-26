using System;
using System.IO;

namespace UniGLTF
{
    public abstract class CollectionSerializationBase : IValueSerialization
    {
        public Type ValueType
        {
            get;
            protected set;
        }

        public bool IsInline
        {
            get { return false; }
        }

        public abstract void GenerateDeserializer(StreamWriter writer, string callName);

        public string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{callName}({argName})";
        }

        public abstract string CreateSerializationCondition(string argName, JsonSchemaAttribute t);

        public abstract void GenerateSerializer(StreamWriter writer, string callName);

        public string GenerateSerializerCall(string callName, string argName)
        {
            return $"{callName}(f, {argName})";
        }
    }
}

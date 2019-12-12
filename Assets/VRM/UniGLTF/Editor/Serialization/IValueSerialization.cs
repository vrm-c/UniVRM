using System;
using System.IO;

namespace UniGLTF
{
    // Use this for initialization
    public interface IValueSerialization
    {
        Type ValueType { get; }

        bool IsInline { get; }

        string GenerateDeserializerCall(string callName, string argName);

        void GenerateDeserializer(StreamWriter writer, string callName);
    }
}

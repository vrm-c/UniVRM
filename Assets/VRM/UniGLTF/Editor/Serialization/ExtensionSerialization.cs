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
            return argName;
        }
    }
}

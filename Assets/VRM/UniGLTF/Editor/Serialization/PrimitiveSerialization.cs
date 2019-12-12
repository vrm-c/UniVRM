using System;
using System.IO;

namespace UniGLTF
{
    public abstract class PrimitiveSerializationBase : IValueSerialization
    {
        public bool IsInline
        {
            get { return true; }
        }

        public abstract Type ValueType { get; }

        public void GenerateDeserializer(StreamWriter writer, string callName)
        {
            throw new System.NotImplementedException();
        }

        public abstract string GenerateDeserializerCall(string callName, string argName);
    }

    public class Int8Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(SByte); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetInt8()";
        }
    }

    public class Int16Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Int16); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetInt16()";
        }
    }

    public class Int32Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Int32); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetInt32()";
        }
    }

    public class Int64Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Int64); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetInt64()";
        }
    }

    public class UInt8Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Byte); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetUInt8()";
        }
    }

    public class UInt16Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(UInt16); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetUInt16()";
        }
    }

    public class UInt32Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(UInt32); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetUInt32()";
        }
    }

    public class UInt64Serialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(UInt64); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetUInt64()";
        }
    }

    public class SingleSerialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Single); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetSingle()";
        }
    }

    public class DoubleSerialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Double); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetDouble()";
        }
    }

    public class BooleanSerialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(Boolean); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetBoolean()";
        }
    }

    public class StringSerialization : PrimitiveSerializationBase
    {
        public override Type ValueType
        {
            get { return typeof(String); }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return argName + ".GetString()";
        }
    }

    public class EnumIntSerialization : PrimitiveSerializationBase
    {
        Type m_type;

        public override Type ValueType
        {
            get { return m_type; }
        }

        public EnumIntSerialization(Type t)
        {
            m_type = t;
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return string.Format("({0}){1}.GetInt32()", m_type.Name, argName);
        }
    }
}
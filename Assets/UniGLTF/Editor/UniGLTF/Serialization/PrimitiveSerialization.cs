using System;
using System.Collections.Generic;
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

        public virtual string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            return "true";
        }

        public void GenerateSerializer(StreamWriter writer, string callName)
        {
            throw new NotImplementedException();
        }

        public string GenerateSerializerCall(string callName, string argName)
        {
            return $"f.Value({argName})";
        }

        public override string ToString()
        {
            return ValueType.ToString();
        }
    }

    public abstract class NumberSerializationBase : PrimitiveSerializationBase
    {
        public override string CreateSerializationCondition(string argName, JsonSchemaAttribute attr)
        {
            if (attr == null)
            {
                return "true";
            }

            var sb = new List<string>();
            if (!double.IsNaN(attr.Minimum))
            {
                if (attr.ExclusiveMinimum)
                {
                    sb.Add($"{argName}>{attr.Minimum}"); // to int
                }
                else
                {
                    sb.Add($"{argName}>={attr.Minimum}"); // to int
                }
            }

            if (sb.Count > 0)
            {
                return string.Join("&&", sb);
            }

            return "true";
        }
    }

    public class Int8Serialization : NumberSerializationBase
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

    public class Int16Serialization : NumberSerializationBase
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

    public class Int32Serialization : NumberSerializationBase
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

    public class Int64Serialization : NumberSerializationBase
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

    public class UInt8Serialization : NumberSerializationBase
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

    public class UInt16Serialization : NumberSerializationBase
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

    public class UInt32Serialization : NumberSerializationBase
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

    public class UInt64Serialization : NumberSerializationBase
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

    public class SingleSerialization : NumberSerializationBase
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

    public class DoubleSerialization : NumberSerializationBase
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

        public override string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            return $"!string.IsNullOrEmpty({argName})";
        }
    }
}

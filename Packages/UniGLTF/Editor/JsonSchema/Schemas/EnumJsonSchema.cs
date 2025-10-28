using System;
using System.Linq;

namespace UniGLTF.JsonSchema.Schemas
{
    public struct EnumValue
    {
        public string Name;
        public int Value;

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }

    public class EnumJsonSchema : JsonSchemaBase
    {
        public readonly EnumValue[] Values;

        public EnumJsonSchema(in JsonSchemaSource source) : base(source)
        {
            Values = source.enumValues.Select(x => new EnumValue
            {
                Name = x.Key,
                Value = x.Value
            }).ToArray();
        }

        public override string ValueType => Title;

        public override bool IsInline => true;

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"({ValueType})argName";
        }

        public override string CreateSerializationCondition(string argName)
        {
            return "true";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"f.Value((int){argName})";
        }

        public override string ToString()
        {
            var values = string.Join(", ", Values);
            return $"{base.ToString()} {{{values}}}";
        }
    }

    public class EnumStringJsonSchema : JsonSchemaBase
    {
        public readonly String[] Values;

        public EnumStringJsonSchema(in JsonSchemaSource source) : base(source)
        {
            Values = source.enumStringValues;
        }

        public override string ValueType => Title;

        public override bool IsInline => true;

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"({ValueType})Enum.Parse(typeof({ValueType}), {argName}.GetString(), true)";
        }

        public override string CreateSerializationCondition(string argName)
        {
            return "true";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"f.Value({argName}.ToString())";
        }

        public override string ToString()
        {
            var values = string.Join(", ", Values);
            return $"{base.ToString()} {{{values}}}";
        }
    }
}

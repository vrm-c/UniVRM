using System;

namespace UniGLTF.JsonSchema.Schemas
{
    public abstract class PrimitiveJsonSchemaBase : JsonSchemaBase
    {
        protected PrimitiveJsonSchemaBase(in JsonSchemaSource source) : base(source)
        { }

        public override bool IsInline => true;

        public override string CreateSerializationCondition(string argName)
        {
            if (IsArrayItem)
            {
                throw new NotImplementedException();
            }
            else
            {
                return $"{argName}.HasValue";
            }
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            if (IsArrayItem)
            {
                return $"f.Value({argName})";
            }
            else
            {
                return $"f.Value({argName}.GetValueOrDefault())";
            }
        }
    }

    public class BoolJsonSchema : PrimitiveJsonSchemaBase
    {
        public override string ValueType => IsArrayItem ? "bool" : "bool?";

        public BoolJsonSchema(in JsonSchemaSource src) : base(src)
        {
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{argName}.GetBoolean()";
        }
    }

    public class IntegerJsonSchema : PrimitiveJsonSchemaBase
    {
        public readonly int? Minimum;
        public readonly bool ExclusiveMinimum;
        public readonly int? Maximum;
        public readonly int? MultipleOf;

        public string IndexTargetJsonPath;

        public IntegerJsonSchema(in JsonSchemaSource source) : base(source)
        {
            if (source.minimum.HasValue)
            {
                Minimum = (int)source.minimum.Value;
            }
            ExclusiveMinimum = source.exclusiveMinimum;
            if (source.maximum.HasValue)
            {
                Maximum = (int)source.maximum.Value;
            }
            if (source.multipleOf.HasValue)
            {
                MultipleOf = (int)source.multipleOf.Value;
            }
        }

        public override string ValueType => IsArrayItem ? "int" : "int?";

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{argName}.GetInt32()";
        }
    }

    public class NumberJsonSchema : PrimitiveJsonSchemaBase
    {
        public readonly double? Minimum;
        public readonly bool ExclusiveMinimum;
        public readonly double? Maximum;
        public readonly double? MultipleOf;

        public NumberJsonSchema(in JsonSchemaSource source) : base(source)
        {
            if (source.minimum.HasValue)
            {
                Minimum = source.minimum.Value;
            }
            ExclusiveMinimum = source.exclusiveMinimum;
            if (source.maximum.HasValue)
            {
                Maximum = source.maximum.Value;
            }
            if (source.multipleOf.HasValue)
            {
                MultipleOf = source.multipleOf.Value;
            }
        }

        public override string ValueType => IsArrayItem ? "float" : "float?";

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{argName}.GetSingle()";
        }
    }

}

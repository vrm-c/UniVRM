using System;

namespace UniGLTF.JsonSchema.Schemas
{
    public class DictionaryJsonSchema : JsonSchemaBase
    {
        public readonly JsonSchemaBase AdditionalProperties;

        public readonly int MinProperties;

        public DictionaryJsonSchema(in JsonSchemaSource source, bool useUpperCamelName) : base(source)
        {
            AdditionalProperties = source.additionalProperties.Create(useUpperCamelName);
            MinProperties = source.minProperties.GetValueOrDefault();
        }

        public override string ValueType => throw new NotImplementedException();

        public override bool IsInline => throw new NotImplementedException();

        public override string CreateSerializationCondition(string argName)
        {
            throw new NotImplementedException();
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            throw new NotImplementedException();
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            throw new NotImplementedException();
        }
    }
}

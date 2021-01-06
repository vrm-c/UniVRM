namespace UniGLTF.JsonSchema.Schemas
{
    public class StringJsonSchema : JsonSchemaBase
    {
        public readonly string Pattern;

        public StringJsonSchema(in JsonSchemaSource source) : base(source)
        {
            Pattern = source.pattern;
        }

        public override string ValueType => "string";

        public override bool IsInline => true;

        public override string CreateSerializationCondition(string argName)
        {
            return $"!string.IsNullOrEmpty({argName})";
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{argName}.GetString()";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"f.Value({argName})";
        }
    }
}

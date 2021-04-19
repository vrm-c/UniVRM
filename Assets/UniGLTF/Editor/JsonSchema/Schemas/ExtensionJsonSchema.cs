namespace UniGLTF.JsonSchema.Schemas
{
    /// <summary>
    /// glTF の extensions, extras を処理するための専用クラス
    /// </summary>
    public class ExtensionJsonSchema : JsonSchemaBase
    {
        public ExtensionJsonSchema(in JsonSchemaSource source) : base(source)
        {
        }

        public override string ValueType => "object";

        public override bool IsInline => true;

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"new glTFExtensionImport({argName})";
        }

        public override string CreateSerializationCondition(string argName)
        {
            return $"{argName}!=null";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"({argName} as glTFExtension).Serialize(f)";
        }
    }
}

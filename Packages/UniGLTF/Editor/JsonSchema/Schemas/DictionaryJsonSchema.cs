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

        public override string ValueType => $"Dictionary<string, {AdditionalProperties.ValueType}>";

        public override bool IsInline => false;

        public override string CreateSerializationCondition(string argName)
        {
            return $"{argName}!=null&&{argName}.Count()>0";
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{callName}({argName})";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"{callName}(f, {argName})";
        }

        public override void GenerateDeserializer(TraverseContext writer, string callName)
        {
            if (writer.Used.Contains(callName))
            {
                return;
            }
            writer.Used.Add(callName);

            var itemCallName = callName + "_ITEM";

            {
                writer.Write(@"
public static $0 $2(JsonNode parsed)
{
    var value = new $1();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), $3);
    }
	return value;
} 
"
    .Replace("$0", ValueType)
    .Replace("$1", ValueType)
    .Replace("$2", callName)
    .Replace("$3", AdditionalProperties.GenerateDeserializerCall(itemCallName, "kv.Value"))
    );

            }

            if (!AdditionalProperties.IsInline)
            {
                AdditionalProperties.GenerateDeserializer(writer, itemCallName);
            }
        }

        public override void GenerateSerializer(TraverseContext writer, string callName)
        {
            if (writer.Used.Contains(callName))
            {
                return;
            }
            writer.Used.Add(callName);

            var itemCallName = callName + "_ITEM";
            writer.Write($@"
public static void {callName}(JsonFormatter f, {ValueType} value)
{{
    f.BeginMap();

    foreach(var kv in value)
    {{
        f.Key(kv.Key);
    "
);

            writer.Write($"{AdditionalProperties.GenerateSerializerCall(itemCallName, "kv.Value")};\n");

            writer.Write(@"
    }
    f.EndMap();
}
");

            if (!AdditionalProperties.IsInline)
            {
                AdditionalProperties.GenerateSerializer(writer, itemCallName);
            }
        }
    }
}

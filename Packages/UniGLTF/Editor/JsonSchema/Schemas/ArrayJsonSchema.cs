using System;
using System.IO;

namespace UniGLTF.JsonSchema.Schemas
{
    public class ArrayJsonSchema : JsonSchemaBase
    {
        public readonly JsonSchemaBase Items;
        public readonly bool UniqueItems;
        public readonly int MinItems;
        public readonly int? MaxItems;

        public ArrayJsonSchema(in JsonSchemaSource source, bool useUpperCamelName) : base(source)
        {
            Items = source.items.Create(useUpperCamelName);
            Items.IsArrayItem = true;
            UniqueItems = source.uniqueItems.GetValueOrDefault();
            MinItems = source.minItems.GetValueOrDefault();
            if (source.maxItems.HasValue)
            {
                MaxItems = source.maxItems.Value;
            }
        }

        public override bool IsInline => false;

        bool ItemsIsPrimitiveType => Items is PrimitiveJsonSchemaBase;

        public override string ValueType
        {
            get
            {
                if (ItemsIsPrimitiveType)
                {
                    return $"{Items.ValueType}[]";
                }
                else
                {
                    return $"List<{Items.ValueType}>";
                }
            }
        }

        public override string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{callName}({argName})";
        }

        public override void GenerateDeserializer(TraverseContext writer, string callName)
        {
            if (writer.Used.Contains(callName))
            {
                return;
            }
            writer.Used.Add(callName);

            var itemCallName = callName + "_ITEM";

            if (ItemsIsPrimitiveType)
            {

                writer.Write(@"
public static $0 $2(JsonNode parsed)
{
    var value = new $1[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = $3;
    }
	return value;
} 
"
    .Replace("$0", ValueType)
    .Replace("$1", Items.ValueType)
    .Replace("$2", callName)
    .Replace("$3", Items.GenerateDeserializerCall(itemCallName, "x"))
    );

            }
            else
            {
                writer.Write(@"
public static $0 $2(JsonNode parsed)
{
    var value = new $1();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add($3);
    }
	return value;
} 
"
    .Replace("$0", ValueType)
    .Replace("$1", ValueType)
    .Replace("$2", callName)
    .Replace("$3", Items.GenerateDeserializerCall(itemCallName, "x"))
    );

            }

            if (!Items.IsInline)
            {
                Items.GenerateDeserializer(writer, itemCallName);
            }
        }

        public override string CreateSerializationCondition(string argName)
        {
            return $"{argName}!=null&&{argName}.Count()>={MinItems}";
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"{callName}(f, {argName})";
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
    f.BeginList();

    foreach(var item in value)
    {{
    "
);

            writer.Write($"{Items.GenerateSerializerCall(itemCallName, "item")};\n");

            writer.Write(@"
    }
    f.EndList();
}
");

            if (!Items.IsInline)
            {
                Items.GenerateSerializer(writer, itemCallName);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniGLTF.JsonSchema.Schemas
{
    public class ObjectJsonSchema : JsonSchemaBase
    {
        public string[] Required;

        public readonly Dictionary<string, JsonSchemaBase> Properties = new Dictionary<string, JsonSchemaBase>();

        bool m_useUpperCamelName;

        public ObjectJsonSchema(in JsonSchemaSource source, bool useUpperCamelName) : base(source)
        {
            m_useUpperCamelName = useUpperCamelName;

            foreach (var kv in source.EnumerateProperties())
            {
                var prop = kv.Value.Create(useUpperCamelName);
                if (prop is null)
                {
                    throw new NotImplementedException();
                }

                var key = kv.Key;
                Properties.Add(key, prop);
            }
            Required = source.required;
        }

        public override string ToString()
        {
            var values = "";
            if (Required != null && Required.Any())
            {
                values = $" require: {{{string.Join(", ", Required)}}}";
            }
            return $"{base.ToString()}{values}";
        }

        public override string ValueType => Title;

        public override bool IsInline => false;

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

            writer.Write(@"
public static $0 $2(JsonNode parsed)
{
    var value = new $0();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();
"
.Replace("$0", ValueType)
.Replace("$2", callName)
);

            foreach (var kv in Properties)
            {
                writer.Write(@"
        if(key==""$0""){
            value.$2 = $1;
            continue;
        }
"
.Replace("$0", kv.Key)
.Replace("$1", kv.Value.GenerateDeserializerCall($"Deserialize_{kv.Key.ToUpperCamel()}", "kv.Value"))
.Replace("$2", kv.Key.ToUpperCamel())
);
            }

            writer.Write(@"
    }
    return value;
}
");

            foreach (var kv in Properties)
            {
                if (!kv.Value.IsInline)
                {
                    kv.Value.GenerateDeserializer(writer, $"Deserialize_{kv.Key.ToUpperCamel()}");
                }
            }
        }

        public override string CreateSerializationCondition(string argName)
        {
            return $"{argName}!=null";
        }

        /// <summary>
        /// シリアライザーのコード生成
        /// 
        /// ObjectのFieldのみ値によって、出力するか否かの判定が必用。
        /// 
        /// 例: 空文字列は出力しない
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="callName"></param>
        public override void GenerateSerializer(TraverseContext writer, string callName)
        {
            if (writer.Used.Contains(callName))
            {
                return;
            }
            writer.Used.Add(callName);

            writer.Write($@"
public static void {callName}(JsonFormatter f, {ValueType} value)
{{
    f.BeginMap();

"
);

            foreach (var kv in Properties)
            {
                var valueName = $"value.{kv.Key.ToUpperCamel()}";
                var condition = "";
                writer.Write($@"
    if({kv.Value.CreateSerializationCondition(valueName)}{condition}){{
        f.Key(""{kv.Key}"");                
        {kv.Value.GenerateSerializerCall($"Serialize_{kv.Key.ToUpperCamel()}", valueName)};
    }}
");
            }

            writer.Write(@"
    f.EndMap();
}
");

            foreach (var kv in Properties)
            {
                if (!kv.Value.IsInline)
                {
                    kv.Value.GenerateSerializer(writer, $"Serialize_{kv.Key.ToUpperCamel()}");
                }
            }
        }

        public override string GenerateSerializerCall(string callName, string argName)
        {
            return $"{callName}(f, {argName})";
        }
    }
}

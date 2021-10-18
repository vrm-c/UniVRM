using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UniGLTF
{
    public class ObjectSerialization : CollectionSerializationBase
    {
        string m_path;
        FieldSerializationInfo[] m_fsi;

        public ObjectSerialization(Type t, string path, string prefix)
        {
            ValueType = t;
            m_path = path;
            m_fsi = t.GetFields(DeserializerGenerator.FIELD_FLAGS)
            .Where(x =>
            {
                if (x.IsLiteral && !x.IsInitOnly)
                {
                    // const
                    return false;
                }
                return true;
            })
            .Select(x =>
            {
                return new FieldSerializationInfo(x, path, prefix);
            }).ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var x in m_fsi)
            {
                sb.Append(x.ToString() + "\n");
            }
            return sb.ToString();
        }

        public override void GenerateDeserializer(StreamWriter writer, string parentName)
        {
            writer.Write(@"
public static $0 $2(JsonNode parsed)
{
    var value = new $0();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();
"
.Replace("$0", ValueType.Name)
.Replace("$2", parentName)
);

            foreach (var f in m_fsi)
            {
                writer.Write(@"
        if(key==""$0""){
            value.$0 = $1;
            continue;
        }
"
.Replace("$0", f.Name)
.Replace("$1", f.Serialization.GenerateDeserializerCall(f.FunctionName, "kv.Value"))
);
            }

            writer.Write(@"
    }
    return value;
}
");

            foreach (var f in m_fsi)
            {
                if (!f.Serialization.IsInline)
                {
                    f.Serialization.GenerateDeserializer(writer, f.FunctionName);
                }
            }
        }

        public override string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
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
        public override void GenerateSerializer(StreamWriter writer, string callName)
        {
            writer.Write($@"
public static void {callName}(JsonFormatter f, {ValueType.Name} value)
{{
    f.BeginMap();

"
);

            foreach (var f in m_fsi)
            {
                var valueName = $"value.{f.Name}";
                var condition = "";
                if (f.Attribute != null && f.Attribute.SerializationConditions != null)
                {
                    condition = "&&" + string.Join("&&", f.Attribute.SerializationConditions);
                }
                writer.Write($@"
    if({f.Serialization.CreateSerializationCondition(valueName, f.Attribute)}{condition}){{
        f.Key(""{f.Name}"");
        {f.Serialization.GenerateSerializerCall(f.FunctionName, valueName)};
    }}
");
            }

            writer.Write(@"
    f.EndMap();
}
");

            foreach (var f in m_fsi)
            {
                if (!f.Serialization.IsInline)
                {
                    f.Serialization.GenerateSerializer(writer, f.FunctionName);
                }
            }
        }
    }
}

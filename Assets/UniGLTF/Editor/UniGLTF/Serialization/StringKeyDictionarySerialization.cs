using System;
using System.IO;

namespace UniGLTF
{
    public class StringKeyDictionarySerialization : CollectionSerializationBase
    {
        IValueSerialization m_inner;

        public StringKeyDictionarySerialization(Type t, IValueSerialization inner)
        {
            ValueType = t;
            m_inner = inner;
        }

        public override void GenerateDeserializer(StreamWriter writer, string callName)
        {
            var itemCallName = callName + "_ITEM";
            writer.Write(@"
 
public static $0 $2(JsonNode parsed)
{
    var value = new Dictionary<string, $1>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), $3);
    }
	return value;
}
"
.Replace("$0", JsonSchemaAttribute.GetTypeName(ValueType))
.Replace("$1", m_inner.ValueType.Name)
.Replace("$2", callName)
.Replace("$3", m_inner.GenerateDeserializerCall(itemCallName, "kv.Value"))
);

            if (!m_inner.IsInline)
            {
                m_inner.GenerateDeserializer(writer, itemCallName);
            }
        }

        public override string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            // return $"{argName}!=null&&{argName}.Count>0";

            // this check is only /extensions/VRM/materialProperties/*
            // should export empty dictionary.
            return $"{argName}!=null";
        }

        public override void GenerateSerializer(StreamWriter writer, string callName)
        {
            var itemCallName = callName + "_ITEM";
            writer.Write($@"
public static void {callName}(JsonFormatter f, Dictionary<string, {m_inner.ValueType.Name}> value)
{{
    f.BeginMap();
    foreach(var kv in value)
    {{
        f.Key(kv.Key);
        {m_inner.GenerateSerializerCall(itemCallName, "kv.Value")};
    }}
    f.EndMap();
}}
");

            if (!m_inner.IsInline)
            {
                m_inner.GenerateSerializer(writer, itemCallName);
            }
        }
    }
}

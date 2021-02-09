using System;
using System.IO;

namespace UniGLTF
{
    public class ArraySerialization : CollectionSerializationBase
    {
        IValueSerialization m_inner;

        public ArraySerialization(Type t, IValueSerialization inner)
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
    var value = new $1[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = $3;
    }
	return value;
} 
"
.Replace("$0", JsonSchemaAttribute.GetTypeName(ValueType))
.Replace("$1", m_inner.ValueType.Name)
.Replace("$2", callName)
.Replace("$3", m_inner.GenerateDeserializerCall(itemCallName, "x"))
);

            if (!m_inner.IsInline)
            {
                m_inner.GenerateDeserializer(writer, itemCallName);
            }
        }

        public override string CreateSerializationCondition(string argName, JsonSchemaAttribute t)
        {
            int min = 0;
            if (t != null)
            {
                min = t.MinItems;
            }

            return $"{argName}!=null&&{argName}.Length>={min}";
        }

        public override void GenerateSerializer(StreamWriter writer, string callName)
        {
            var itemCallName = callName + "_ITEM";
            writer.Write($@"
public static void {callName}(JsonFormatter f, {m_inner.ValueType.Name}[] value)
{{
    f.BeginList();

    foreach(var item in value)
    {{
    "
);

            writer.Write($"{m_inner.GenerateSerializerCall(itemCallName, "item")};\n");

            writer.Write(@"
    }
    f.EndList();
}
");

            if (!m_inner.IsInline)
            {
                m_inner.GenerateSerializer(writer, itemCallName);
            }
        }
    }
}

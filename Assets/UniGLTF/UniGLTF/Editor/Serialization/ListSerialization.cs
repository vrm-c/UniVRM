using System;
using System.IO;
using System.Linq;

namespace UniGLTF
{
    public class ListSerialization : FunctionSerializationBase
    {
        IValueSerialization m_inner;

        public ListSerialization(Type t, IValueSerialization inner)
        {
            ValueType = t;
            m_inner = inner;
        }

        public override string ToString()
        {
            return m_inner.ToString();
        }

        public override void GenerateDeserializer(StreamWriter writer, string callName)
        {
            var itemCallName = callName + "_LIST";
            writer.Write(@"
public static $0 $2(ListTreeNode<JsonValue> parsed)
{
    var value = new List<$1>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add($3);
    }
	return value;
}"
.Replace("$0", UniJSON.JsonSchemaAttribute.GetTypeName(ValueType))
.Replace("$1", m_inner.ValueType.Name)
.Replace("$2", callName)
.Replace("$3", m_inner.GenerateDeserializerCall(itemCallName, "x"))
);

            if (!m_inner.IsInline)
            {
                m_inner.GenerateDeserializer(writer, itemCallName);
            }
        }
    }
}

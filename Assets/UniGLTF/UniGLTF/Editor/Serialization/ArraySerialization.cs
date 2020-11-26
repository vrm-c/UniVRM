using System;
using System.IO;

namespace UniGLTF
{
    public class ArraySerialization : FunctionSerializationBase
    {
        IValueSerialization m_inner;

        public ArraySerialization(Type t, IValueSerialization inner)
        {
            ValueType = t;
            m_inner = inner;
        }
        public override void GenerateDeserializer(StreamWriter writer, string callName)
        {
            var itemCallName = callName + "_ARRAY";

            writer.Write(@"
public static $0 $2(ListTreeNode<JsonValue> parsed)
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

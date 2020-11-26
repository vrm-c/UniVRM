using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UniGLTF
{
    public abstract class SerializationBase : IValueSerialization
    {
        public Type ValueType
        {
            get;
            protected set;
        }

        public bool IsInline
        {
            get { return false; }
        }

        public abstract void GenerateDeserializer(StreamWriter writer, string callName);

        public string GenerateDeserializerCall(string callName, string argName)
        {
            return $"{callName}({argName})";
        }

        public abstract void GenerateSerializer(StreamWriter writer, string callName);

        public string GenerateSerializerCall(string callName, string argName)
        {
            return $"{callName}(f, {argName})";
        }
    }

    public class ObjectSerialization : SerializationBase
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
public static $0 $2(ListTreeNode<JsonValue> parsed)
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
                writer.Write($"    f.Key(\"{f.Name}\");\n");
                writer.Write($"    {f.Serialization.GenerateSerializerCall(f.FunctionName, $"value.{f.Name}")};\n");
                writer.Write("\n");
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

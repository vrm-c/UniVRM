using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniJSON;

namespace UniGLTF
{
    public class FieldSerializationInfo
    {
        FieldInfo m_fi;
        public FieldInfo FieldInfo
        {
            get { return m_fi; }
        }

        public string Name
        {
            get { return FieldInfo.Name; }
        }

        public string Path
        {
            get;
            private set;
        }

        public string FunctionName
        {
            get
            {
                return "Deserialize_" + Path
                .Replace("/", "_")
                .Replace("[]", "_")
                ;
            }
        }

        JsonSchemaAttribute m_attr;

        public IValueSerialization Serialization
        {
            get;
            private set;
        }

        public FieldSerializationInfo(FieldInfo fi, string path)
        {
            m_fi = fi;
            Path = path + "/" + fi.Name;
            m_attr = fi.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(JsonSchemaAttribute)) as JsonSchemaAttribute;

            Serialization = GetSerialization(m_fi.FieldType, Path);
        }

        static IValueSerialization GetSerialization(Type t, string path)
        {
            if (t.IsArray)
            {
                return new ArraySerialization(t,
                    GetSerialization(t.GetElementType(), path + "[]"));
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                return new ListSerialization(t,
                    GetSerialization(t.GetGenericArguments()[0], path + "[]"));
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && t.GetGenericArguments()[0] == typeof(string))
            {
                return new StringKeyDictionarySerialization(t, 
                    GetSerialization(t.GetGenericArguments()[1], path));
            }

            // GetCollectionType(fi.FieldType, out suffix, out t);
            if (t == typeof(sbyte))
            {
                return new Int8Serialization();
            }
            else if (t == typeof(short))
            {
                return new Int16Serialization();
            }
            else if (t == typeof(int))
            {
                return new Int32Serialization();
            }
            else if (t == typeof(long))
            {
                return new Int64Serialization();
            }
            else if (t == typeof(byte))
            {
                return new UInt8Serialization();
            }
            else if (t == typeof(ushort))
            {
                return new UInt16Serialization();
            }
            else if (t == typeof(uint))
            {
                return new UInt32Serialization();
            }
            else if (t == typeof(ulong))
            {
                return new UInt64Serialization();
            }
            else if (t == typeof(float))
            {
                return new SingleSerialization();
            }
            else if (t == typeof(double))
            {
                return new DoubleSerialization();
            }
            else if (t == typeof(string))
            {
                return new StringSerialization();
            }
            else if (t == typeof(bool))
            {
                return new BooleanSerialization();
            }
            else if (t.IsEnum)
            {
                return new EnumIntSerialization(t);
            }

            return new ObjectSerialization(t, path);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var typeName = BaseJsonSchemaAttribute.GetTypeName(m_fi.FieldType);

            if (m_attr != null)
            {
                sb.AppendLine(string.Format("{0}: {1}", Path, m_attr.GetInfo(m_fi)));
            }
            else
            {
                sb.AppendLine(string.Format("{0}: {1}", Path, typeName));
            }

            sb.Append(Serialization.ToString());
            return sb.ToString();
        }
    }
}
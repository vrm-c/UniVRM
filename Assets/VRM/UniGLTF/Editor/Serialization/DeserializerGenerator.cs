using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniJSON;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public class FieldSerializationInfo
    {
        FieldInfo m_fi;
        string m_path;

        JsonSchemaAttribute m_attr;

        ObjectSerializationInfo m_child;

        public FieldSerializationInfo(FieldInfo fi, string path)
        {
            m_fi = fi;
            m_path = path;
            m_attr = fi.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(JsonSchemaAttribute)) as JsonSchemaAttribute;

            var suffix = default(string);
            var t = default(Type);
            var isDictionary = default(bool);
            GetCollectionType(fi.FieldType, out suffix, out t, out isDictionary);

            if (t == typeof(object))
            {
                // 終わり
                return;
            }
            if (fi.IsLiteral && !fi.IsInitOnly)
            {
                // const
                // 終わり
                return;
            }
            if (t.IsClass && t.GetFields(DeserializerGenerator.FIELD_FLAGS).Length == 0)
            {
                // 終わり
                return;
            }
            if (IsPrimitive(t))
            {
                // 終わり
                return;
            }

            m_child = new ObjectSerializationInfo(t, m_path + "/" + m_fi.Name + suffix);
        }

        static void GetCollectionType(Type t, out string suffix, out Type collectionType, out bool isDictionary)
        {
            if (t.IsArray)
            {
                suffix = "[]";
                collectionType = t.GetElementType();
                isDictionary = false;
                return;
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                suffix = "[]";
                collectionType = t.GetGenericArguments()[0];
                isDictionary = false;
                return;
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && t.GetGenericArguments()[0] == typeof(string))
            {
                suffix = "";
                collectionType = t.GetGenericArguments()[1];
                isDictionary = true;
                return;
            }

            suffix = "";
            collectionType = t;
            isDictionary = false;
        }

        static bool IsPrimitive(Type t)
        {
            if (t == typeof(sbyte)
            || t == typeof(short)
            || t == typeof(int)
            || t == typeof(long)
            || t == typeof(byte)
            || t == typeof(ushort)
            || t == typeof(uint)
            || t == typeof(ulong)
            || t == typeof(float)
            || t == typeof(double)
            || t == typeof(string)
            || t == typeof(bool)
            )
            {
                return true;
            }

            if (t.IsEnum)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var typeName = BaseJsonSchemaAttribute.GetTypeName(m_fi.FieldType);

            if (m_attr != null)
            {
                sb.AppendLine(string.Format("{0}/{1}: {2}", m_path, m_fi.Name, m_attr.GetInfo(m_fi)));
            }
            else
            {
                sb.AppendLine(string.Format("{0}/{1}: {2}", m_path, m_fi.Name, typeName));
            }

            if (m_child != null)
            {
                sb.Append(m_child.ToString());
            }
            return sb.ToString();
        }
    }

    public class ObjectSerializationInfo
    {
        string m_path;
        FieldSerializationInfo[] m_fsi;

        public ObjectSerializationInfo(Type t, string path)
        {
            m_path = path;
            m_fsi = t.GetFields(DeserializerGenerator.FIELD_FLAGS).Select(x =>
            {
                return new FieldSerializationInfo(x, path);
            }).ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var x in m_fsi)
            {
                sb.Append(x.ToString());
            }
            return sb.ToString();
        }
    }

    public static class DeserializerGenerator
    {
        public const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        /// <summary>
        /// AOT向けにデシリアライザを生成する
        /// </summary>
        [MenuItem(VRM.VRMVersion.MENU + "/Generate Deserializer")]
        static void GenerateSerializer()
        {
            var info = new ObjectSerializationInfo(typeof(glTF), "gltf");
            Debug.Log(info);
        }
    }
}

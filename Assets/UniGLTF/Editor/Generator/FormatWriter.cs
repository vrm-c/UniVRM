using System;
using System.Collections.Generic;
using System.IO;
using UniGLTF.JsonSchema;
using UniGLTF.JsonSchema.Schemas;

namespace GenerateUniGLTFSerialization
{
    class FormatWriter
    {
        TextWriter m_w;
        string m_prefix;

        HashSet<string> m_used = new HashSet<string>();

        FormatWriter(TextWriter writer, string prefix)
        {
            m_w = writer;
            m_prefix = prefix;
        }

        // static string UpperCamelCase(string src)
        // {
        //     if(string.IsNullOrEmpty(src))
        //     {
        //         return "";
        //     }
        //     return src.Substring(0, 1).ToUpper() + src.Substring(1);
        // }

        // string ClassName(string src)
        // {
        //     if(string.IsNullOrEmpty(src))
        //     {
        //         // root
        //         return m_prefix;
        //     }
        //     else{
        //         return String.Join("", src.Split("__").Select(x => UpperCamelCase(x)));
        //     }
        // }

        const string EnumStringAttr = "[JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]";

        static (string, string) PropType(JsonSchemaBase schema)
        {
            switch (schema.JsonSchemaType)
            {
                case JsonSchemaType.String:
                case JsonSchemaType.Boolean:
                case JsonSchemaType.Integer:
                case JsonSchemaType.Number:
                case JsonSchemaType.Object:
                case JsonSchemaType.Array:
                    return (null, schema.ValueType);

                case JsonSchemaType.EnumString:
                    return (EnumStringAttr, schema.ValueType);
            }

            throw new NotImplementedException();
        }

        const string FieldIndent = "        ";

        void WriteObject(ObjectJsonSchema schema, string rootName = default)
        {
            if (m_used.Contains(schema.Title))
            {
                return;
            }
            m_used.Add(schema.Title);

            var className = schema.Title;
            m_w.Write($@"
    public class {className}
    {{
");

            if (!string.IsNullOrEmpty(rootName))
            {
                var indent = "        ";
                m_w.WriteLine($"{indent}public const string ExtensionName = \"{rootName}\";");
                m_w.WriteLine($"{indent}public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);");
                m_w.WriteLine();
            }

            var isFirst = true;
            foreach (var kv in schema.Properties)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    m_w.WriteLine();
                }
                if (!string.IsNullOrEmpty(kv.Value.Description))
                {
                    m_w.WriteLine($"{FieldIndent}// {kv.Value.Description}");
                }
                var (attr, propType) = PropType(kv.Value);
                if (!string.IsNullOrEmpty(attr))
                {
                    m_w.WriteLine($"{FieldIndent}{attr}");
                }
                m_w.WriteLine($"{FieldIndent}public {propType} {kv.Key.ToUpperCamel()};");
            }

            // close class
            m_w.WriteLine("    }");
        }

        void WriteEnumString(EnumStringJsonSchema schema)
        {
            if (m_used.Contains(schema.Title))
            {
                return;
            }
            m_used.Add(schema.Title);

            var className = schema.Title;
            m_w.Write($@"
    public enum {className}
    {{
");
            foreach (var value in schema.Values)
            {
                m_w.WriteLine($"        {value},");
            }

            // close
            m_w.Write(@"
    }
");
        }

        void Traverse(JsonSchemaSource source, string rootName = default)
        {
            foreach (var child in source.Children())
            {
                Traverse(child);
            }

            switch (source.type)
            {
                case JsonSchemaType.Object:
                    {
                        var schema = source.Create(true);
                        if (schema is ObjectJsonSchema obj)
                        {
                            if (!string.IsNullOrEmpty(rootName))
                            {
                                obj.Title = rootName;
                            }
                            WriteObject(obj, rootName);
                        }
                        else if (schema is ExtensionJsonSchema ext)
                        {
                            // WriteObject(ext);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    break;

                case JsonSchemaType.EnumString:
                    WriteEnumString(source.Create(true) as EnumStringJsonSchema);
                    break;
            }
        }

        public static void Write(TextWriter w, JsonSchemaSource root, string rootName)
        {
            w.Write($@"// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.{rootName}
{{
");

            new FormatWriter(w, root.title).Traverse(root, rootName);

            // close namespace
            w.WriteLine("}");
        }
    }
}

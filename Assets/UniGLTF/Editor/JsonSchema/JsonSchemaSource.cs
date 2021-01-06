using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniGLTF.JsonSchema
{
    /// <summary>
    /// 型が確定する前にパースして値を集める
    /// </summary>
    public class JsonSchemaSource
    {
        public FileInfo FilePath;

        public string JsonPath;
        public static (string, string) SplitParent(string jsonPath)
        {
            var splitted = jsonPath.Split('.');
            return (String.Join(".", splitted.Take(splitted.Length - 1)), splitted[splitted.Length - 1]);
        }
        public void AddJsonPath(string jsonPath, JsonSchemaSource source)
        {
            var (parent, child) = SplitParent(jsonPath);
            var parentSchema = this.Get(parent);
            var materialExtensions = parentSchema;
            source.JsonPath = jsonPath;
            materialExtensions.AddProperty(child, source);
        }
        public JsonSchemaSource Get(string jsonPath)
        {
            if (JsonPath == jsonPath)
            {
                return this;
            }

            if (jsonPath.StartsWith(JsonPath))
            {
                foreach (var child in Children())
                {
                    var found = child.Get(jsonPath);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        public JsonSchemaType type;
        public string title;
        public string description;
        public string gltfDetail;

        public JsonSchemaSource baseSchema;

        #region Number
        public double? minimum;
        public bool exclusiveMinimum;
        public double? maximum;

        public double? multipleOf;
        #endregion

        #region String
        public string pattern;
        #endregion

        #region Object
        List<KeyValuePair<string, JsonSchemaSource>> m_properties;

        public JsonSchemaSource GetProperty(string name, bool remove = false)
        {
            if (m_properties is null)
            {
                return null;
            }
            for (int i = 0; i < m_properties.Count; ++i)
            {
                if (m_properties[i].Key == name)
                {
                    var found = m_properties[i].Value;
                    if (remove)
                    {
                        m_properties.RemoveAt(i);
                    }
                    return found;
                }
            }

            return null;
        }

        public void AddProperty(string name, JsonSchemaSource prop)
        {
            if (name is null)
            {
                throw new ArgumentNullException();
            }
            if (prop.type == JsonSchemaType.Unknown)
            {
                if (name == "extensions" || name == "extras")
                {
                    // return;
                    prop.type = JsonSchemaType.Object;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (m_properties is null)
            {
                m_properties = new List<KeyValuePair<string, JsonSchemaSource>>();
            }

            if (m_properties.Any(x => x.Key == name))
            {
                throw new ArgumentException($"{name}: is already exist");
            }
            m_properties.Add(new KeyValuePair<string, JsonSchemaSource>(name, prop));
        }

        public IEnumerable<KeyValuePair<string, JsonSchemaSource>> EnumerateProperties()
        {
            if (m_properties != null)
            {
                foreach (var kv in m_properties)
                {
                    yield return kv;
                }
            }
        }

        public string[] required;
        #endregion

        #region Dictionary
        public JsonSchemaSource additionalProperties;
        public int? minProperties;
        #endregion

        #region  Array
        public JsonSchemaSource items;
        public int? minItems;
        public int? maxItems;
        public bool? uniqueItems;
        #endregion

        #region Enum
        public KeyValuePair<string, int>[] enumValues;
        public string[] enumStringValues;
        #endregion

        public IEnumerable<JsonSchemaSource> Children()
        {
            if (m_properties != null)
            {
                foreach (var kv in m_properties)
                {
                    yield return kv.Value;
                }
            }
            else if (additionalProperties != null)
            {
                yield return additionalProperties;
            }
            else if (items != null)
            {
                if (type != JsonSchemaType.Array)
                {
                    throw new NotImplementedException();
                }
                yield return items;
            }
        }

        public IEnumerable<JsonSchemaSource> Traverse()
        {
            yield return this;

            if (m_properties != null)
            {
                foreach (var kv in m_properties)
                {
                    foreach (var x in kv.Value.Traverse())
                    {
                        yield return x;
                    }
                }
            }
            else if (additionalProperties != null)
            {
                foreach (var x in additionalProperties.Traverse())
                {
                    yield return x;
                }
            }
            else if (items != null)
            {
                foreach (var x in items.Traverse())
                {
                    yield return x;
                }
            }
        }

        public Schemas.JsonSchemaBase Create(bool useUpperCamelName, string rootName = default)
        {
            // if (baseSchema != null)
            // {
            //     baseSchema.MergeTo(this);
            // }

            if (baseSchema != null)
            {
                if (type == JsonSchemaType.Unknown)
                {
                    type = baseSchema.type;
                }
            }

            switch (type)
            {
                case JsonSchemaType.Object:
                    if (this.JsonPath.EndsWith(".extensions") || this.JsonPath.EndsWith(".extras"))
                    {
                        return new Schemas.ExtensionJsonSchema(this);
                    }

                    if ((m_properties != null && m_properties.Any()) || additionalProperties is null)
                    {
                        var obj = new Schemas.ObjectJsonSchema(this, useUpperCamelName);
                        if (!string.IsNullOrEmpty(rootName))
                        {
                            obj.Title = rootName;
                        }
                        return obj;
                    }
                    else
                    {
                        return new Schemas.DictionaryJsonSchema(this, useUpperCamelName);
                    }
                case JsonSchemaType.Array:
                    return new Schemas.ArrayJsonSchema(this, useUpperCamelName);
                case JsonSchemaType.Boolean:
                    return new Schemas.BoolJsonSchema(this);
                case JsonSchemaType.String:
                    return new Schemas.StringJsonSchema(this);
                case JsonSchemaType.Number:
                    return new Schemas.NumberJsonSchema(this);
                case JsonSchemaType.Integer:
                    return new Schemas.IntegerJsonSchema(this);
                case JsonSchemaType.Enum:
                    return new Schemas.EnumJsonSchema(this);
                case JsonSchemaType.EnumString:
                    return new Schemas.EnumStringJsonSchema(this);
                default:
                    return null;
            }
        }

        public void Dump(string indent = "")
        {
            Console.WriteLine($"{indent}{JsonPath}: {type}");

            foreach (var x in Children())
            {
                x.Dump(indent + "  ");
            }
        }
    }
}

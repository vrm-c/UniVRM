using System;
using System.Collections.Generic;
using System.Text;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// Extension または Extras に使う
    /// </summary>
    public abstract class glTFExtension
    {
        // NO BOM
        static Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>
        /// for unit test
        /// 
        /// parse exported value
        /// </summary>
        public virtual glTFExtensionImport Deserialize()
        {
            throw new NotImplementedException();
        }

        public virtual void Serialize(JsonFormatter f)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Export(Serialize)用
    /// 
    /// 実体は、
    /// 
    /// Dictionary<string, ArraySegment<byte>>
    /// 
    /// key: json object のキー名
    /// value: utf8 エンコーディング済みのJSON
    /// 
    /// </summary>
    public class glTFExtensionExport : glTFExtension
    {
        readonly Dictionary<string, ArraySegment<byte>> m_serialized;

        public glTFExtensionExport()
        {
            m_serialized = new Dictionary<string, ArraySegment<byte>>();
        }

        public override string ToString()
        {
            var f = new JsonFormatter();
            Serialize(f);
            var bytes = f.GetStoreBytes();
            return "export: " + Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public glTFExtensionExport Add(string key, ArraySegment<byte> raw)
        {
            m_serialized[key] = raw;
            return this;
        }

        public override void Serialize(JsonFormatter f)
        {
            f.BeginMap();
            if (m_serialized != null)
            {
                foreach (var kv in m_serialized)
                {
                    f.Key(kv.Key);
                    f.Raw(kv.Value);
                }
            }
            f.EndMap();
        }

        public static glTFExtensionExport GetOrCreate(ref glTFExtension extension)
        {
            if (extension is glTFExtensionExport exported)
            {
                // get
                return exported;
            }

            if (extension != null)
            {
                // glTFExtensionImport ?
                throw new NotImplementedException();
            }

            // or create
            exported = new glTFExtensionExport();
            extension = exported;
            return exported;
        }

        /// <summary>
        /// for unit test
        /// 
        /// parse exported value
        /// </summary>
        public override glTFExtensionImport Deserialize()
        {
            var f = new JsonFormatter();
            f.GenSerialize(this);
            var b = f.GetStoreBytes();
            var json = Encoding.UTF8.GetString(b.Array, b.Offset, b.Count);
            return new glTFExtensionImport(json.ParseAsJson());
        }
    }

    /// <summary>
    /// Import(Deserialize)用
    /// 
    /// パース済みの JSONの部分 を保持する
    /// 
    /// JsonNode がJsonの部分を参照できる。
    /// 
    /// </summary>
    public class glTFExtensionImport : glTFExtension
    {
        readonly JsonNode m_json;
        public glTFExtensionImport(JsonNode json)
        {
            m_json = json;
        }

        public override string ToString()
        {
            var bytes = m_json.Value.Bytes;
            return "import: " + Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public IEnumerable<KeyValuePair<JsonNode, JsonNode>> ObjectItems()
        {
            if (m_json.Value.ValueType == ValueNodeType.Object)
            {
                foreach (var kv in m_json.ObjectItems())
                {
                    yield return kv;
                }
            }
        }

        public override void Serialize(JsonFormatter f)
        {
            f.Raw(m_json.Value.Bytes);
        }
    }

    public static class GltfExtensionFormatterExtensions
    {
        /// <summary>
        /// Json化
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        public static void GenSerialize(this JsonFormatter f, glTFExtension v)
        {
            if (v != null)
            {
                v.Serialize(f);
                return;
            }

            throw new NotImplementedException();
        }
    }
}

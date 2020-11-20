using System;
using System.Collections.Generic;
using System.Text;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// Extension または Extras に使う
    /// </summary>
    public class glTFExtension
    {
        // NO BOM
        static Encoding Utf8 = new UTF8Encoding(false);

        #region for Export
        public readonly Dictionary<string, ArraySegment<byte>> Serialized;
        public glTFExtension()
        {
            Serialized = new Dictionary<string, ArraySegment<byte>>();
        }
        public static glTFExtension Create(string key, string serialized)
        {
            var e = new glTFExtension();
            e.Serialized.Add(key, new ArraySegment<byte>(Utf8.GetBytes(serialized)));
            return e;
        }
        #endregion

        #region for Import
        readonly ListTreeNode<JsonValue> m_json;
        public glTFExtension(ListTreeNode<JsonValue> json)
        {
            m_json = json;
        }

        public IEnumerable<KeyValuePair<ListTreeNode<JsonValue>, ListTreeNode<JsonValue>>> ObjectItems()
        {
            if (m_json.Value.ValueType == ValueNodeType.Object)
            {
                foreach (var kv in m_json.ObjectItems())
                {
                    yield return kv;
                }
            }
        }
        #endregion
    }

    public static class GltfExtensionFormatterExtensions
    {
        public static void GenSerialize(this JsonFormatter f, glTFExtension v)
        {
            //CommaCheck();
            f.BeginMap();
            foreach (var kv in v.Serialized)
            {
                f.Key(kv.Key);
                f.Raw(kv.Value);
            }
            f.EndMap();
        }
    }
}

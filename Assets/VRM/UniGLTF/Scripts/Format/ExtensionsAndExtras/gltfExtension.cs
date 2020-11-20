using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// Extension または Extras に使う
    /// </summary>
    public class glTFExtension
    {
        #region for Export
        public readonly Dictionary<string, string> Serialized;
        public glTFExtension()
        {
            Serialized = new Dictionary<string, string>();
        }
        public static glTFExtension Create(string key, string serialized)
        {
            var e = new glTFExtension();
            e.Serialized.Add(key, serialized);
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
}

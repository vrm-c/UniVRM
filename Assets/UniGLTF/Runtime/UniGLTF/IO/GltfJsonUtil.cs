using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UniJSON;
using UnityEngine;

namespace UniGLTF
{
    public static class GltfJsonUtil
    {
        const string EXTENSION_USED_KEY = "extensionUsed";

        /// <summary>
        /// JsonPath を 再帰的に列挙する
        /// object[] の中身は int(array index) or string(object key)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<object[]> TraverseJsonPath(JsonNode node, List<object> path)
        {
            if (path == null)
            {
                path = new List<object>();
            }
            yield return path.ToArray();

            if (node.IsArray())
            {
                int i = 0;
                foreach (var child in node.ArrayItems())
                {
                    path.Add(i);
                    foreach (var x in TraverseJsonPath(child, path))
                    {
                        yield return x;
                    }
                    path.RemoveAt(path.Count - 1);
                    ++i;
                }
            }
            else if (node.IsMap())
            {
                foreach (var kv in node.ObjectItems())
                {
                    path.Add(kv.Key.GetString());
                    foreach (var x in TraverseJsonPath(kv.Value, path))
                    {
                        yield return x;
                    }
                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        static string DoubleQuote(string src)
        {
            return $"\"{src}\"";
        }

        /// <summary>
        /// jsonPath が
        /// 
        /// [..., "extensions", "EXTENSION_NAME"]
        /// 
        /// で有る場合に EXTENSION_NAME を返す。
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <param name="extensionName"></param>
        /// <returns></returns>
        static bool TryGetExtensionName(object[] path, out string extensionName)
        {
            if (path.Length >= 2)
            {
                if (path[path.Length - 2] is string x)
                {
                    if (x == "extensions")
                    {
                        if (path[path.Length - 1] is string y)
                        {
                            extensionName = y;
                            return true;
                        }
                        else
                        {
                            // ありえない。はず
                            var join = string.Join(", ", path);
                            Debug.LogWarning($"invalid json path: {join}");
                        }
                    }
                }
            }

            extensionName = default;
            return false;
        }

        /// <summary>
        /// https://github.com/KhronosGroup/glTF/blob/main/specification/2.0/schema/glTF.schema.json
        /// 
        /// extensionUsed の更新を各拡張自身にやらせるのは無駄だし、手動でコントロールするのも間違いの元である。
        /// 完成品の JSON から後付けで作ることにした。
        /// 
        /// * Exporter しか使わない処理なので、GC, 処理速度は気にしてない
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FindUsedExtensionsAndUpdateJson(string src)
        {
            var parsed = src.ParseAsJson();

            // unique な extension 名を収集
            var used = new HashSet<string>();
            foreach (var path in TraverseJsonPath(parsed, null))
            {
                if (TryGetExtensionName(path, out string extensionName))
                {
                    used.Add(extensionName);
                }
            }

            if (used.Count == 0)
            {
                // 無いとき
                if (parsed.ContainsKey(EXTENSION_USED_KEY))
                {
                    // 消す
                    throw new NotImplementedException();
                }
                else
                {
                    // OK
                }
            }
            else
            {
                if (parsed.ContainsKey(EXTENSION_USED_KEY))
                {
                    var values = "[" + string.Join(",", used.Select(x => DoubleQuote(x))) + "]";
                    // Debug.Log($"replace: {values}");
                    // replace
                    var node = parsed[EXTENSION_USED_KEY];

                    var segment = node.Value.Segment.Bytes;
                    // arraySegment.Array に JSON 全体
                    // arraySegment.Offset~Count に extensionUsed 値が入っている

                    // BOM 無し encoder
                    var utf8 = new UTF8Encoding(false);
                    var bytes = utf8.GetBytes(values);

                    using (var w = new MemoryStream())
                    {
                        // before
                        w.Write(segment.Array, 0, segment.Offset);
                        // replace
                        w.Write(bytes, 0, bytes.Length);
                        // after
                        var after = segment.Offset + segment.Count;
                        w.Write(segment.Array, after, segment.Array.Length - after);

                        src = utf8.GetString(w.ToArray());
                    }
                }
                else
                {
                    var values = "\"" + EXTENSION_USED_KEY + "\":[" + string.Join(",", used.Select(x => DoubleQuote(x))) + "]";
                    // Debug.Log($"add: {values}");
                    // add
                    var close = src.LastIndexOf("}");
                    src = src.Substring(0, close) + "," + values + "}";
                }
            }

            return src;
        }
    }
}

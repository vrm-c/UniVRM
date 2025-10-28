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
        public const string EXTENSION_USED_KEY = "extensionsUsed";

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
                            UniGLTFLogger.Warning($"invalid json path: {join}");
                        }
                    }
                }
            }

            extensionName = default;
            return false;
        }

        static void CopyJson(IReadOnlyList<string> extensionsUsed, JsonFormatter dst, JsonNode src, int level)
        {
            if (src.IsArray())
            {
                dst.BeginList();
                foreach (var v in src.ArrayItems())
                {
                    CopyJson(extensionsUsed, dst, v, level + 1);
                }
                dst.EndList();
            }
            else if (src.IsMap())
            {
                if (level == 0)
                {
                    // 最上層だけ extensionsUsed の処理をする
                    var done = false;
                    dst.BeginMap();
                    foreach (var kv in src.ObjectItems())
                    {
                        var key = kv.Key.GetString();
                        if (key == EXTENSION_USED_KEY)
                        {
                            if (extensionsUsed.Count == 0)
                            {
                                // skip
                            }
                            else
                            {
                                dst.Key(key);
                                // replace
                                dst.BeginList();
                                foreach (var ex in extensionsUsed)
                                {
                                    dst.Value(ex);
                                }
                                dst.EndList();
                                // 処理済
                            }
                            done = true;
                        }
                        else
                        {
                            dst.Key(key);
                            CopyJson(extensionsUsed, dst, kv.Value, level + 1);
                        }
                    }
                    if (!done && level == 0 && extensionsUsed.Count > 0)
                    {
                        // add
                        dst.Key(EXTENSION_USED_KEY);
                        dst.BeginList();
                        foreach (var ex in extensionsUsed)
                        {
                            dst.Value(ex);
                        }
                        dst.EndList();
                    }
                    dst.EndMap();
                }
                else
                {
                    dst.BeginMap();
                    foreach (var kv in src.ObjectItems())
                    {
                        dst.Key(kv.Key.GetUtf8String());
                        CopyJson(extensionsUsed, dst, kv.Value, level + 1);
                    }
                    dst.EndMap();
                }
            }
            else
            {
                // leaf
                dst.Value(src);
            }
        }

        /// <summary>
        /// https://github.com/KhronosGroup/glTF/blob/main/specification/2.0/schema/glTF.schema.json
        /// 
        /// extensionsUsed の更新を各拡張自身にやらせるのは無駄だし、手動でコントロールするのも間違いの元である。
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

            // json 加工
            var f = new JsonFormatter();
            CopyJson(used.ToArray(), f, parsed, 0);

            // bom無しutf8
            var bytes = f.GetStoreBytes();
            var utf8 = new UTF8Encoding(false);
            return utf8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }
    }
}

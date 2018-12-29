using NUnit.Framework;
using System;
using System.Linq;


namespace UniJSON
{
    public class JsonParserTest
    {
        [Test]
        public void Tests()
        {
            {
                var result = JsonParser.Parse("1");
                Assert.AreEqual(1, result.GetInt32());
            }

            {
                var result = JsonParser.Parse("{ \"a\": { \"b\": 1 }}");
                Assert.True(result.ContainsKey("a"));
            }
        }

        [Test]
        public void NullTest()
        {
            {
                var node = JsonParser.Parse("null");
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(4, node.Value.Bytes.Count);
                Assert.True(node.IsNull());
            }
        }

        [Test]
        public void BooleanTest()
        {
            {
                var node = JsonParser.Parse("true");
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(4, node.Value.Bytes.Count);
                Assert.True(node.IsBoolean());
                Assert.AreEqual(true, node.GetBoolean());
                Assert.Catch(typeof(FormatException), () => node.GetDouble());
            }
            {
                var node = JsonParser.Parse(" false ");
                Assert.AreEqual(1, node.Value.Bytes.Offset);
                Assert.AreEqual(5, node.Value.Bytes.Count);
                Assert.True(node.IsBoolean());
                Assert.AreEqual(false, node.GetBoolean());
            }
        }

        [Test]
        public void NumberTest()
        {
            {
                var node = JsonParser.Parse("1");
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(1, node.Value.Bytes.Count);
                Assert.True(node.IsInteger());
                Assert.AreEqual(1, (int)node.GetDouble());
                Assert.Catch(typeof(DeserializationException), () => node.GetBoolean());
            }
            {
                var node = JsonParser.Parse(" 22 ");
                Assert.AreEqual(1, node.Value.Bytes.Offset);
                Assert.AreEqual(2, node.Value.Bytes.Count);
                Assert.True(node.IsInteger());
                Assert.AreEqual(22, (int)node.GetDouble());
            }
            {
                var node = JsonParser.Parse(" 3.3 ");
                Assert.AreEqual(1, node.Value.Bytes.Offset);
                Assert.AreEqual(3, node.Value.Bytes.Count);
                Assert.True(node.IsFloat());
                Assert.AreEqual(3, (int)node.GetDouble());
                Assert.AreEqual(3.3f, (float)node.GetDouble());
            }
            {
                var node = JsonParser.Parse(" -4.44444444444444444444 ");
                Assert.True(node.IsFloat());
                Assert.AreEqual(-4, (int)node.GetDouble());
                Assert.AreEqual(-4.44444444444444444444, node.GetDouble());
            }
            {
                var node = JsonParser.Parse(" -5e-4 ");
                Assert.True(node.IsFloat());
                Assert.AreEqual(0, (int)node.GetDouble());
                Assert.AreEqual(-5e-4, node.GetDouble());
            }
            {
                var node = JsonParser.Parse("NaN");
                Assert.True(node.IsFloat());
                Assert.AreEqual(Double.NaN, node.GetDouble());
            }
            {
                var node = JsonParser.Parse("Infinity");
                Assert.True(node.IsFloat());
                Assert.AreEqual(Double.PositiveInfinity, node.GetDouble());
            }
            {
                var node = JsonParser.Parse("-Infinity");
                Assert.True(node.IsFloat());
                Assert.AreEqual(Double.NegativeInfinity, node.GetDouble());
            }
        }

        [Test]
        public void StringTest()
        {
            {
                var value = "hoge";
                var quoted = "\"hoge\"";
                Assert.AreEqual(quoted, JsonString.Quote(value));
                var node = JsonParser.Parse(quoted);
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(quoted.Length, node.Value.Bytes.Count);
                Assert.True(node.IsString());
                Assert.AreEqual("hoge", node.GetString());
            }

            {
                var value = "fuga\n  hoge";
                var quoted = "\"fuga\\n  hoge\"";
                Assert.AreEqual(quoted, JsonString.Quote(value));
                var node = JsonParser.Parse(quoted);
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(quoted.Length, node.Value.Bytes.Count);
                Assert.True(node.IsString());
                Assert.AreEqual(value, node.GetString());
            }
        }

        [Test]
        public void StringEscapeTest()
        {
            {
                var value = "\"";
                var escaped = "\\\"";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\\";
                var escaped = "\\\\";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "/";
                var escaped = "\\/";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\b";
                var escaped = "\\b";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\f";
                var escaped = "\\f";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\n";
                var escaped = "\\n";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\r";
                var escaped = "\\r";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
            {
                var value = "\t";
                var escaped = "\\t";
                Assert.AreEqual(escaped, JsonString.Escape(value));
                Assert.AreEqual(value, JsonString.Unescape(escaped));
            }
        }

        [Test]
        public void ObjectTest()
        {
            {
                var json = "{}";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);

                Assert.AreEqual(2, node.Value.Bytes.Count);

                Assert.True(node.IsMap());
                Assert.AreEqual(0, node.ObjectItems().Count());
            }

            {
                var json = "{\"key\":\"value\"}";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(json.Length, node.Value.Bytes.Count);
                Assert.True(node.IsMap());

                var it = node.ObjectItems().GetEnumerator();

                Assert.IsTrue(it.MoveNext());
                Assert.AreEqual("key", it.Current.Key.GetString());
                Assert.AreEqual("value", it.Current.Value.GetString());

                Assert.IsFalse(it.MoveNext());
            }

            {
                var json = "{\"key\":\"value\"}";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);
                Assert.AreEqual(json.Length, node.Value.Bytes.Count);
                Assert.True(node.IsMap());

                var it = node.ObjectItems().GetEnumerator();

                Assert.IsTrue(it.MoveNext());
                Assert.AreEqual("key", it.Current.Key.GetString());
                Assert.AreEqual("value", it.Current.Value.GetString());

                Assert.IsFalse(it.MoveNext());
            }
        }

        [Test]
        public void NestedObjectTest()
        {
            {
                var json = "{\"key\":{ \"nestedKey\": \"nestedValue\" }, \"key2\": { \"nestedKey2\": \"nestedValue2\" } }";
                var node = JsonParser.Parse(json);
                Assert.True(node.IsMap());

                {
                    var it = node.ObjectItems().GetEnumerator();

                    Assert.IsTrue(it.MoveNext());
                    Assert.AreEqual("key", it.Current.Key.GetString());
                    Assert.True(it.Current.Value.IsMap());

                    Assert.IsTrue(it.MoveNext());
                    Assert.AreEqual("key2", it.Current.Key.GetString());
                    Assert.True(it.Current.Value.IsMap());

                    Assert.IsFalse(it.MoveNext());
                }

                var nested = node["key2"];

                {
                    var it = nested.ObjectItems().GetEnumerator();

                    Assert.IsTrue(it.MoveNext());
                    Assert.AreEqual("nestedKey2", it.Current.Key.GetString());
                    Assert.AreEqual("nestedValue2", it.Current.Value.GetString());

                    Assert.IsFalse(it.MoveNext());
                }

                Assert.AreEqual("nestedValue2", node["key2"]["nestedKey2"].GetString());
            }
        }

        [Test]
        public void ArrayTest()
        {
            {
                var json = "[]";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);

                //Assert.Catch(() => { var result = node.Value.Bytes.Count; }, "raise exception");
                Assert.AreEqual(2, node.Value.Bytes.Count);

                Assert.True(node.IsArray());

                Assert.AreEqual("[\n]", node.ToString("  "));
            }

            {
                var json = "[1,2,3]";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);

                //Assert.Catch(() => { var result = node.Value.Bytes.Count; }, "raise exception");

                Assert.True(node.IsArray());
                Assert.AreEqual(1, node[0].GetDouble());
                Assert.AreEqual(2, node[1].GetDouble());
                Assert.AreEqual(3, node[2].GetDouble());

                Assert.AreEqual("[\n  1,\n  2,\n  3\n]", node.ToString("  "));
            }

            {
                var json = "[\"key\",1]";
                var node = JsonParser.Parse(json);
                Assert.AreEqual(0, node.Value.Bytes.Offset);

                //Assert.Catch(() => { var result = node.Value.Bytes.Count; }, "raise exception");
                Assert.AreEqual(json.Length, node.Value.Bytes.Count);

                Assert.True(node.IsArray());

                var it = node.ArrayItems().GetEnumerator();

                Assert.IsTrue(it.MoveNext());
                Assert.AreEqual("key", it.Current.GetString());

                Assert.IsTrue(it.MoveNext());
                Assert.AreEqual(1, it.Current.GetDouble());

                Assert.IsFalse(it.MoveNext());

                Assert.AreEqual("key", node[0].GetString());
                Assert.AreEqual(1, node[1].GetDouble());

                Assert.AreEqual("[\n  \"key\",\n  1\n]", node.ToString("  "));
            }
        }

        [Test]
        public void ParseTest()
        {
            var json = "{";
            Assert.Catch(typeof(ParserException), () => JsonParser.Parse(json));
        }

        [Test]
        public void Utf8Test()
        {
            JsonParser.Parse("\"５\"");
        }

        [Test]
        public void TimeTest()
        {
            var f = new JsonFormatter();
            f.Value(new DateTimeOffset());

            Assert.AreEqual("\"0001-01-01T00:00:00Z\"", new Utf8String(f.GetStoreBytes()).ToString());
        }
    }
}

#pragma warning disable 0649
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UniGLTF;

namespace UniJSON
{
    public class JsonSerializerTests
    {
        struct Point
        {
            public float X;
            public float Y;

            public float[] Vector;

            public override string ToString()
            {
                return string.Format("{{X={0}, Y={1}, {2}}}", X, Y, Vector);
            }
        }

        enum HogeFuga
        {
            Hoge,
            Fuga,
        }

        struct EnumTest
        {
            public HogeFuga EnumDefault;

            [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt)]
            public HogeFuga EnumAsInt;

            [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
            public HogeFuga EnumAsString;

            [JsonSchema(EnumSerializationType = EnumSerializationType.AsLowerString)]
            public HogeFuga EnumAsLowerString;
        }

        #region Serializer
        static void SerializeValue(float value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Value(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }
        static void SerializeValue(double value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Value(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }
        static void SerializeValue(bool value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Value(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }
        static void SerializeValue(string value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Value(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }
        static void SerializeList<T>(IEnumerable<T> values, string json, Action<JsonFormatter, T> seri)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.BeginList();
            foreach (var value in values)
            {
                seri(f, value);
            }
            f.EndList();

            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }

        [Test]
        public void JsonSerializerTest()
        {
            SerializeValue(1, "1");
            SerializeValue(1.1f, "1.1");
            SerializeValue(1.2, "1.2");
            SerializeValue(Double.NaN, "NaN");
            SerializeValue(Double.PositiveInfinity, "Infinity");
            SerializeValue(Double.NegativeInfinity, "-Infinity");
            SerializeValue(true, "true");
            SerializeValue(false, "false");
            SerializeValue("ascii", "\"ascii\"");

            SerializeList(new[] { 1 }, "[1]", (f, x) => f.Value(x));
            SerializeList(new[] { 1.1f }, "[1.1]", (f, x) => f.Value(x));
            SerializeList(new[] { 1.2 }, "[1.2]", (f, x) => f.Value(x));
            SerializeList(new[] { true, false }, "[true,false]", (f, x) => f.Value(x));
            SerializeList(new[] { "ascii" }, "[\"ascii\"]", (f, x) => f.Value(x));
            SerializeList(new List<int> { 1 }, "[1]", (f, x) => f.Value(x));
            //SerializeValue(new object[] { null, 1, "a" }, "[null,1,\"a\"]");

            // SerializeValue(new Dictionary<string, object> { }, "{}");
            // SerializeValue(new Dictionary<string, object> { { "a", 1 } }, "{\"a\":1}");
            // SerializeValue(new Dictionary<string, object> { { "a",
            //         new Dictionary<string, object>{
            //         } } }, "{\"a\":{}}");

            // SerializeValue(new Point { X = 1 }, "{\"X\":1,\"Y\":0}");

            // SerializeValue(HogeFuga.Fuga, "1");

            // SerializeValue(new EnumTest(), "{\"EnumDefault\":0,\"EnumAsInt\":0,\"EnumAsString\":\"Hoge\",\"EnumAsLowerString\":\"hoge\"}");

            // SerializeValue((object)new Point { X = 1 }, "{\"X\":1,\"Y\":0}");
        }

        [Test]
        public void KeyValue()
        {
            var p = new Point
            {
                X = 1,
                Vector = new float[] { 1, 2, 3 }
            };

            var f = new JsonFormatter();
            f.BeginMap();
            f.Key(nameof(p.Vector));
            f.BeginList();
            f.Value(p.Vector[0]);
            f.Value(p.Vector[1]);
            f.Value(p.Vector[2]);
            f.EndList();
            f.EndMap();

            var json = JsonParser.Parse(new Utf8String(f.GetStoreBytes()));

            Assert.AreEqual(1, json.GetObjectCount());
            Assert.AreEqual(1, json["Vector"][0].GetInt32());
        }

        #endregion

        #region Deserialize
        // static void DeserializeValue<T>(T value, string json)
        // {
        //     var parsed = JsonParser.Parse(json);

        //     var t = default(T);
        //     parsed.Deserialize(ref t);

        //     Assert.AreEqual(value, t);
        // }

        // [Test]
        // public void JsonDeserializerTest()
        // {
        //     DeserializeValue(1, "1");
        //     DeserializeValue(1.1f, "1.1");
        //     DeserializeValue(1.2, "1.2");
        //     DeserializeValue(true, "true");
        //     DeserializeValue(false, "false");
        //     DeserializeValue("ascii", "\"ascii\"");

        //     DeserializeValue(new[] { 1 }, "[1]");
        //     DeserializeValue(new[] { 1.1f }, "[1.1]");
        //     DeserializeValue(new[] { 1.2 }, "[1.2]");
        //     DeserializeValue(new[] { true, false }, "[true,false]");
        //     DeserializeValue(new[] { "ascii" }, "[\"ascii\"]");
        //     DeserializeValue(new List<int> { 1 }, "[1]");
        //     //DeserializeValue(new object[] { null, 1, "a" }, "[null,1,\"a\"]");

        //     DeserializeValue(new Point { X = 1 }, "{\"X\":1,\"Y\":0}");

        //     DeserializeValue(HogeFuga.Fuga, "1");

        //     DeserializeValue(new EnumTest(), "{\"EnumDefault\":0,\"EnumAsInt\":0,\"EnumAsString\":\"Hoge\",\"EnumAsLowerString\":\"hoge\"}");
        // }

        class DictionaryValue : IEquatable<DictionaryValue>
        {
            public Dictionary<string, object> Dict = new Dictionary<string, object>();

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var rhs = obj as DictionaryValue;
                if (rhs != null)
                {
                    return Equals(rhs);
                }
                else
                {
                    return base.Equals(obj);
                }
            }

            public bool Equals(DictionaryValue other)
            {
                if (Dict == null && other.Dict == null)
                {
                    return true;
                }
                else if (Dict == null || other.Dict == null)
                {
                    return false;
                }
                else
                {
                    return Dict.OrderBy(x => x.Key).SequenceEqual(other.Dict.OrderBy(x => x.Key));
                }
            }
        }

        // [Test]
        // public void JsonDictionaryDeserializerTest()
        // { 
        //     DeserializeValue(new Dictionary<string, object> { }, "{}");
        //     DeserializeValue(new Dictionary<string, object> { { "a", 1 } }, "{\"a\":1}");
        //     DeserializeValue(new Dictionary<string, object> { { "a",
        //             new Dictionary<string, object>{
        //             } } }, "{\"a\":{}}");

        //     // fix dictionary member deserialization
        //     DeserializeValue(new DictionaryValue(), "{\"Dict\": {}}");
        // }
        #endregion
    }
}

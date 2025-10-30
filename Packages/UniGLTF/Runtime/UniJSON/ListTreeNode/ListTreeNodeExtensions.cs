using System.Collections.Generic;


namespace UniJSON
{
    public static class ListTreeNodeExtensions
    {
        #region IValue
        public static bool IsNull(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Null;
        }

        public static bool IsBoolean(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Boolean;
        }

        public static bool IsString(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.String;
        }

        public static bool IsInteger(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Integer;
        }

        public static bool IsFloat(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Number
                   || self.Value.ValueType == ValueNodeType.NaN
                   || self.Value.ValueType == ValueNodeType.Infinity
                   || self.Value.ValueType == ValueNodeType.MinusInfinity;
        }

        public static bool IsArray(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Array;
        }

        public static bool IsMap(this JsonNode self)
        {
            return self.Value.ValueType == ValueNodeType.Object;
        }

        public static bool GetBoolean(this JsonNode self) { return self.Value.GetBoolean(); }
        public static string GetString(this JsonNode self) { return self.Value.GetString(); }
        public static Utf8String GetUtf8String(this JsonNode self) { return self.Value.GetUtf8String(); }
        public static sbyte GetSByte(this JsonNode self) { return self.Value.GetSByte(); }
        public static short GetInt16(this JsonNode self) { return self.Value.GetInt16(); }
        public static int GetInt32(this JsonNode self) { return self.Value.GetInt32(); }
        public static long GetInt64(this JsonNode self) { return self.Value.GetInt64(); }
        public static byte GetByte(this JsonNode self) { return self.Value.GetByte(); }
        public static ushort GetUInt16(this JsonNode self) { return self.Value.GetUInt16(); }
        public static uint GetUInt32(this JsonNode self) { return self.Value.GetUInt32(); }
        public static ulong GetUInt64(this JsonNode self) { return self.Value.GetUInt64(); }
        public static float GetSingle(this JsonNode self) { return self.Value.GetSingle(); }
        public static double GetDouble(this JsonNode self) { return self.Value.GetDouble(); }
        #endregion

        public static IEnumerable<JsonNode> Traverse(this JsonNode self)
        {
            yield return self;
            if (self.IsArray())
            {
                foreach (var x in self.ArrayItems())
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap())
            {
                foreach (var kv in self.ObjectItems())
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }
    }
}

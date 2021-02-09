using System;


namespace UniJSON
{
    public static class StringExtensions
    {
        public static ListTreeNode<JsonValue> ParseAsJson(this string json)
        {
            return JsonParser.Parse(json);
        }
        public static ListTreeNode<JsonValue> ParseAsJson(this Utf8String json)
        {
            return JsonParser.Parse(json);
        }
        public static ListTreeNode<JsonValue> ParseAsJson(this byte[] bytes)
        {
            return JsonParser.Parse(new Utf8String(bytes));
        }
        public static ListTreeNode<JsonValue> ParseAsJson(this ArraySegment<byte> bytes)
        {
            return JsonParser.Parse(new Utf8String(bytes));
        }
    }
}

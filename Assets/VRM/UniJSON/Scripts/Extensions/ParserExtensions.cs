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

        public static ListTreeNode<MsgPackValue> ParseAsMsgPack(this byte[] bytes)
        {
            return MsgPackParser.Parse(bytes);
        }
        public static ListTreeNode<MsgPackValue> ParseAsMsgPack(this ArraySegment<byte> bytes)
        {
            return MsgPackParser.Parse(bytes);
        }

        public static ListTreeNode<TomlValue> ParseAsToml(this string toml)
        {
            return TomlParser.Parse(toml);
        }
        public static ListTreeNode<TomlValue> ParseAsToml(this Utf8String toml)
        {
            return TomlParser.Parse(toml);
        }
        public static ListTreeNode<TomlValue> ParseAsToml(this byte[] bytes)
        {
            return TomlParser.Parse(new Utf8String(bytes));
        }
        public static ListTreeNode<TomlValue> ParseAsToml(this ArraySegment<byte> bytes)
        {
            return TomlParser.Parse(new Utf8String(bytes));
        }

    }
}

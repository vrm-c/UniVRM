using System;


namespace UniJSON
{
    public static class StringExtensions
    {
        public static JsonNode ParseAsJson(this string json)
        {
            return JsonParser.Parse(json);
        }
        public static JsonNode ParseAsJson(this Utf8String json)
        {
            return JsonParser.Parse(json);
        }
        public static JsonNode ParseAsJson(this byte[] bytes)
        {
            return JsonParser.Parse(new Utf8String(bytes));
        }
        public static JsonNode ParseAsJson(this ArraySegment<byte> bytes)
        {
            return JsonParser.Parse(new Utf8String(bytes));
        }
    }
}

using System;

namespace UniJSON
{
    public enum JsonDiffType
    {
        KeyAdded,
        KeyRemoved,
        ValueChanged,
    }

    public struct JsonDiff
    {
        public JsonPointer Path;
        public JsonDiffType DiffType;
        public string Msg;

        public static JsonDiff Create(JsonNode node, JsonDiffType diffType, string msg)
        {
            return new JsonDiff
            {
                Path = JsonPointer.Create(node),
                DiffType = diffType,
                Msg = msg,
            };
        }

        public override string ToString()
        {
            switch (DiffType)
            {
                case JsonDiffType.KeyAdded:
                    return string.Format("+ {0}: {1}", Path, Msg);
                case JsonDiffType.KeyRemoved:
                    return string.Format("- {0}: {1}", Path, Msg);
                case JsonDiffType.ValueChanged:
                    return string.Format("= {0}: {1}", Path, Msg);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

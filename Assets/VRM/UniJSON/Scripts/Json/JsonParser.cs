using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class JsonParseResult
    {
        public List<JsonValue> Values = new List<JsonValue>();
    }

    public static class JsonParser
    {
        static ValueNodeType GetValueType(Utf8String segment)
        {
            switch (Char.ToLower((char)segment[0]))
            {
                case '{': return ValueNodeType.Object;
                case '[': return ValueNodeType.Array;
                case '"': return ValueNodeType.String;
                case 't': return ValueNodeType.Boolean;
                case 'f': return ValueNodeType.Boolean;
                case 'n':
                    if (segment.ByteLength >= 2 && Char.ToLower((char) segment[1]) == 'a')
                    {
                        return ValueNodeType.NaN;
                    }

                    return ValueNodeType.Null;

                case 'i':
                    return ValueNodeType.Infinity;

                case '-':
                    if (segment.ByteLength >= 2 && Char.ToLower((char) segment[1]) == 'i')
                    {
                        return ValueNodeType.MinusInfinity;
                    }
                    goto case '0';// fall through
                case '0': // fall through
                case '1': // fall through
                case '2': // fall through
                case '3': // fall through
                case '4': // fall through
                case '5': // fall through
                case '6': // fall through
                case '7': // fall through
                case '8': // fall through
                case '9': // fall through
                    {
                        if (segment.IsInt)
                        {
                            return ValueNodeType.Integer;
                        }
                        else
                        {
                            return ValueNodeType.Number;
                        }
                    }

                default:
                    throw new ParserException(segment + " is not valid json start");
            }
        }

        /// <summary>
        /// Expected null, boolean, integer, number
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="valueType"></param>
        /// <param name="parentIndex"></param>
        /// <returns></returns>
        static JsonValue ParsePrimitive(Utf8String segment, ValueNodeType valueType, int parentIndex)
        {
            int i = 1;
            for (; i < segment.ByteLength; ++i)
            {
                if (Char.IsWhiteSpace((char)segment[i])
                    || segment[i] == '}'
                    || segment[i] == ']'
                    || segment[i] == ','
                    || segment[i] == ':'
                    )
                {
                    break;
                }
            }
            return new JsonValue(segment.Subbytes(0, i), valueType, parentIndex);
        }

        static JsonValue ParseString(Utf8String segment, int parentIndex)
        {
            int pos;
            if (segment.TrySearchAscii((Byte)'"', 1, out pos))
            {
                return new JsonValue(segment.Subbytes(0, pos + 1), ValueNodeType.String, parentIndex);
            }
            else
            {
                throw new ParserException("no close string: " + segment);
            }
        }

        static Utf8String ParseArray(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            var closeChar = ']';
            bool isFirst = true;
            var current = segment.Subbytes(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearchByte(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new ParserException("no white space expected");
                    }
                    current = current.Subbytes(nextToken);
                }

                {
                    if (current[0] == closeChar)
                    {
                        // end
                        break;
                    }
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    // search ',' or closeChar
                    int keyPos;
                    if (!current.TrySearchByte(x => x == ',', out keyPos))
                    {
                        throw new ParserException("',' expected");
                    }
                    current = current.Subbytes(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearchByte(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new ParserException("not whitespace expected");
                    }
                    current = current.Subbytes(nextToken);
                }

                // value
                var value = Parse(current, values, parentIndex);
                current = current.Subbytes(value.Segment.ByteLength);
            }

            return current;
        }

        static Utf8String ParseObject(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            var closeChar = '}';
            bool isFirst = true;
            var current = segment.Subbytes(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearchByte(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new ParserException("no white space expected");
                    }
                    current = current.Subbytes(nextToken);
                }

                {
                    if (current[0] == closeChar)
                    {
                        break;
                    }
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    // search ',' or closeChar
                    int keyPos;
                    if (!current.TrySearchByte(x => x == ',', out keyPos))
                    {
                        throw new ParserException("',' expected");
                    }
                    current = current.Subbytes(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearchByte(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new ParserException("not whitespace expected");
                    }
                    current = current.Subbytes(nextToken);
                }

                // key
                var key = Parse(current, values, parentIndex);
                if (key.ValueType != ValueNodeType.String)
                {
                    throw new ParserException("object key must string: " + key.Segment);
                }
                current = current.Subbytes(key.Segment.ByteLength);

                // search ':'
                int valuePos;
                if (!current.TrySearchByte(x => x == ':', out valuePos))
                {
                    throw new ParserException(": is not found");
                }
                current = current.Subbytes(valuePos + 1);

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearchByte(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new ParserException("not whitespace expected");
                    }
                    current = current.Subbytes(nextToken);
                }

                // value
                var value = Parse(current, values, parentIndex);
                current = current.Subbytes(value.Segment.ByteLength);
            }

            return current;
        }

        static JsonValue Parse(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            // skip white space
            int pos;
            if (!segment.TrySearchByte(x => !char.IsWhiteSpace((char)x), out pos))
            {
                throw new ParserException("only whitespace");
            }
            segment = segment.Subbytes(pos);

            var valueType = GetValueType(segment);
            switch (valueType)
            {
                case ValueNodeType.Boolean:
                case ValueNodeType.Integer:
                case ValueNodeType.Number:
                case ValueNodeType.Null:
                case ValueNodeType.NaN:
                case ValueNodeType.Infinity:
                case ValueNodeType.MinusInfinity:
                    {
                        var value= ParsePrimitive(segment, valueType, parentIndex);
                        values.Add(value);
                        return value;
                    }

                case ValueNodeType.String:
                    {
                        var value= ParseString(segment, parentIndex);
                        values.Add(value);
                        return value;
                    }

                case ValueNodeType.Array: // fall through
                    {
                        var index = values.Count;
                        values.Add(new JsonValue()); // placeholder
                        var current = ParseArray(segment, values, index);
                        values[index] = new JsonValue(segment.Subbytes(0, current.Bytes.Offset + 1 - segment.Bytes.Offset),
                            ValueNodeType.Array, parentIndex);
                        return values[index];
                    }

                case ValueNodeType.Object: // fall through
                    {
                        var index = values.Count;
                        values.Add(new JsonValue()); // placeholder
                        var current=ParseObject(segment, values, index);
                        values[index] = new JsonValue(segment.Subbytes(0, current.Bytes.Offset + 1 - segment.Bytes.Offset),
                            ValueNodeType.Object, parentIndex);
                        return values[index];
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public static ListTreeNode<JsonValue> Parse(String json)
        {
            return Parse(Utf8String.From(json));
        }

        public static ListTreeNode<JsonValue> Parse(Utf8String json)
        {
            var result = new List<JsonValue>();
            var value = Parse(json, result, -1);
            if (value.ValueType != ValueNodeType.Array && value.ValueType != ValueNodeType.Object)
            {
                result.Add(value);
                return new ListTreeNode<JsonValue>(result);
            }
            else
            {
                return new ListTreeNode<JsonValue>(result);
            }
        }
    }
}

using System;


namespace UniJSON
{
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
                    if (segment.ByteLength >= 2 && Char.ToLower((char)segment[1]) == 'a')
                    {
                        return ValueNodeType.NaN;
                    }

                    return ValueNodeType.Null;

                case 'i':
                    return ValueNodeType.Infinity;

                case '-':
                    if (segment.ByteLength >= 2 && Char.ToLower((char)segment[1]) == 'i')
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
        static JsonNode ParsePrimitive(JsonNode tree, Utf8String segment, ValueNodeType valueType)
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
            return tree.AddValue(segment.Subbytes(0, i).Bytes, valueType);
        }

        static JsonNode ParseString(JsonNode tree, Utf8String segment)
        {
            int pos;
            if (segment.TrySearchAscii((Byte)'"', 1, out pos))
            {
                return tree.AddValue(segment.Subbytes(0, pos + 1).Bytes, ValueNodeType.String);
            }
            else
            {
                throw new ParserException("no close string: " + segment);
            }
        }

        static JsonNode ParseArray(JsonNode tree, Utf8String segment)
        {
            var array = tree.AddValue(segment.Bytes, ValueNodeType.Array);

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
                var child = Parse(array, current);
                current = current.Subbytes(child.Value.Segment.ByteLength);
            }

            // fix array range
            var count = current.Bytes.Offset + 1 - segment.Bytes.Offset;
            array.SetValueBytesCount(count);
            
            return array;
        }

        static JsonNode ParseObject(JsonNode tree, Utf8String segment)
        {
            var obj = tree.AddValue(segment.Bytes, ValueNodeType.Object);

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
                var key = Parse(obj, current);
                if (!key.IsString())
                {
                    throw new ParserException("object key must string: " + key.Value.Segment);
                }
                current = current.Subbytes(key.Value.Segment.ByteLength);

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
                var value = Parse(obj, current);
                current = current.Subbytes(value.Value.Segment.ByteLength);
            }

            // fix obj range
            var count = current.Bytes.Offset + 1 - segment.Bytes.Offset;
            obj.SetValueBytesCount(count);

            return obj;
        }

        public static JsonNode Parse(JsonNode tree, Utf8String segment)
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
                    return ParsePrimitive(tree, segment, valueType);

                case ValueNodeType.String:
                    return ParseString(tree, segment);

                case ValueNodeType.Array: // fall through
                    return ParseArray(tree, segment);

                case ValueNodeType.Object: // fall through
                    return ParseObject(tree, segment);

                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonNode Parse(String json)
        {
            return Parse(Utf8String.From(json));
        }

        public static JsonNode Parse(Utf8String json)
        {
            return Parse(default(JsonNode), json);
        }
    }
}

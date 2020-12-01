using System;
using System.Collections.Generic;


namespace UniJSON
{
    public static class TomlParser
    {
        static TomlValue ParseLHS(Utf8String segment, int parentIndex)
        {
            var it = segment.GetIterator();
            while (it.MoveNext())
            {
                if (it.Current == '"')
                {
                    throw new NotImplementedException();
                }
                else if (it.Current == '.')
                {
                    throw new NotImplementedException();
                }
                else if (it.Current == ' ' || it.Current == '\t' || it.Current == '=')
                {
                    return new TomlValue(segment.Subbytes(0, it.BytePosition),
                        TomlValueType.BareKey, parentIndex);
                }
            }

            throw new NotImplementedException();
        }

        static TomlValue ParseRHS(Utf8String segment, int parentIndex)
        {
            switch ((char)segment[0])
            {
                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (segment.IsInt)
                    {
                        return new TomlValue(segment.SplitInteger(), TomlValueType.Integer, parentIndex);
                    }
                    else
                    {
                        return new TomlValue(segment, TomlValueType.Float, parentIndex);
                    }

                case '"':
                    {
                        int pos;
                        if (segment.TrySearchAscii((Byte)'"', 1, out pos))
                        {
                            return new TomlValue(segment.Subbytes(0, pos + 1), TomlValueType.BasicString, parentIndex);
                        }
                        else
                        {
                            throw new ParserException("no close string: " + segment);
                        }
                    }

                case '[':
                    {
                        throw new NotImplementedException();
                    }
            }

            throw new NotImplementedException();
        }

        public static ListTreeNode<TomlValue> Parse(Utf8String segment)
        {
            var values = new List<TomlValue>()
            {
                new TomlValue(segment, TomlValueType.Table, -1),
            };
            var current = 0;

            while (!segment.IsEmpty)
            {
                segment = segment.TrimStart();
                if (segment.IsEmpty)
                {
                    break;
                }

                if (segment[0] == '#')
                {
                    // comment line
                    // skip to line end
                    segment = segment.Subbytes(segment.GetLine().ByteLength);
                    continue;
                }

                if (segment.ByteLength>=4 && segment[0]=='[' && segment[1]=='[')
                {
                    // [[array_name]]
                    throw new NotImplementedException();
                }
                else if (segment.ByteLength>=2 && segment[0]=='[')
                {
                    // [table_name]
                    int table_end;
                    if (!segment.TrySearchByte(x => x == ']', out table_end))
                    {
                        throw new ParserException("] not found");
                    }
                    var table_name = segment.Subbytes(1, table_end-1).Trim();
                    if (table_name.IsEmpty)
                    {
                        throw new ParserException("empty table name");
                    }

                    // top level key
                    values.Add(new TomlValue(table_name, TomlValueType.Table, 0));
                    current = values.Count - 1;

                    // skip to line end
                    segment = segment.Subbytes(segment.GetLine().ByteLength);
                }
                else
                {
                    // key = value
                    {
                        var key = ParseLHS(segment, current);
                        switch(key.TomlValueType)
                        {
                            case TomlValueType.BareKey:
                            case TomlValueType.QuotedKey:
                                {
                                    values.Add(key);

                                    // skip key
                                    segment = segment.Subbytes(key.Bytes.Count);
                                }
                                break;

                            case TomlValueType.DottedKey:
                                throw new NotImplementedException();
                        }
                    }

                    {
                        // search and skip =
                        int eq;
                        if (!segment.TrySearchByte(x => x == '=', out eq))
                        {
                            throw new ParserException("= not found");
                        }
                        segment = segment.Subbytes(eq + 1);

                        // skip white space
                        segment = segment.TrimStart();
                    }

                    {
                        var value = ParseRHS(segment, current);
                        values.Add(value);

                        // skip value
                        segment = segment.Subbytes(value.Bytes.Count);

                        // skip to line end
                        segment = segment.Subbytes(segment.GetLine().ByteLength);
                    }
                }
            }

            return new ListTreeNode<TomlValue>(values);
        }

        public static ListTreeNode<TomlValue> Parse(String Toml)
        {
            return Parse(Utf8String.From(Toml));
        }
    }
}

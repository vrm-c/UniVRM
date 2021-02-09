using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public struct JsonNode
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is JsonNode))
            {
                return false;
            }

            var rhs = (JsonNode)obj;

            if ((Value.ValueType == ValueNodeType.Integer || Value.ValueType == ValueNodeType.Null)
                && (rhs.Value.ValueType == ValueNodeType.Integer || rhs.Value.ValueType == ValueNodeType.Number))
            {
                // ok
            }
            else if (Value.ValueType != rhs.Value.ValueType)
            {
                return false;
            }

            switch (Value.ValueType)
            {
                case ValueNodeType.Null:
                    return true;

                case ValueNodeType.Boolean:
                    return Value.GetBoolean() == rhs.GetBoolean();

                case ValueNodeType.Integer:
                case ValueNodeType.Number:
                    return Value.GetDouble() == rhs.GetDouble();

                case ValueNodeType.String:
                    return Value.GetString() == rhs.GetString();

                case ValueNodeType.Array:
                    return this.ArrayItems().SequenceEqual(rhs.ArrayItems());

                case ValueNodeType.Object:
                    {
                        //var l = ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                        //var r = rhs.ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                        //return l.Equals(r);
                        return this.ObjectItems().OrderBy(x => x.Key.GetUtf8String()).SequenceEqual(rhs.ObjectItems().OrderBy(x => x.Key.GetUtf8String()));
                    }
            }

            return false;
        }

        public override string ToString()
        {
            if (this.IsArray())
            {
                var sb = new StringBuilder();
                sb.Append("[");
                /*
                bool isFirst = true;
                foreach (var x in this.ArrayItems())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(x.ToString());                    
                }
                */
                sb.Append("]");
                return sb.ToString();
            }
            else if (this.IsMap())
            {
                var sb = new StringBuilder();
                sb.Append("{");
                /*
                bool isFirst = true;
                foreach (var kv in this.ObjectItems())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(kv.Key.ToString());
                    sb.Append(": ");
                    sb.Append(kv.Value.ToString());
                }
                */
                sb.Append("}");
                return sb.ToString();
            }
            else
            {
                return Value.ToString();
            }
        }

        IEnumerable<string> ToString(string indent, int level, bool value = false)
        {
            if (this.IsArray())
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return "[\n";

                var isFirst = true;
                var childLevel = level + 1;
                foreach (var x in this.ArrayItems())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        yield return ",\n";
                    }

                    foreach (var y in x.ToString(indent, childLevel))
                    {
                        yield return y;
                    }
                }
                if (!isFirst)
                {
                    yield return "\n";
                }

                for (int i = 0; i < level; ++i) yield return indent;
                yield return "]";
            }
            else if (this.IsMap())
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return "{\n";

                var isFirst = true;
                var childLevel = level + 1;
                foreach (var kv in this.ObjectItems().OrderBy(x => x.Key.ToString()))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        yield return ",\n";
                    }

                    // key
                    for (int i = 0; i < childLevel; ++i) yield return indent;
                    yield return kv.Key.ToString();
                    yield return ": ";

                    foreach (var y in kv.Value.ToString(indent, childLevel, true))
                    {
                        yield return y;
                    }
                }
                if (!isFirst)
                {
                    yield return "\n";
                }

                for (int i = 0; i < level; ++i) yield return indent;
                yield return "}";
            }
            else
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return Value.ToString();
            }
        }

        public string ToString(string indent)
        {
            return string.Join("", ToString(indent, 0).ToArray());
        }

        public IEnumerable<JsonDiff> Diff(JsonNode rhs, JsonPointer path = default(JsonPointer))
        {
            switch (Value.ValueType)
            {
                case ValueNodeType.Null:
                case ValueNodeType.Boolean:
                case ValueNodeType.Number:
                case ValueNodeType.Integer:
                case ValueNodeType.String:
                    if (!Equals(rhs))
                    {
                        yield return JsonDiff.Create(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value, rhs.Value));
                    }
                    yield break;
            }

            if (Value.ValueType != rhs.Value.ValueType)
            {
                yield return JsonDiff.Create(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value.ValueType, rhs.Value));
                yield break;
            }

            if (Value.ValueType == ValueNodeType.Object)
            {

                var l = this.ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                var r = rhs.ObjectItems().ToDictionary(x => x.Key, x => x.Value);

                foreach (var kv in l)
                {
                    JsonNode x;
                    if (r.TryGetValue(kv.Key, out x))
                    {
                        r.Remove(kv.Key);
                        // Found
                        foreach (var y in kv.Value.Diff(x))
                        {
                            yield return y;
                        }
                    }
                    else
                    {
                        // Removed
                        yield return JsonDiff.Create(kv.Value, JsonDiffType.KeyRemoved, kv.Value.Value.ToString());
                    }
                }

                foreach (var kv in r)
                {
                    // Added
                    yield return JsonDiff.Create(kv.Value, JsonDiffType.KeyAdded, kv.Value.Value.ToString());
                }
            }
            else if (Value.ValueType == ValueNodeType.Array)
            {
                var ll = this.ArrayItems().GetEnumerator();
                var rr = rhs.ArrayItems().GetEnumerator();
                while (true)
                {
                    var lll = ll.MoveNext();
                    var rrr = rr.MoveNext();
                    if (lll && rrr)
                    {
                        // found
                        foreach (var y in ll.Current.Diff(rr.Current))
                        {
                            yield return y;
                        }
                    }
                    else if (lll)
                    {
                        yield return JsonDiff.Create(ll.Current, JsonDiffType.KeyRemoved, ll.Current.Value.ToString());
                    }
                    else if (rrr)
                    {
                        yield return JsonDiff.Create(rr.Current, JsonDiffType.KeyAdded, rr.Current.Value.ToString());
                    }
                    else
                    {
                        // end
                        break;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Whole tree nodes
        /// </summary>
        List<JsonValue> m_Values;
        public bool IsValid
        {
            get
            {
                return m_Values != null;
            }
        }

        /// <summary>
        /// This node index
        /// </summary>
        int _valueIndex;
        public int ValueIndex
        {
            get
            {
                if (m_Values == null) return -1;
                return _valueIndex;
            }
        }

        public JsonNode Prev
        {
            get
            {
                return new JsonNode(m_Values, ValueIndex - 1);
            }
        }

        public JsonValue Value
        {
            get
            {
                if (m_Values == null)
                {
                    return default;
                }
                return m_Values[ValueIndex];
            }
        }
        public void SetValue(JsonValue value)
        {
            m_Values[ValueIndex] = value;
        }

        #region Children
        public int ChildCount
        {
            get { return Value.ChildCount; }
        }

        public IEnumerable<JsonNode> Children
        {
            get
            {
                int count = 0;
                for (int i = ValueIndex; count < ChildCount && i < m_Values.Count; ++i)
                {
                    if (m_Values[i].ParentIndex == ValueIndex)
                    {
                        ++count;
                        yield return new JsonNode(m_Values, i);
                    }
                }
            }
        }

        public JsonNode this[String key]
        {
            get
            {
                return this[Utf8String.From(key)];
            }
        }

        public JsonNode this[Utf8String key]
        {
            get
            {
                return this.GetObjectItem(key);
            }
        }

        public JsonNode this[int index]
        {
            get
            {
                return this.GetArrayItem(index);
            }
        }
        #endregion
        public bool HasParent
        {
            get
            {
                return Value.ParentIndex >= 0 && Value.ParentIndex < m_Values.Count;
            }
        }
        public JsonNode Parent
        {
            get
            {
                if (Value.ParentIndex < 0)
                {
                    throw new Exception("no parent");
                }
                if (Value.ParentIndex >= m_Values.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return new JsonNode(m_Values, Value.ParentIndex);
            }
        }

        public JsonNode(List<JsonValue> values, int index = 0) : this()
        {
            m_Values = values;
            _valueIndex = index;
        }

        #region JsonPointer
        public JsonNode AddKey(Utf8String key)
        {
            return AddValue(default(JsonValue).Key(key, ValueIndex));
        }

        public JsonNode AddValue(ArraySegment<byte> bytes, ValueNodeType valueType)
        {
            return AddValue(default(JsonValue).New(bytes, valueType, ValueIndex));
        }

        public JsonNode AddValue(JsonValue value)
        {
            if (m_Values == null)
            {
                // initialize empty tree
                m_Values = new List<JsonValue>();
                _valueIndex = -1;
            }
            else
            {
                IncrementChildCount();
            }
            var index = m_Values.Count;
            m_Values.Add(value);
            return new JsonNode(m_Values, index);
        }

        void IncrementChildCount()
        {
            var value = Value;
            value.SetChildCount(value.ChildCount + 1);
            SetValue(value);
        }

        public void SetValueBytesCount(int count)
        {
            var value = Value;
            value.SetBytesCount(count);
            SetValue(value);
        }
        #endregion
    }
}

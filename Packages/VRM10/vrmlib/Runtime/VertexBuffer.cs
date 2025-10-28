using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;

namespace VrmLib
{
    public class VertexBuffer : IEnumerable<KeyValuePair<string, BufferAccessor>>
    {
        public Dictionary<string, BufferAccessor> VertexBuffers = new Dictionary<string, BufferAccessor>();

        public bool ContainsKey(string key)
        {
            return VertexBuffers.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, BufferAccessor>> GetEnumerator()
        {
            return VertexBuffers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string key, BufferAccessor accessor)
        {
            // if exists, over write
            VertexBuffers[key] = accessor;
        }

        public bool TryGetValue(string key, out BufferAccessor accessor)
        {
            return VertexBuffers.TryGetValue(key, out accessor);
        }

        public int Count
        {
            get
            {
                if (VertexBuffers.TryGetValue(PositionKey, out BufferAccessor buffer))
                {
                    return buffer.Count;
                }
                return 0;
            }
        }

        public const string PositionKey = "POSITION";

        public BufferAccessor Positions
        {
            get
            {
                if (VertexBuffers.TryGetValue(PositionKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public const string NormalKey = "NORMAL";
        public BufferAccessor Normals
        {
            get
            {
                if (VertexBuffers.TryGetValue(NormalKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public const string TangentKey = "TANGENT";
        public const string ColorKey = "COLOR_0";
        public BufferAccessor Colors
        {
            get
            {
                if (VertexBuffers.TryGetValue(ColorKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public const string TexCoordKey = "TEXCOORD_0";
        public BufferAccessor TexCoords
        {
            get
            {
                if (VertexBuffers.TryGetValue(TexCoordKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public const string TexCoordKey2 = "TEXCOORD_1";

        public const string JointKey = "JOINTS_0";
        public BufferAccessor Joints
        {
            get
            {
                if (VertexBuffers.TryGetValue(JointKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public const string WeightKey = "WEIGHTS_0";
        public BufferAccessor Weights
        {
            get
            {
                if (VertexBuffers.TryGetValue(WeightKey, out BufferAccessor buffer))
                {
                    return buffer;
                }
                return null;
            }
        }

        public void RemoveTangent()
        {
            if (VertexBuffers.ContainsKey(TangentKey))
            {
                VertexBuffers.Remove(TangentKey);
            }
        }

        public int ByteLength
        {
            get
            {
                return VertexBuffers.Sum(x => x.Value.ByteLength);
            }
        }

        public void ValidateLength(string name = "")
        {
            foreach (var kv in VertexBuffers)
            {
                if (kv.Key == PositionKey) continue;

                if (kv.Value.Count != Count)
                {
                    var msg = "vertex attribute not same length";
                    if (!string.IsNullOrEmpty(name))
                    {
                        msg = $"{name}: {msg}";
                    }
                    throw new ArgumentException(msg);
                }
            }
        }

        public void ValidateNAN()
        {
            foreach (var kv in VertexBuffers)
            {
                if (kv.Value.ComponentType == AccessorValueType.FLOAT)
                {
                    var values = kv.Value.GetSpan<float>(false);
                    int i = 0;
                    foreach (var f in values)
                    {
                        if (float.IsNaN(f)) throw new ArithmeticException("float error");
                        ++i;
                    }
                }
            }
        }

        public VertexBuffer()
        {
        }
    }
}

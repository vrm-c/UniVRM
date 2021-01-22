using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniGLTF
{
    public enum GlbChunkType : uint
    {
        JSON = 0x4E4F534A,
        BIN = 0x004E4942,
    }

    public struct GlbHeader
    {

        public static readonly byte[] GLB_MAGIC = new byte[] { 0x67, 0x6C, 0x54, 0x46 };  // "glTF"
        public static readonly byte[] GLB_VERSION = new byte[] { 2, 0, 0, 0 };

        public static void WriteTo(Stream s)
        {
            s.Write(GLB_MAGIC, 0, GLB_MAGIC.Length);
            s.Write(GLB_VERSION, 0, GLB_VERSION.Length);
        }

        public static int TryParse(ArraySegment<Byte> bytes, out int pos, out Exception message)
        {
            pos = 0;

            if (!bytes.Slice(0, 4).SequenceEqual(GLB_MAGIC))
            {
                message = new FormatException("invalid magic");
                return 0;
            }
            pos += 4;

            if (!bytes.Slice(pos, 4).SequenceEqual(GLB_VERSION))
            {
                message = new FormatException("invalid magic");
                return 0;
            }
            pos += 4;

            var totalLength = BitConverter.ToInt32(bytes.Array, bytes.Offset + pos);
            pos += 4;

            message = null;
            return totalLength;
        }
    }

    public struct GlbChunk
    {
        public GlbChunkType ChunkType;
        public ArraySegment<Byte> Bytes;

        public GlbChunk(string json) : this(
            GlbChunkType.JSON,
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(json))
            )
        {
        }

        public GlbChunk(ArraySegment<Byte> bytes) : this(
            GlbChunkType.BIN,
            bytes
            )
        {
        }

        public GlbChunk(GlbChunkType type, ArraySegment<Byte> bytes)
        {
            ChunkType = type;
            Bytes = bytes;
        }

        public static GlbChunk CreateJson(string json)
        {
            return CreateJson(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)));
        }

        public static GlbChunk CreateJson(ArraySegment<byte> bytes)
        {
            return new GlbChunk(GlbChunkType.JSON, bytes);
        }

        public static GlbChunk CreateBin(ArraySegment<Byte> bytes)
        {
            return new GlbChunk(GlbChunkType.BIN, bytes);
        }

        byte GetPaddingByte()
        {
            // chunk type
            switch (ChunkType)
            {
                case GlbChunkType.JSON:
                    return 0x20;

                case GlbChunkType.BIN:
                    return 0x00;

                default:
                    throw new Exception("unknown chunk type: " + ChunkType);
            }
        }

        public int WriteTo(Stream s)
        {
            // padding
            var paddingValue = Bytes.Count % 4;
            var padding = (paddingValue > 0) ? 4 - paddingValue : 0;

            // size
            var bytes = BitConverter.GetBytes((int)(Bytes.Count + padding));
            s.Write(bytes, 0, bytes.Length);

            // chunk type
            switch (ChunkType)
            {
                case GlbChunkType.JSON:
                    s.WriteByte((byte)'J');
                    s.WriteByte((byte)'S');
                    s.WriteByte((byte)'O');
                    s.WriteByte((byte)'N');
                    break;

                case GlbChunkType.BIN:
                    s.WriteByte((byte)'B');
                    s.WriteByte((byte)'I');
                    s.WriteByte((byte)'N');
                    s.WriteByte((byte)0);
                    break;

                default:
                    throw new Exception("unknown chunk type: " + ChunkType);
            }

            // body
            s.Write(Bytes.Array, Bytes.Offset, Bytes.Count);

            // 4byte align
            var pad = GetPaddingByte();
            for (int i = 0; i < padding; ++i)
            {
                s.WriteByte(pad);
            }

            return 4 + 4 + Bytes.Count + padding;
        }
    }

    /// <summary>
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#glb-file-format-specification 
    /// </summary>
    public struct Glb
    {
        public readonly GlbChunk Json;
        public readonly GlbChunk Binary;

        public Glb(GlbChunk json, GlbChunk binary)
        {
            if (json.ChunkType != GlbChunkType.JSON) throw new ArgumentException();
            Json = json;
            if (binary.ChunkType != GlbChunkType.BIN) throw new ArgumentException();
            Binary = binary;
        }

        public static Glb Create(ArraySegment<byte> json, ArraySegment<byte> bin)
        {
            return new Glb(GlbChunk.CreateJson(json), GlbChunk.CreateBin(bin));
        }

        public static Glb Create(string json, ArraySegment<byte> bin)
        {
            return new Glb(GlbChunk.CreateJson(json), GlbChunk.CreateBin(bin));
        }

        public byte[] ToBytes()
        {
            using (var s = new MemoryStream())
            {
                GlbHeader.WriteTo(s);

                var pos = s.Position;
                s.Position += 4; // skip total size

                int size = 12;

                {
                    size += Json.WriteTo(s);
                }
                {
                    size += Binary.WriteTo(s);
                }

                s.Position = pos;
                var bytes = BitConverter.GetBytes(size);
                s.Write(bytes, 0, bytes.Length);

                return s.ToArray();
            }
        }

        public static GlbChunkType ToChunkType(string src)
        {
            switch (src)
            {
                case "BIN":
                    return GlbChunkType.BIN;

                case "JSON":
                    return GlbChunkType.JSON;

                default:
                    throw new FormatException("unknown chunk type: " + src);
            }
        }

        public static Glb Parse(Byte[] bytes)
        {
            return Parse(new ArraySegment<byte>(bytes));
        }

        public static Glb Parse(ArraySegment<Byte> bytes)
        {
            if (TryParse(bytes, out Glb glb, out Exception ex))
            {
                return glb;
            }
            else
            {
                throw ex;
            }
        }

        public static bool TryParse(Byte[] bytes, out Glb glb, out Exception ex)
        {
            return TryParse(new ArraySegment<byte>(bytes), out glb, out ex);
        }

        public static bool TryParse(ArraySegment<Byte> bytes, out Glb glb, out Exception ex)
        {
            glb = default(Glb);
            if (bytes.Count == 0)
            {
                ex = new Exception("empty bytes");
                return false;
            }

            var length = GlbHeader.TryParse(bytes, out int pos, out ex);
            if (length == 0)
            {
                return false;
            }
            bytes = bytes.Slice(0, length);

            try
            {
                var chunks = new List<GlbChunk>();
                while (pos < bytes.Count)
                {
                    var chunkDataSize = BitConverter.ToInt32(bytes.Array, bytes.Offset + pos);
                    pos += 4;

                    //var type = (GlbChunkType)BitConverter.ToUInt32(bytes, pos);
                    var chunkTypeBytes = bytes.Slice(pos, 4).Where(x => x != 0).ToArray();
                    var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
                    var type = ToChunkType(chunkTypeStr);
                    pos += 4;

                    chunks.Add(new GlbChunk
                    {
                        ChunkType = type,
                        Bytes = bytes.Slice(pos, chunkDataSize)
                    });

                    pos += chunkDataSize;
                }

                glb = new Glb(chunks[0], chunks[1]);
                return true;
            }
            catch (Exception _ex)
            {
                ex = _ex;
                return false;
            }
        }
    }
}

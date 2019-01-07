using System;
using System.IO;
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
        public static void WriteTo(Stream s)
        {
            s.WriteByte((Byte)'g');
            s.WriteByte((Byte)'l');
            s.WriteByte((Byte)'T');
            s.WriteByte((Byte)'F');
            var bytes = BitConverter.GetBytes((UInt32)2);
            s.Write(bytes, 0, bytes.Length);
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
            for(int i=0; i<padding; ++i)
            {
                s.WriteByte(pad);
            }

            return 4 + 4 + Bytes.Count + padding;
        }
    }

    public static class Glb
    {
        public static byte[] ToBytes(string json, ArraySegment<byte> body)
        {
            using (var s = new MemoryStream())
            {
                GlbHeader.WriteTo(s);

                var pos = s.Position;
                s.Position += 4; // skip total size

                int size = 12;

                {
                    var chunk = new GlbChunk(json);
                    size += chunk.WriteTo(s);
                }
                {
                    var chunk = new GlbChunk(body);
                    size += chunk.WriteTo(s);
                }

                s.Position = pos;
                var bytes = BitConverter.GetBytes(size);
                s.Write(bytes, 0, bytes.Length);

                return s.ToArray();
            }
        }
    }
}

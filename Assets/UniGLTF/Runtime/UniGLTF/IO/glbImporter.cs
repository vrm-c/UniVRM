using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniGLTF
{
    public static class glbImporter
    {
        public const string GLB_MAGIC = "glTF";
        public const float GLB_VERSION = 2.0f;

        public static GlbChunkType ToChunkType(this string src)
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

        public static string ToChunkTypeString(this GlbChunkType type)
        {
            switch (type)
            {
                case GlbChunkType.JSON:
                    return "JSON";
                case GlbChunkType.BIN:
                    return "BIN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [Obsolete("Use ParseGlbChunks(bytes)")]
        public static List<GlbChunk> ParseGlbChanks(Byte[] bytes)
        {
            return ParseGlbChunks(bytes);
        }

        public static List<GlbChunk> ParseGlbChunks(Byte[] bytes)
        {
            //
            // glb header(12byte)
            //
            if (bytes.Length < 12)
            {
                throw new GlbParseException("glb header not found");
            }

            int pos = 0;
            if (Encoding.ASCII.GetString(bytes, 0, 4) != GLB_MAGIC)
            {
                throw new GlbParseException("invalid magic");
            }
            pos += 4;

            var version = BitConverter.ToUInt32(bytes, pos);
            if (version != GLB_VERSION)
            {
                throw new GlbParseException($"unknown version: {version}");
            }
            pos += 4;

            var totalLength = BitConverter.ToUInt32(bytes, pos);
            if (bytes.Length < totalLength)
            {
                throw new GlbParseException($"not enough size: {bytes.Length} < {totalLength}");
            }
            pos += 4;

            var chunks = new List<GlbChunk>();
            while (pos < bytes.Length)
            {
                var chunkDataSize = BitConverter.ToInt32(bytes, pos);
                pos += 4;

                //var type = (GlbChunkType)BitConverter.ToUInt32(bytes, pos);
                var chunkTypeBytes = bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
                var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
                pos += 4;

                chunks.Add(new GlbChunk
                {
                    ChunkTypeString = chunkTypeStr,
                    Bytes = new ArraySegment<byte>(bytes, (int)pos, (int)chunkDataSize)
                });

                pos += chunkDataSize;
            }

            return chunks;
        }
    }
}

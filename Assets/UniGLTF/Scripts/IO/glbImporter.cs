using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UniGLTF
{
    public static class glbImporter
    {
        public const string GLB_MAGIC = "glTF";
        public const float GLB_VERSION = 2.0f;

        public static GlbChunkType ToChunkType(string src)
        {
            switch(src)
            {
                case "BIN":
                    return GlbChunkType.BIN;

                case "JSON":
                    return GlbChunkType.JSON;

                default:
                    throw new FormatException("unknown chunk type: " + src);
            }
        }

        public static List<GlbChunk> ParseGlbChanks(Byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                throw new Exception("empty bytes");
            }

            int pos = 0;
            if (Encoding.ASCII.GetString(bytes, 0, 4) != GLB_MAGIC)
            {
                throw new Exception("invalid magic");
            }
            pos += 4;

            var version = BitConverter.ToUInt32(bytes, pos);
            if (version != GLB_VERSION)
            {
                Debug.LogWarningFormat("unknown version: {0}", version);
                return null;
            }
            pos += 4;

            //var totalLength = BitConverter.ToUInt32(bytes, pos);
            pos += 4;

            var chunks = new List<GlbChunk>();
            while (pos < bytes.Length)
            {
                var chunkDataSize = BitConverter.ToInt32(bytes, pos);
                pos += 4;

                //var type = (GlbChunkType)BitConverter.ToUInt32(bytes, pos);
                var chunkTypeBytes = bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
                var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
                var type = ToChunkType(chunkTypeStr);
                pos += 4;

                chunks.Add(new GlbChunk
                {
                    ChunkType = type,
                    Bytes = new ArraySegment<byte>(bytes, (int)pos, (int)chunkDataSize)
                });

                pos += chunkDataSize;
            }

            return chunks;
        }
    }
}

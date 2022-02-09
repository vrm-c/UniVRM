using System;
using UniGLTF;
using UniJSON;

namespace UniVRM10
{
    public class Vrm10ExportData : ExportingGltfData
    {
        public void Reserve(int bytesLength)
        {
            _buffer.ExtendCapacity(bytesLength);
        }

        public int AppendToBuffer(ArraySegment<byte> segment)
        {
            var gltfBufferView = _buffer.Extend(segment);
            var viewIndex = GLTF.bufferViews.Count;
            GLTF.bufferViews.Add(gltfBufferView);
            return viewIndex;
        }

        public byte[] ToBytes()
        {
            GLTF.buffers[0].byteLength = _buffer.Bytes.Count;

            var f = new JsonFormatter();
            UniGLTF.GltfSerializer.Serialize(f, GLTF);
            var json = f.GetStoreBytes();

            var glb = UniGLTF.Glb.Create(json, _buffer.Bytes);
            return glb.ToBytes();
        }
    }
}

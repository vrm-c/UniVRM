using System;


namespace UniGLTF
{
    /// <summary>
    /// for glb chunk buffer read
    /// </summary>
    public class ArraySegmentByteBuffer : IBytesBuffer
    {
        ArraySegment<Byte> m_bytes;

        public ArraySegmentByteBuffer(ArraySegment<Byte> bytes)
        {
            m_bytes = bytes;
        }

        public string Uri
        {
            get;
            private set;
        }

        public glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target) where T : struct
        {
            throw new NotImplementedException();
        }

        public void ExtendCapacity(int capacity)
        {
            throw new NotImplementedException();
        }

        public ArraySegment<byte> Bytes => m_bytes;
    }
}

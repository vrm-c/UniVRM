using System;

namespace UniGLTF
{
    public interface IBytesBuffer
    {
        string Uri { get; }
        ArraySegment<Byte> Bytes { get; }
        glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target = glBufferTarget.NONE) where T : struct;
        void ExtendCapacity(int capacity);
    }
}

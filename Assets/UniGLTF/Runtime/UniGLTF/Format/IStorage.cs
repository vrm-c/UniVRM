using System;

namespace UniGLTF
{
    public interface IStorage
    {
        ArraySegment<Byte> Get(string url = default);
    }
}

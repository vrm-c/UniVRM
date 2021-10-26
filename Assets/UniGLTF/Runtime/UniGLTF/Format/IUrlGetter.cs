using System;

namespace UniGLTF
{
    public interface IUrlGetter
    {
        ArraySegment<Byte> Get(string url = default);
    }
}

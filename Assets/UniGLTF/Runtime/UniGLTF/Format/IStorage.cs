using System;

namespace UniGLTF
{
    public interface IStorage
    {
        ArraySegment<Byte> Get(string url = default);

        /// <summary>
        /// Get original filepath if exists
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetPath(string url);
    }
}

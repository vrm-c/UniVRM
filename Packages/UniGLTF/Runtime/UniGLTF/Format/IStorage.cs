using System;

namespace UniGLTF
{
    /// <summary>
    /// Represents bytes access by URL in gltf
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// gltf の buffer の バイト列アクセス を実装する。
        /// 1. url による相対パス
        /// 2. url によるbase64 encoding
        /// 3. url がnullのときに bin chunk(buffers[0]) にアクセスする
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        ArraySegment<Byte> Get(string url = default);
    }
}

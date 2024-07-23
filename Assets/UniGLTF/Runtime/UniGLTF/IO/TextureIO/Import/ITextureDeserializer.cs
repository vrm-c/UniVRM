using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 単純に Texture2D アセットを生成する機能
    /// </summary>
    public interface ITextureDeserializer
    {
        /// <summary>
        /// imageData をもとに Texture2D を生成する.
        /// await する場合は awaitCaller を用いて await しなければならない。(Editor では同期ロードをしなければならないため)
        /// </summary>
        Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller);
    }
}

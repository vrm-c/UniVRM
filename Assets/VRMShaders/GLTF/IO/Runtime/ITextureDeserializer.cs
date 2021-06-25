using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// 単純に Texture2D アセットを生成する機能
    /// </summary>
    public interface ITextureDeserializer
    {
        /// <summary>
        /// imageData をもとに Texture2D を生成する
        /// </summary>
        /// <param name="imageData">データ</param>
        /// <param name="useMipmap">Texture2D の mipmap が生成されるべきか否か</param>
        /// <param name="colorSpace">Texture2D の色空間</param>
        /// <param name="awaitCaller">await caller</param>
        /// <returns></returns>
        Task<Texture2D> LoadTextureAsync(byte[] imageData, bool useMipmap, ColorSpace colorSpace, IAwaitCaller awaitCaller);
    }
}

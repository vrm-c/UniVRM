using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    public interface ITextureDeserializer
    {
        Task<Texture2D> LoadTextureAsync(GetTextureBytesAsync getTextureBytesAsync, bool useMipmap, ColorSpace colorSpace);
    }
}

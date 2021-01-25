using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif


namespace UniGLTF
{
    public interface ITextureExporter
    {
        (Byte[] bytes, string mine) GetBytesWithMime(Texture texture, glTFTextureTypes textureType);
        IEnumerable<(Texture texture, glTFTextureTypes textureType)> GetTextures(Material m);
        int ExportTexture(glTF gltf, int bufferIndex, Texture texture, glTFTextureTypes textureType);
    }
}

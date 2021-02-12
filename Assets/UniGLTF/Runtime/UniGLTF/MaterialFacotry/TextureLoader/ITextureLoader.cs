using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public interface ITextureLoader : IDisposable
    {
        Texture2D Texture { get; }

        /// <summary>
        /// Call from unity main thread
        /// </summary>
        /// <param name="isLinear"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        IEnumerator ProcessOnMainThread(glTF gltf, IStorage storage, bool isLinear, glTFTextureSampler sampler);
    }
}

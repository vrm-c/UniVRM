using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    public interface ITextureLoader : IDisposable
    {
        Texture2D Texture { get; }

        /// <summary>
        /// Call from any thread
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="storage"></param>
        void ProcessOnAnyThread(glTF gltf, IStorage storage);

        /// <summary>
        /// Call from unity main thread
        /// </summary>
        /// <param name="isLinear"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        IEnumerator ProcessOnMainThread(bool isLinear, glTFTextureSampler sampler);
    }



}

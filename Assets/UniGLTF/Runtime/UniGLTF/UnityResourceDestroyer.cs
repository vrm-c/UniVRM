using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    /// <summary>
    /// Mesh, Material, Texture などを抱えておいて確実に破棄できるようにする
    /// </summary>
    public class UnityResourceDestroyer : MonoBehaviour
    {
        List<UnityEngine.Object> m_resources = new List<UnityEngine.Object>();
        public IList<UnityEngine.Object> Resources => m_resources;

        void OnDestroy()
        {
            Debug.Log("UnityResourceDestroyer.OnDestroy");
            foreach (var x in Resources)
            {
#if VRM_DEVELOP
                Debug.Log($"Destroy: {x}");
#endif                
                Destroy(x);
            }
        }

        public static Action<UnityEngine.Object> DestroyResource()
        {
            Action<UnityEngine.Object> des = (UnityEngine.Object o) => UnityEngine.Object.Destroy(o);
            Action<UnityEngine.Object> desi = (UnityEngine.Object o) => UnityEngine.Object.DestroyImmediate(o);
            Action<UnityEngine.Object> func = Application.isPlaying
                ? des
                : desi
                ;
            return func;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    /// <summary>
    /// Mesh, Material, Texture などを抱えておいて確実に破棄できるようにする
    /// </summary>
    public class UnityResourceDestroyer : MonoBehaviour
    {
        List<UnityEngine.Object> m_resources = new List<Object>();
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
    }
}

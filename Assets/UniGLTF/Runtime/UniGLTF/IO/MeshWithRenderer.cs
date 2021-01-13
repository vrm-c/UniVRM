using System;
using UnityEngine;

namespace UniGLTF
{
    public struct MeshWithRenderer
    {
        public Mesh Mesh;
        [Obsolete("Use Renderer")]
        public Renderer Rendererer => Renderer;
        public Renderer Renderer;

        public MeshWithRenderer(Transform x)
        {
            Mesh = x.GetSharedMesh();
            Renderer = x.GetComponent<Renderer>();
        }
    }
}

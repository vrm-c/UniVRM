using System;
using System.Collections.Generic;
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

        public static IEnumerable<MeshWithRenderer> FromNodes(IEnumerable<Transform> nodes)
        {
            foreach (var node in nodes)
            {
                var x = new MeshWithRenderer(node);
                if (x.Mesh == null)
                {
                    continue; ;
                }
                if (x.Renderer.sharedMaterials == null
                || x.Renderer.sharedMaterials.Length == 0)
                {
                    continue;
                }

                yield return x;
            }
        }
    }
}

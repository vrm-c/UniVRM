using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public class ModelMap
    {
        public readonly Dictionary<VrmLib.Node, GameObject> Nodes = new Dictionary<VrmLib.Node, GameObject>();
        public readonly Dictionary<VrmLib.MeshGroup, Mesh> Meshes = new Dictionary<VrmLib.MeshGroup, Mesh>();
        public readonly Dictionary<VrmLib.Node, Renderer> Renderers = new Dictionary<VrmLib.Node, Renderer>();
    }
}

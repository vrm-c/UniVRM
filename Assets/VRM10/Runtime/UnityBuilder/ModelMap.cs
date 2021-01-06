using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public class ModelMap
    {
        public readonly Dictionary<VrmLib.Node, GameObject> Nodes = new Dictionary<VrmLib.Node, GameObject>();
        public readonly Dictionary<VrmLib.Texture, Texture2D> Textures = new Dictionary<VrmLib.Texture, Texture2D>();
        public readonly Dictionary<VrmLib.Material, Material> Materials = new Dictionary<VrmLib.Material, Material>();
        public readonly Dictionary<VrmLib.MeshGroup, Mesh> Meshes = new Dictionary<VrmLib.MeshGroup, Mesh>();
        public readonly Dictionary<VrmLib.Node, Renderer> Renderers = new Dictionary<VrmLib.Node, Renderer>();
    }
}

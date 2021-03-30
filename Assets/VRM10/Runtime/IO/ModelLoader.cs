using System;
using System.Linq;
using VrmLib;

namespace UniVRM10
{
    public static class ModelLoader
    {
        public static Model Load(Vrm10Storage storage, string rootName)
        {
            if (storage == null)
            {
                return null;
            }

            var model = new Model(Coordinates.Vrm1)
            {
                AssetVersion = storage.AssetVersion,
                AssetGenerator = storage.AssetGenerator,
                AssetCopyright = storage.AssetCopyright,
                AssetMinVersion = storage.AssetMinVersion,
            };

            // node
            model.Root.Name = rootName;
            for (var i = 0; i < storage.NodeCount; ++i)
            {
                var node = storage.CreateNode(i);
                model.Nodes.Add(node);
            }
            for (var i = 0; i < model.Nodes.Count; ++i)
            {
                var parent = model.Nodes[i];
                foreach (var j in storage.GetChildNodeIndices(i))
                {
                    var child = model.Nodes[j];
                    parent.Add(child);
                }
            }
            foreach (var x in model.Nodes)
            {
                if (x.Parent == null)
                {
                    model.Root.Add(x);
                }
            }

            // skin
            model.Skins.AddRange(Enumerable.Range(0, storage.SkinCount).Select(x => storage.CreateSkin(x, model.Nodes)));

            // mesh
            model.MeshGroups.AddRange(Enumerable.Range(0, storage.MeshCount).Select(x => storage.CreateMesh(x)));

            // skin
            for (int i = 0; i < storage.NodeCount; ++i)
            {
                var (meshIndex, skinIndex) = storage.GetNodeMeshSkin(i);
                if (meshIndex >= 0 && meshIndex < model.MeshGroups.Count)
                {
                    var node = model.Nodes[i];
                    var mesh = model.MeshGroups[meshIndex];
                    node.MeshGroup = mesh;
                    if (skinIndex >= 0 && skinIndex < model.Skins.Count)
                    {
                        var skin = model.Skins[skinIndex];
                        mesh.Skin = skin;
                    }
                }
            }

            return model;
        }
    }
}

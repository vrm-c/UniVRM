using System;
using System.Linq;

namespace VrmLib
{
    public static class ModelLoader
    {
        public static Model Load(IVrmStorage storage, string rootName, bool estimateHumanoid = false)
        {
            if (storage == null)
            {
                return null;
            }

            var model = new Model(Coordinates.Gltf)
            {
                AssetVersion = storage.AssetVersion,
                AssetGenerator = storage.AssetGenerator,
                AssetCopyright = storage.AssetCopyright,
                AssetMinVersion = storage.AssetMinVersion,
                OriginalJson = storage.OriginalJson,
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

            // image
            model.Images.AddRange(Enumerable.Range(0, storage.ImageCount).Select(x => storage.CreateImage(x)));

            // texture
            model.Textures.AddRange(Enumerable.Range(0, storage.TextureCount).Select(x => storage.CreateTexture(x, model.Images)));

            // material
            model.Materials.AddRange(Enumerable.Range(0, storage.MaterialCount).Select(x => storage.CreateMaterial(x, model.Textures)));

            // skin
            model.Skins.AddRange(Enumerable.Range(0, storage.SkinCount).Select(x => storage.CreateSkin(x, model.Nodes)));

            // mesh
            model.MeshGroups.AddRange(Enumerable.Range(0, storage.MeshCount).Select(x => storage.CreateMesh(x, model.Materials)));

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

            // animation
            model.Animations.AddRange(Enumerable.Range(0, storage.AnimationCount).Select(x => storage.CreateAnimation(x, model.Nodes)));

            // VRM
            if (!LoadVrm(model, storage) && estimateHumanoid)
            {
                // VRMでないときにボーン推定する
                model.HumanoidBoneEstimate();
            }

            return model;
        }

        static bool LoadVrm(Model model, IVrmStorage storage)
        {
            if (!storage.HasVrm)
            {
                return false;
            }

            var meta = storage.CreateVrmMeta(model.Textures);

            var Vrm = new Vrm(meta, storage.VrmExporterVersion, storage.VrmSpecVersion);
            model.Vrm = Vrm;

            storage.LoadVrmHumanoid(model.Nodes);

            if (!model.CheckVrmHumanoid())
            {
                throw new Exception("CheckVrmHumanoid");
            }

            Vrm.ExpressionManager = storage.CreateVrmExpression(model.MeshGroups, model.Materials, model.Nodes);

            Vrm.SpringBone = storage.CreateVrmSpringBone(model.Nodes);

            Vrm.FirstPerson = storage.CreateVrmFirstPerson(model.Nodes, model.MeshGroups);

            Vrm.LookAt = storage.CreateVrmLookAt();

            return true;
        }
    }
}

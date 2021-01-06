using System.Linq;

namespace VrmLib
{
    public static class ModelExtensionsForSingleMesh
    {
        ///
        /// 各ノードのスキニングで使用されている回数
        ///
        public static int[] GetNodeSkinUseCount(Model model)
        {
            // create new skin
            var useCountList = new int[model.Nodes.Count];
            foreach (var n in model.Root.Traverse().Skip(1))
            {
                var g = n.MeshGroup;
                if (g == null)
                {
                    continue;
                }

                if (g.Skin == null)
                {
                    // Skin無し。そのMeshに乗る
                    var index = model.Nodes.IndexOf(n);
                    ++useCountList[index];
                }
                else
                {
                    // Skinあり。VertexBufferの JOINT_0 と WEIGHT_0 を見る
                    var skinJoints = g.Skin.Joints;
                    foreach (var m in g.Meshes)
                    {
                        var joints = m.VertexBuffer.GetOrCreateJoints();
                        var weights = m.VertexBuffer.GetOrCreateWeights();
                        for (int i = 0; i < joints.Length; ++i)
                        {
                            var j = joints[i];
                            var w = weights[i];
                            if (w.X > 0)
                            {
                                var node = skinJoints[j.Joint0];
                                var index = model.Nodes.IndexOf(node);
                                ++useCountList[index];
                            }
                            if (w.Y > 0)
                            {
                                var node = skinJoints[j.Joint1];
                                var index = model.Nodes.IndexOf(node);
                                ++useCountList[index];
                            }
                            if (w.Z > 0)
                            {
                                var node = skinJoints[j.Joint2];
                                var index = model.Nodes.IndexOf(node);
                                ++useCountList[index];
                            }
                            if (w.W > 0)
                            {
                                var node = skinJoints[j.Joint3];
                                var index = model.Nodes.IndexOf(node);
                                ++useCountList[index];
                            }
                        }
                    }
                }
            }
            return useCountList;
        }

        /// <summary>
        /// Integrate meshes to a single mesh
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MeshGroup CreateSingleMesh(this Model model, string name)
        {
            // new mesh to store result
            var meshGroup = new MeshGroup(name);
            var mesh = new Mesh
            {
                VertexBuffer = new VertexBuffer()
            };
            meshGroup.Meshes.Add(mesh);

            var useCountList = GetNodeSkinUseCount(model);

            // new Skin.
            // Joints has include all joint
            meshGroup.Skin = new Skin();
            for (int i = 0; i < useCountList.Length; ++i)
            {
                if (useCountList[i] > 0)
                {
                    // add joint that has bone weight
                    meshGroup.Skin.Joints.Add(model.Nodes[i]);
                }
            }
            model.Skins.Clear();
            model.Skins.Add(meshGroup.Skin);

            // concatenate all mesh
            foreach (var node in model.Root.Traverse().Skip(1))
            {
                var g = node.MeshGroup;
                if (g != null)
                {
                    foreach (var m in g.Meshes)
                    {
                        if (g.Skin != null && m.VertexBuffer.Joints != null && m.VertexBuffer.Weights != null)
                        {
                            var jointIndexMap = g.Skin.Joints.Select(x => meshGroup.Skin.Joints.IndexOf(x)).ToArray();
                            mesh.Append(m.VertexBuffer, m.IndexBuffer, m.Submeshes, m.MorphTargets, jointIndexMap);
                        }
                        else
                        {
                            var rootIndex = meshGroup.Skin.Joints.IndexOf(node);
                            mesh.Append(m.VertexBuffer, m.IndexBuffer, m.Submeshes, m.MorphTargets, null, rootIndex, node.Matrix);
                        }
                    }
                }
            }

            foreach (var target in mesh.MorphTargets)
            {
                target.VertexBuffer.Resize(mesh.VertexBuffer.Count);
            }

            return meshGroup;
        }
    }
}
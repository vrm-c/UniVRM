using System;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;

namespace UniVRM10
{
    public class VrmAnimationData
    {
        public GltfData Data { get; }

        public VrmAnimationData(GltfData data)
        {
            Data = data;

            // ヒューマノイド向け
            Func<glTFNode, glTFNode> getParent = (node) =>
            {
                var index = Data.GLTF.nodes.IndexOf(node);
                return Data.GLTF.nodes.FirstOrDefault(x => x.children != null && x.children.Contains(index));
            };

            ForceTransformUniqueName.Process(Data.GLTF.nodes,
                node => node.name,
                (node, name) => node.name = name,
                node =>
                {
                    var parent = getParent(node);
                    if (parent != null && node.children != null && node.children.Length == 0)
                    {
                        return $"{parent.name}-{node.name}";
                    }
                    return null;
                });
        }
    }
}
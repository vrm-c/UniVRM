using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF.Utils;

namespace UniGLTF
{
    public static class ForceGltfNodeUniqueName
    {
        public static void Process(List<glTFNode> nodes)
        {
            Func<glTFNode, glTFNode> getParent = (node) =>
            {
                var index = nodes.IndexOf(node);
                return nodes.FirstOrDefault(x => x.children != null && x.children.Contains(index));
            };

            ForceTransformUniqueName.Process(nodes,
                static node => node.name,
                static (node, name) => node.name = name,
                node =>
                {
                    var parent = getParent(node);
                    if (parent != null && (node.children == null || node.children.Length == 0))
                    {
                        return $"{parent.name}-{node.name}";
                    }
                    return null;
                });

        }
    }
}
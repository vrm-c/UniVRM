using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UniVRM10
{
    public static class ComponentBuilder
    {
        #region Util
        static (Transform, Mesh) GetTransformAndMesh(Transform t)
        {
            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return (t, skinnedMeshRenderer.sharedMesh);
            }

            var filter = t.GetComponent<MeshFilter>();
            if (filter != null)
            {
                return (t, filter.sharedMesh);
            }

            return default;
        }
        #endregion

        #region Build10

        static UniVRM10.MorphTargetBinding Build10(this VrmLib.MorphTargetBind bind, GameObject root, ModelMap loader)
        {
            var node = loader.Nodes[bind.Node].transform;
            var mesh = loader.Meshes[bind.Node.MeshGroup];
            // var transformMeshTable = loader.Root.transform.Traverse()
            //     .Select(GetTransformAndMesh)
            //     .Where(x => x.Item2 != null)
            //     .ToDictionary(x => x.Item2, x => x.Item1);
            // var node = transformMeshTable[mesh];
            // var transform = loader.Nodes[node].transform;
            var relativePath = node.RelativePathFrom(root.transform);

            var names = new List<string>();
            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                names.Add(mesh.GetBlendShapeName(i));
            }

            // VRM-1.0 では値域は [0-1.0f]
            return new UniVRM10.MorphTargetBinding(relativePath, names.IndexOf(bind.Name), bind.Value * 100.0f);
        }
        #endregion
    }
}

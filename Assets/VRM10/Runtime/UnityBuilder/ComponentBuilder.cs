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

        static UniVRM10.MaterialColorBinding? Build10(this VrmLib.MaterialColorBind bind, ModelMap loader)
        {
            var kv = bind.Property;
            var value = kv.Value.ToUnityVector4();
            var material = loader.Materials[bind.Material];

            var binding = default(UniVRM10.MaterialColorBinding?);
            if (material != null)
            {
                try
                {
                    binding = new UniVRM10.MaterialColorBinding
                    {
                        MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
                        BindType = bind.BindType,
                        TargetValue = value,
                        // BaseValue = material.GetColor(kv.Key),
                    };
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return binding;
        }

        static UniVRM10.MaterialUVBinding? Build10(this VrmLib.TextureTransformBind bind, ModelMap loader)
        {
            var material = loader.Materials[bind.Material];

            var binding = default(UniVRM10.MaterialUVBinding?);
            if (material != null)
            {
                try
                {
                    binding = new UniVRM10.MaterialUVBinding
                    {
                        MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
                        Scaling = new Vector2(bind.Scale.X, bind.Scale.Y),
                        Offset = new Vector2(bind.Offset.X, bind.Offset.Y),
                    };
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return binding;
        }
        #endregion
    }
}

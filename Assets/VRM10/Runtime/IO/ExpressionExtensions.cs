using System;
using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;

namespace UniVRM10
{
    public static class ExpressionExtensions
    {
        public static UniVRM10.MorphTargetBinding Build10(this MorphTargetBind bind, GameObject root, Vrm10Importer.ModelMap loader, VrmLib.Model model)
        {
            var libNode = model.Nodes[bind.Node.Value];
            var node = loader.Nodes[libNode].transform;
            var mesh = loader.Meshes[libNode.MeshGroup];
            var relativePath = node.RelativePathFrom(root.transform);
            return new UniVRM10.MorphTargetBinding(relativePath, bind.Index.Value, bind.Weight.Value);
        }

        public static UniVRM10.MaterialColorBinding? Build10(this MaterialColorBind bind, IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo> materials)
        {
            var value = new Vector4(bind.TargetValue[0], bind.TargetValue[1], bind.TargetValue[2], bind.TargetValue[3]);
            var material = materials[bind.Material.Value].Asset;

            var binding = default(UniVRM10.MaterialColorBinding?);
            if (material != null)
            {
                try
                {
                    binding = new UniVRM10.MaterialColorBinding
                    {
                        MaterialName = material.name, // 名前で持つべき？
                        BindType = bind.Type,
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

        public static UniVRM10.MaterialUVBinding? Build10(this TextureTransformBind bind, IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo> materials)
        {
            var material = materials[bind.Material.Value].Asset;

            var binding = default(UniVRM10.MaterialUVBinding?);
            if (material != null)
            {
                var (scale, offset) = UniGLTF.TextureTransform.VerticalFlipScaleOffset(new Vector2(bind.Scale[0], bind.Scale[1]), new Vector2(bind.Offset[0], bind.Offset[1]));

                try
                {
                    binding = new UniVRM10.MaterialUVBinding
                    {
                        MaterialName = material.name, // 名前で持つべき
                        Scaling = scale,
                        Offset = offset,
                    };
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return binding;
        }
    }
}

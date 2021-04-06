using System;
using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;

namespace UniVRM10
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// for SubAssetName
        /// </summary>
        /// <returns></returns>
        public static string ExtractKey(this Expression expression)
        {
            ExpressionKey key =
            (expression.Preset == ExpressionPreset.custom)
            ? ExpressionKey.CreateCustom(expression.Name)
            : ExpressionKey.CreateFromPreset(expression.Preset)
            ;
            return key.ExtractKey;
        }

        public static UniVRM10.MorphTargetBinding Build10(this MorphTargetBind bind, GameObject root, RuntimeUnityBuilder.ModelMap loader, VrmLib.Model model)
        {
            var libNode = model.Nodes[bind.Node.Value];
            var node = loader.Nodes[libNode].transform;
            var mesh = loader.Meshes[libNode.MeshGroup];
            var relativePath = node.RelativePathFrom(root.transform);
            // VRM-1.0 では値域は [0-1.0f]
            return new UniVRM10.MorphTargetBinding(relativePath, bind.Index.Value, bind.Weight.Value * 100.0f);
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
                try
                {
                    binding = new UniVRM10.MaterialUVBinding
                    {
                        MaterialName = material.name, // 名前で持つべき
                        Scaling = new Vector2(bind.Scaling[0], bind.Scaling[1]),
                        Offset = new Vector2(bind.Offset[0], bind.Offset[1]),
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

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
        public static string ExtractName(this Expression expression)
        {
            ExpressionKey key =
            (expression.Preset == ExpressionPreset.custom)
            ? ExpressionKey.CreateCustom(expression.Name)
            : ExpressionKey.CreateFromPreset(expression.Preset)
            ;
            return $"Expression.{key}";
        }

        public static UniVRM10.MorphTargetBinding Build10(this MorphTargetBind bind, GameObject root, ModelMap loader, VrmLib.Model model)
        {
            var libNode = model.Nodes[bind.Node.Value];
            var node = loader.Nodes[libNode].transform;
            var mesh = loader.Meshes[libNode.MeshGroup];
            var relativePath = node.RelativePathFrom(root.transform);
            // VRM-1.0 では値域は [0-1.0f]
            return new UniVRM10.MorphTargetBinding(relativePath, bind.Index.Value, bind.Weight.Value * 100.0f);
        }

        // static UniVRM10.MaterialColorBinding? Build10(this MaterialColorBind bind, ModelMap loader)
        // {
        //     var kv = bind.Property;
        //     var value = kv.Value.ToUnityVector4();
        //     var material = loader.Materials[bind.Material];

        //     var binding = default(UniVRM10.MaterialColorBinding?);
        //     if (material != null)
        //     {
        //         try
        //         {
        //             binding = new UniVRM10.MaterialColorBinding
        //             {
        //                 MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
        //                 BindType = bind.BindType,
        //                 TargetValue = value,
        //                 // BaseValue = material.GetColor(kv.Key),
        //             };
        //         }
        //         catch (Exception)
        //         {
        //             // do nothing
        //         }
        //     }
        //     return binding;
        // }

        // static UniVRM10.MaterialUVBinding? Build10(this VrmLib.TextureTransformBind bind, ModelMap loader)
        // {
        //     var material = loader.Materials[bind.Material];

        //     var binding = default(UniVRM10.MaterialUVBinding?);
        //     if (material != null)
        //     {
        //         try
        //         {
        //             binding = new UniVRM10.MaterialUVBinding
        //             {
        //                 MaterialName = bind.Material.Name, // UniVRM-0Xの実装は名前で持っている
        //                 Scaling = new Vector2(bind.Scale.X, bind.Scale.Y),
        //                 Offset = new Vector2(bind.Offset.X, bind.Offset.Y),
        //             };
        //         }
        //         catch (Exception)
        //         {
        //             // do nothing
        //         }
        //     }
        //     return binding;
        // }

    }
}

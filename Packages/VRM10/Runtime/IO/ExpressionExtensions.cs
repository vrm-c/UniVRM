using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniGLTF.Extensions.VRMC_vrm_expressions_node_transform;
using UnityEngine;

namespace UniVRM10
{
    public static class ExpressionExtensions
    {
        public static MorphTargetBinding? Build10(this MorphTargetBind bind, GameObject root, Vrm10Importer importer)
        {
            if (bind.Node.TryGetValidIndex(importer.Nodes.Count, out var nodeIndex))
            {
                var node = importer.Nodes[nodeIndex];
                var smr = node.GetComponent<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    return default;
                }
                var relativePath = node.RelativePathFrom(root.transform);
                return new MorphTargetBinding(relativePath, bind.Index.Value, bind.Weight.Value);
            }
            else
            {
                return default;
            }
        }

        public static UniVRM10.MaterialColorBinding? Build10(this MaterialColorBind bind, IReadOnlyList<MaterialFactory.MaterialLoadInfo> materials)
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

        public static UniVRM10.MaterialUVBinding? Build10(this TextureTransformBind bind, IReadOnlyList<MaterialFactory.MaterialLoadInfo> materials)
        {
            var material = materials[bind.Material.Value].Asset;

            var binding = default(UniVRM10.MaterialUVBinding?);
            if (material != null)
            {
                // Default values: scale [1, 1], offset [0, 0]
                Vector2 scaleVec = bind.Scale != null && bind.Scale.Length >= 2
                    ? new Vector2(bind.Scale[0], bind.Scale[1])
                    : new Vector2(1.0f, 1.0f);
                Vector2 offsetVec = bind.Offset != null && bind.Offset.Length >= 2
                    ? new Vector2(bind.Offset[0], bind.Offset[1])
                    : new Vector2(0.0f, 0.0f);

                var (scale, offset) = UniGLTF.TextureTransform.VerticalFlipScaleOffset(scaleVec, offsetVec);

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

        public static NodeTransformBinding? Build10(this NodeTransformBind bind, GameObject root, Vrm10Importer importer)
        {
            if (bind.Node.TryGetValidIndex(importer.Nodes.Count, out var nodeIndex))
            {
                var node = importer.Nodes[nodeIndex];
                var relativePath = node.RelativePathFrom(root.transform);

                var t = bind.Translation != null && bind.Translation.Length >= 3
                    ? new Vector3(bind.Translation[0], bind.Translation[1], bind.Translation[2])
                    : Vector3.zero;
                var r = bind.Rotation != null && bind.Rotation.Length >= 4
                    ? new Quaternion(bind.Rotation[0], bind.Rotation[1], bind.Rotation[2], bind.Rotation[3])
                    : Quaternion.identity;
                var s = bind.Scale != null && bind.Scale.Length >= 3
                    ? new Vector3(bind.Scale[0], bind.Scale[1], bind.Scale[2])
                    : Vector3.zero;
                return new NodeTransformBinding(relativePath, t, r, s);
            }
            else
            {
                return default;
            }
        }
    }
}

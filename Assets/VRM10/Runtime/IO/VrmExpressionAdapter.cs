using System;
using System.Collections.Generic;
using System.Numerics;
using VrmLib;

namespace UniVRM10
{
    public static class ExpressionAdapter
    {
        public static VrmLib.Expression FromGltf(this UniGLTF.Extensions.VRMC_vrm.Expression x, List<VrmLib.Node> nodes, List<VrmLib.Material> materials)
        {
            var expression = new VrmLib.Expression((VrmLib.ExpressionPreset)x.Preset,
                x.Name,
                x.IsBinary.HasValue && x.IsBinary.Value)
            {
                IgnoreBlink = x.IgnoreBlink.GetValueOrDefault(),
                IgnoreLookAt = x.IgnoreLookAt.GetValueOrDefault(),
                IgnoreMouth = x.IgnoreMouth.GetValueOrDefault(),
            };

            if (x.MorphTargetBinds != null)
            {
                foreach (var y in x.MorphTargetBinds)
                {
                    var node = nodes[y.Node.Value];
                    var blendShapeName = node.Mesh.MorphTargets[y.Index.Value].Name;
                    var blendShapeBind = new MorphTargetBind(node, blendShapeName, y.Weight.Value);
                    expression.MorphTargetBinds.Add(blendShapeBind);
                }
            }

            if (x.MaterialColorBinds != null)
            {
                foreach (var y in x.MaterialColorBinds)
                {
                    var material = materials[y.Material.Value];
                    var materialColorBind = new MaterialColorBind(material, EnumUtil.Cast<MaterialBindType>(y.Type), y.TargetValue.ToVector4(Vector4.Zero));
                    expression.MaterialColorBinds.Add(materialColorBind);
                }
            }

            if (x.TextureTransformBinds != null)
            {
                foreach (var y in x.TextureTransformBinds)
                {
                    var material = materials[y.Material.Value];
                    var materialUVBind = new TextureTransformBind(material,
                        y.Scaling.ToVector2(Vector2.One),
                        y.Offset.ToVector2(Vector2.Zero));
                    expression.TextureTransformBinds.Add(materialUVBind);
                }
            }

            return expression;
        }
        // public static ExpressionManager FromGltf(this UniGLTF.VRMC_vrm.Expression master, List<VrmLib.Node> nodes, List<VrmLib.Material> materials)
        // {
        //     var manager = new ExpressionManager();
        //     foreach (var x in master.BlendShapeGroups)
        //     {
        //         VrmLib.Expression expression = FromGltf(x, nodes, materials);

        //         manager.ExpressionList.Add(expression);
        //     };
        //     return manager;
        // }

        public static UniGLTF.Extensions.VRMC_vrm.MorphTargetBind ToGltf(this MorphTargetBind self, List<VrmLib.Node> nodes)
        {
            var name = self.Name;
            var value = self.Value;
            var index = self.Node.Mesh.MorphTargets.FindIndex(x => x.Name == name);
            if (index < 0)
            {
                throw new IndexOutOfRangeException(string.Format("MorphTargetName {0} is not found", name));
            }

            return new UniGLTF.Extensions.VRMC_vrm.MorphTargetBind
            {
                Node = nodes.IndexOfThrow(self.Node),
                Index = self.Node.Mesh.MorphTargets.FindIndex(x => x.Name == name),
                Weight = value,
            };
        }

        public static UniGLTF.Extensions.VRMC_vrm.MaterialColorBind ToGltf(this MaterialColorBind self, List<VrmLib.Material> materials)
        {
            var m = new UniGLTF.Extensions.VRMC_vrm.MaterialColorBind
            {
                Material = materials.IndexOfThrow(self.Material),
                Type = EnumUtil.Cast<UniGLTF.Extensions.VRMC_vrm.MaterialColorType>(self.BindType),
                TargetValue = self.Property.Value.ToFloat4()
            };
            return m;
        }

        public static UniGLTF.Extensions.VRMC_vrm.TextureTransformBind ToGltf(this TextureTransformBind self, List<VrmLib.Material> materials)
        {
            var m = new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
            {
                Material = materials.IndexOfThrow(self.Material),
                Scaling = self.Scale.ToFloat2(),
                Offset = self.Offset.ToFloat2(),
            };
            return m;
        }

        public static UniGLTF.Extensions.VRMC_vrm.Expression ToGltf(this VrmLib.Expression x, List<VrmLib.Node> nodes, List<VrmLib.Material> materials)
        {
            var g = new UniGLTF.Extensions.VRMC_vrm.Expression
            {
                Preset = (UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)x.Preset,
                Name = x.Name,
                IsBinary = x.IsBinary,
                IgnoreBlink = x.IgnoreBlink,
                IgnoreLookAt = x.IgnoreLookAt,
                IgnoreMouth = x.IgnoreMouth,
            };
            foreach (var blendShapeBind in x.MorphTargetBinds)
            {
                g.MorphTargetBinds.Add(blendShapeBind.ToGltf(nodes));
            }
            foreach (var materialColorBind in x.MaterialColorBinds)
            {
                g.MaterialColorBinds.Add(materialColorBind.ToGltf(materials));
            }
            foreach (var materialUVBind in x.TextureTransformBinds)
            {
                g.TextureTransformBinds.Add(materialUVBind.ToGltf(materials));
            }
            return g;
        }

        // public static UniGLTF.VRMC_vrm.BlendShape ToGltf(this ExpressionManager src, List<VrmLib.Node> nodes, List<VrmLib.Material> materials)
        // {
        //     var blendShape = new UniGLTF.VRMC_vrm.BlendShape
        //     {
        //     };
        //     if (src != null)
        //     {
        //         foreach (var x in src.ExpressionList)
        //         {
        //             blendShape.BlendShapeGroups.Add(x.ToGltf(nodes, materials));
        //         }
        //     }
        //     return blendShape;
        // }
    }
}

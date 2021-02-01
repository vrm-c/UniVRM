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
            var expression = new VrmLib.Expression(x.Preset.ToVrmFormat(),
                x.Name,
                x.IsBinary.HasValue && x.IsBinary.Value)
            {
                OverrideBlink = EnumUtil.Cast<VrmLib.ExpressionOverrideType>(x.OverrideBlink),
                OverrideLookAt = EnumUtil.Cast<VrmLib.ExpressionOverrideType>(x.OverrideLookAt),
                OverrideMouth = EnumUtil.Cast<VrmLib.ExpressionOverrideType>(x.OverrideMouth),
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
                Preset = x.Preset.ToGltfFormat(),
                Name = x.Name,
                IsBinary = x.IsBinary,
                OverrideBlink = EnumUtil.Cast<UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType>(x.OverrideBlink),
                OverrideLookAt = EnumUtil.Cast<UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType>(x.OverrideLookAt),
                OverrideMouth = EnumUtil.Cast<UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType>(x.OverrideMouth),
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

        private static UniGLTF.Extensions.VRMC_vrm.ExpressionPreset ToGltfFormat(this VrmLib.ExpressionPreset preset)
        {
            switch (preset)
            {
                case ExpressionPreset.Custom:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom;
                case ExpressionPreset.Aa:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa;
                case ExpressionPreset.Ih:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih;
                case ExpressionPreset.Ou:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou;
                case ExpressionPreset.Ee:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee;
                case ExpressionPreset.Oh:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh;
                case ExpressionPreset.Blink:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blink;
                case ExpressionPreset.Joy:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.happy;
                case ExpressionPreset.Angry:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.angry;
                case ExpressionPreset.Sorrow:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.sad;
                case ExpressionPreset.Fun:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.relaxed;
                case ExpressionPreset.LookUp:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookUp;
                case ExpressionPreset.LookDown:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookDown;
                case ExpressionPreset.LookLeft:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookLeft;
                case ExpressionPreset.LookRight:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookRight;
                case ExpressionPreset.BlinkLeft:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkLeft;
                case ExpressionPreset.BlinkRight:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkRight;
                case ExpressionPreset.Neutral:
                    return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.neutral;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

        private static VrmLib.ExpressionPreset ToVrmFormat(this UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset)
        {
            switch (preset)
            {
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom: return ExpressionPreset.Custom;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa: return ExpressionPreset.Aa;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih: return ExpressionPreset.Ih;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou: return ExpressionPreset.Ou;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee: return ExpressionPreset.Ee;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh: return ExpressionPreset.Oh;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blink: return ExpressionPreset.Blink;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.happy: return ExpressionPreset.Joy;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.angry: return ExpressionPreset.Angry;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.sad: return ExpressionPreset.Sorrow;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.relaxed: return ExpressionPreset.Fun;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookUp: return ExpressionPreset.LookUp;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.surprised:
                    throw new NotImplementedException();
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookDown: return ExpressionPreset.LookDown;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookLeft: return ExpressionPreset.LookLeft;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookRight: return ExpressionPreset.LookRight;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkLeft: return ExpressionPreset.BlinkLeft;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkRight: return ExpressionPreset.BlinkRight;
                case UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.neutral: return ExpressionPreset.Neutral;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmExpression
    {
        static UniGLTF.Extensions.VRMC_vrm.ExpressionPreset ToPreset(JsonNode json)
        {
            switch (json.GetString().ToLower())
            {
                case "unknown": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom;

                // https://github.com/vrm-c/vrm-specification/issues/185
                case "neutral": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.neutral;

                case "a": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.aa;
                case "i": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ih;
                case "u": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ou;
                case "e": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.ee;
                case "o": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.oh;

                case "blink": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blink;
                case "blink_l": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkLeft;
                case "blink_r": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.blinkRight;

                // https://github.com/vrm-c/vrm-specification/issues/163
                case "joy": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.happy;
                case "angry": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.angry;
                case "sorrow": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.sad;
                case "fun": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.relaxed;

                case "lookup": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookUp;
                case "lookdown": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookDown;
                case "lookleft": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookLeft;
                case "lookright": return UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.lookRight;
            }

            throw new NotImplementedException();
        }

        static IEnumerable<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind> ToMorphTargetBinds(UniGLTF.glTF gltf, JsonNode json)
        {
            foreach (var x in json.ArrayItems())
            {
                var meshIndex = x["mesh"].GetInt32();
                var morphTargetIndex = x["index"].GetInt32();
                var weight = x["weight"].GetSingle();

                var bind = new UniGLTF.Extensions.VRMC_vrm.MorphTargetBind();

                // https://github.com/vrm-c/vrm-specification/pull/106
                // https://github.com/vrm-c/vrm-specification/pull/153
                bind.Node = gltf.nodes.IndexOf(gltf.nodes.First(y => y.mesh == meshIndex));
                bind.Index = morphTargetIndex;
                // https://github.com/vrm-c/vrm-specification/issues/209                
                bind.Weight = weight * 0.01f;

                yield return bind;
            }
        }

        public const string COLOR_PROPERTY = "_Color";
        public const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
        public const string RIM_COLOR_PROPERTY = "_RimColor";
        public const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
        public const string SHADE_COLOR_PROPERTY = "_ShadeColor";

        static UniGLTF.Extensions.VRMC_vrm.MaterialColorType ToMaterialType(string src)
        {
            switch (src)
            {
                case COLOR_PROPERTY:
                    return UniGLTF.Extensions.VRMC_vrm.MaterialColorType.color;

                case EMISSION_COLOR_PROPERTY:
                    return UniGLTF.Extensions.VRMC_vrm.MaterialColorType.emissionColor;

                case RIM_COLOR_PROPERTY:
                    return UniGLTF.Extensions.VRMC_vrm.MaterialColorType.rimColor;

                case SHADE_COLOR_PROPERTY:
                    return UniGLTF.Extensions.VRMC_vrm.MaterialColorType.shadeColor;

                case OUTLINE_COLOR_PROPERTY:
                    return UniGLTF.Extensions.VRMC_vrm.MaterialColorType.outlineColor;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// MaterialValue の仕様変更
        /// 
        /// * MaterialColorBind
        /// * TextureTransformBind
        /// 
        /// の２種類になった。
        /// 
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="json"></param>
        /// <param name="expression"></param>
        static void ToMaterialColorBinds(UniGLTF.glTF gltf, JsonNode json, UniGLTF.Extensions.VRMC_vrm.Expression expression)
        {
            foreach (var x in json.ArrayItems())
            {
                var materialName = x["materialName"].GetString();
                var materialIndex = gltf.materials.IndexOf(gltf.materials.First(y => y.name == materialName));
                var propertyName = x["propertyName"].GetString();
                var targetValue = x["targetValue"].ArrayItems().Select(y => y.GetSingle()).ToArray();
                if (propertyName.EndsWith("_ST"))
                {
                    var scaling = new float[] { targetValue[0], targetValue[1] };
                    expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                    {
                        Material = materialIndex,
                        Scaling = new float[] { targetValue[0], targetValue[1] },
                        Offset = new float[] { targetValue[2], targetValue[3] }
                    });
                }
                else if (propertyName.EndsWith("_ST_S"))
                {
                    expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                    {
                        Material = materialIndex,
                        Scaling = new float[] { targetValue[0], 1 },
                        Offset = new float[] { targetValue[2], 0 }
                    });
                }
                else if (propertyName.EndsWith("_ST_T"))
                {
                    expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                    {
                        Material = materialIndex,
                        Scaling = new float[] { 1, targetValue[1] },
                        Offset = new float[] { 0, targetValue[3] }
                    });
                }
                else
                {
                    // color
                    expression.MaterialColorBinds.Add(new UniGLTF.Extensions.VRMC_vrm.MaterialColorBind
                    {
                        Material = materialIndex,
                        Type = ToMaterialType(propertyName),
                        TargetValue = targetValue,
                    });
                }
            }
        }

        public static IEnumerable<UniGLTF.Extensions.VRMC_vrm.Expression> Migrate(UniGLTF.glTF gltf, JsonNode json)
        {
            foreach (var blendShapeClip in json["blendShapeGroups"].ArrayItems())
            {
                var name = blendShapeClip["name"].GetString();
                var isBinary = false;
                if (blendShapeClip.TryGet("isBinary", out JsonNode isBinaryNode))
                {
                    isBinary = isBinaryNode.GetBoolean();
                }
                var expression = new UniGLTF.Extensions.VRMC_vrm.Expression
                {
                    Name = name,
                    Preset = ToPreset(blendShapeClip["presetName"]),
                    IsBinary = isBinary,
                    MorphTargetBinds = new List<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind>(),
                    MaterialColorBinds = new List<UniGLTF.Extensions.VRMC_vrm.MaterialColorBind>(),
                    TextureTransformBinds = new List<UniGLTF.Extensions.VRMC_vrm.TextureTransformBind>(),
                };
                expression.MorphTargetBinds = ToMorphTargetBinds(gltf, blendShapeClip["binds"]).ToList();

                if (blendShapeClip.TryGet("materialValues", out JsonNode materialValues))
                {
                    ToMaterialColorBinds(gltf, materialValues, expression);
                }

                yield return expression;
            }
        }
    }
}

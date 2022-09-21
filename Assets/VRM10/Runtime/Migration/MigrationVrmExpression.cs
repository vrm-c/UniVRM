using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    internal static class MigrationVrmExpression
    {
        /// <summary>
        /// preset 名の文字列から ExpressionPreset を確定させる。
        ///
        /// 0.x の特殊な挙動として、
        /// preset名が "unknown" の場合に、
        /// "name" を preset 名として解釈を試みる。
        /// </summary>
        /// <param name="json"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static ExpressionPreset ToPreset(JsonNode json, string name)
        {
            var src = json.GetString().ToLowerInvariant();
            if (src == "unknown")
            {
                // VRM 0.x では以下の実装が存在した。
                // Preset が unknown で Name が Joy の表情があったときに
                // それは Preset が Joy であるとみなしてロードしていた。
                src = name.ToLowerInvariant();
            }

            switch (src)
            {
                // https://github.com/vrm-c/vrm-specification/issues/185
                // https://github.com/vrm-c/UniVRM/issues/1482
                case "neutral": return ExpressionPreset.neutral;

                case "a": return ExpressionPreset.aa;
                case "i": return ExpressionPreset.ih;
                case "u": return ExpressionPreset.ou;
                case "e": return ExpressionPreset.ee;
                case "o": return ExpressionPreset.oh;

                case "blink": return ExpressionPreset.blink;
                case "blink_l": return ExpressionPreset.blinkLeft;
                case "blink_r": return ExpressionPreset.blinkRight;

                // https://github.com/vrm-c/vrm-specification/issues/163
                case "joy": return ExpressionPreset.happy;
                case "angry": return ExpressionPreset.angry;
                case "sorrow": return ExpressionPreset.sad;
                case "fun": return ExpressionPreset.relaxed;
                case "surprised": return ExpressionPreset.surprised;

                case "lookup": return ExpressionPreset.lookUp;
                case "lookdown": return ExpressionPreset.lookDown;
                case "lookleft": return ExpressionPreset.lookLeft;
                case "lookright": return ExpressionPreset.lookRight;
            }

            return ExpressionPreset.custom;
        }

        static IEnumerable<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind> ToMorphTargetBinds(JsonNode json,
            MigrationVrm.MeshIndexToNodeIndexFunc meshToNode)
        {
            foreach (var x in json.ArrayItems())
            {
                var meshIndex = x["mesh"].GetInt32();
                var morphTargetIndex = x["index"].GetInt32();
                var weight = x["weight"].GetSingle();

                var bind = new UniGLTF.Extensions.VRMC_vrm.MorphTargetBind();

                // https://github.com/vrm-c/vrm-specification/pull/106
                // https://github.com/vrm-c/vrm-specification/pull/153
                var nodeIndex = meshToNode(meshIndex);
                if (nodeIndex == -1)
                {
                    // invalid data. skip
                    Debug.LogWarning($"[MigrationVrmExpression] node.mesh == {meshIndex} not found");
                    continue;
                }
                bind.Node = nodeIndex;
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

        static UniGLTF.Extensions.VRMC_vrm.MaterialColorType? ToMaterialColorType(string src)
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

            return default;
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
                var material = gltf.materials.FirstOrDefault(y => y.name == materialName);
                if (material == null)
                {
                    // invalid data. skip
                    Debug.LogWarning($"[MigrationVrmExpression] material.name == {materialName} is not found");
                    continue;
                }
                var materialIndex = gltf.materials.IndexOf(material);
                if (materialIndex == -1)
                {
                    // invalid data. skip
                    Debug.LogWarning($"[MigrationVrmExpression] material.name == {materialName} index");
                    continue;
                }
                var propertyName = x["propertyName"].GetString();
                var targetValue = x["targetValue"].ArrayItems().Select(y => y.GetSingle()).ToArray();
                if (propertyName == "_MainTex_ST")
                {
                    // VRM-0 は無変換
                    var (scale, offset) = UniGLTF.TextureTransform.VerticalFlipScaleOffset(
                        new UnityEngine.Vector2(targetValue[0], targetValue[1]),
                        new UnityEngine.Vector2(targetValue[2], targetValue[3]));

                    if (!expression.TextureTransformBinds.Exists(bind => bind.Material == materialIndex))
                    {
                        expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                        {
                            Material = materialIndex,
                            Scale = new float[] { scale.x, scale.y },
                            Offset = new float[] { offset.x, offset.y }
                        });
                    }
                }
                else if (propertyName == "_MainTex_ST_S")
                {
                    // VRM-0 は無変換
                    var (scale, offset) = UniGLTF.TextureTransform.VerticalFlipScaleOffset(
                        new UnityEngine.Vector2(targetValue[0], 1),
                        new UnityEngine.Vector2(targetValue[2], 0));

                    if (!expression.TextureTransformBinds.Exists(bind => bind.Material == materialIndex))
                    {
                        expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                        {
                            Material = materialIndex,
                            Scale = new float[] { scale.x, scale.y },
                            Offset = new float[] { offset.x, offset.y }
                        });
                    }
                }
                else if (propertyName == "_MainTex_ST_T")
                {
                    // VRM-0 は無変換
                    var (scale, offset) = UniGLTF.TextureTransform.VerticalFlipScaleOffset(
                        new UnityEngine.Vector2(1, targetValue[1]),
                        new UnityEngine.Vector2(0, targetValue[3]));

                    if (!expression.TextureTransformBinds.Exists(bind => bind.Material == materialIndex))
                    {
                        expression.TextureTransformBinds.Add(new UniGLTF.Extensions.VRMC_vrm.TextureTransformBind
                        {
                            Material = materialIndex,
                            Scale = new float[] { scale.x, scale.y },
                            Offset = new float[] { offset.x, offset.y }
                        });
                    }
                }
                else
                {
                    var materialColorType = ToMaterialColorType(propertyName);
                    if (materialColorType.HasValue)
                    {
                        // color
                        expression.MaterialColorBinds.Add(new UniGLTF.Extensions.VRMC_vrm.MaterialColorBind
                        {
                            Material = materialIndex,
                            Type = materialColorType.Value,
                            TargetValue = targetValue,
                        });
                    }
                }
            }
        }

        public static IEnumerable<(ExpressionPreset, string, UniGLTF.Extensions.VRMC_vrm.Expression)> Migrate(UniGLTF.glTF gltf, JsonNode json,
            MigrationVrm.MeshIndexToNodeIndexFunc meshToNode)
        {
            foreach (var blendShapeClip in json["blendShapeGroups"].ArrayItems())
            {
                var name = blendShapeClip["name"].GetString();
                var isBinary = false;
                if (blendShapeClip.TryGet("isBinary", out JsonNode isBinaryNode))
                {
                    isBinary = isBinaryNode.GetBoolean();
                }
                var preset = ToPreset(blendShapeClip["presetName"], name);
                var expression = new UniGLTF.Extensions.VRMC_vrm.Expression
                {
                    IsBinary = isBinary,
                    MorphTargetBinds = new List<UniGLTF.Extensions.VRMC_vrm.MorphTargetBind>(),
                    MaterialColorBinds = new List<UniGLTF.Extensions.VRMC_vrm.MaterialColorBind>(),
                    TextureTransformBinds = new List<UniGLTF.Extensions.VRMC_vrm.TextureTransformBind>(),
                };
                expression.MorphTargetBinds = ToMorphTargetBinds(blendShapeClip["binds"], meshToNode).ToList();

                if (blendShapeClip.TryGet("materialValues", out JsonNode materialValues))
                {
                    ToMaterialColorBinds(gltf, materialValues, expression);
                }

                yield return (preset, name, expression);
            }
        }

        static void Check(string name, JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Expression vrm1,
            MigrationVrm.MeshIndexToNodeIndexFunc meshToNode)
        {
            if (vrm0["binds"].GetArrayCount() == 0)
            {
                if (vrm1.MorphTargetBinds == null)
                {
                    // OK
                    return;
                }
                else
                {
                    throw new MigrationException($"expression.{name}.binds", "different count");
                }
            }

            foreach (var (l, r) in Enumerable.Zip(vrm0["binds"].ArrayItems(), vrm1.MorphTargetBinds, (x, y) => (x, y)))
            {
                var mesh = l["mesh"].GetInt32();
                var node = meshToNode(mesh);
                if (node != r.Node)
                {
                    throw new MigrationException($"expression.{name}.binds.node", $"{node} != {r.Node}");
                }

                var index = l["index"].GetInt32();
                if (index != r.Index)
                {
                    throw new MigrationException($"expression.{name}.binds.index", $"{index} != {r.Index}");
                }

                var weight = l["weight"].GetSingle() * 0.01f; // [0, 100] to [0, 1.0f]
                if (weight != r.Weight)
                {
                    throw new MigrationException($"expression.{name}.binds.weight", $"{weight} != {r.Weight}");
                }
            }
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.Expressions vrm1,
            MigrationVrm.MeshIndexToNodeIndexFunc meshToNode)
        {
            foreach (var blendShape in vrm0["blendShapeGroups"].ArrayItems())
            {
                var name = blendShape["presetName"].GetString().ToLower();
                switch (name)
                {
                    case "a": Check(name, blendShape, vrm1.Preset.Aa, meshToNode); break;
                    case "i": Check(name, blendShape, vrm1.Preset.Ih, meshToNode); break;
                    case "u": Check(name, blendShape, vrm1.Preset.Ou, meshToNode); break;
                    case "e": Check(name, blendShape, vrm1.Preset.Ee, meshToNode); break;
                    case "o": Check(name, blendShape, vrm1.Preset.Oh, meshToNode); break;
                    case "blink": Check(name, blendShape, vrm1.Preset.Blink, meshToNode); break;
                    case "joy": Check(name, blendShape, vrm1.Preset.Happy, meshToNode); break;
                    case "angry": Check(name, blendShape, vrm1.Preset.Angry, meshToNode); break;
                    case "sorrow": Check(name, blendShape, vrm1.Preset.Sad, meshToNode); break;
                    case "fun": Check(name, blendShape, vrm1.Preset.Relaxed, meshToNode); break;
                    case "lookup": Check(name, blendShape, vrm1.Preset.LookUp, meshToNode); break;
                    case "lookdown": Check(name, blendShape, vrm1.Preset.LookDown, meshToNode); break;
                    case "lookleft": Check(name, blendShape, vrm1.Preset.LookLeft, meshToNode); break;
                    case "lookright": Check(name, blendShape, vrm1.Preset.LookRight, meshToNode); break;
                    case "blink_l": Check(name, blendShape, vrm1.Preset.BlinkLeft, meshToNode); break;
                    case "blink_r": Check(name, blendShape, vrm1.Preset.BlinkRight, meshToNode); break;
                    case "neutral": Check(name, blendShape, vrm1.Preset.Neutral, meshToNode); break;
                    default:
                        {
                            string found = default;
                            foreach (var kv in vrm1.Custom)
                            {
                                if (kv.Key.ToLower() == name)
                                {
                                    Check(name, blendShape, kv.Value, meshToNode);
                                    found = kv.Key;
                                    break;
                                }
                            }
                            if (found == null)
                            {
                                throw new MigrationException(name, $"expression not migrated");
                            }
                            break;
                        }
                }
            }
        }
    }
}

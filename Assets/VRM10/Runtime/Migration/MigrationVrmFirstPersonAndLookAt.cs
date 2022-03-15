using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmLookAtAndFirstPerson
    {
        private static LookAtRangeMap MigrateLookAtRangeMap(JsonNode vrm0, string key, float defaultXRange, float defaultYRange)
        {
            // NOTE: Curve は VRM 1.0 では廃止されるため, 考慮しません.
            if (vrm0.TryGet(key, out var curveMapperNode) &&
                curveMapperNode.TryGet("xRange", out var xRangeNode) &&
                curveMapperNode.TryGet("yRange", out var yRangeNode))
            {
                return new LookAtRangeMap
                {
                    InputMaxValue = xRangeNode.GetSingle(),
                    OutputScale = yRangeNode.GetSingle(),
                };
            }

            return new LookAtRangeMap
            {
                InputMaxValue = defaultXRange,
                OutputScale = defaultYRange,
            };
        }

        private static LookAtType MigrateLookAtType(JsonNode vrm0, string key)
        {
            if (vrm0.TryGet(key, out var lookAtTypeStringNode))
            {
                switch (lookAtTypeStringNode.GetString().ToLowerInvariant())
                {
                    case "bone":
                        return LookAtType.bone;
                    case "blendshape":
                        return LookAtType.expression;
                }
            }

            return LookAtType.bone;
        }

        private static FirstPersonType MigrateFirstPersonType(JsonNode vrm0, string key)
        {
            if (vrm0.TryGet(key, out var firstPersonTypeStringNode))
            {
                switch (firstPersonTypeStringNode.GetString().ToLowerInvariant())
                {
                    case "auto":
                        return FirstPersonType.auto;
                    case "both":
                        return FirstPersonType.both;
                    case "thirdpersononly":
                        return FirstPersonType.thirdPersonOnly;
                    case "firstpersononly":
                        return FirstPersonType.firstPersonOnly;
                }
            }

            return FirstPersonType.auto;
        }

        private static int? MigrateFirstPersonMeshIndex(JsonNode vrm0, string key, glTF gltf)
        {
            if (vrm0.TryGet(key, out var meshIndexNode))
            {
                var meshIndex = meshIndexNode.GetInt32();
                return FindRenderNodeIndexFromMeshIndex(gltf, meshIndex);
            }

            return default;
        }

        public static (LookAt, FirstPerson) Migrate(glTF gltf, JsonNode vrm0)
        {
            // VRM1
            // firstPerson に同居していた LookAt は独立します
            var lookAtType = MigrateLookAtType(vrm0, "lookAtTypeName");
            var defaultXRangeValue = 90f;
            var defaultYRangeValue = GetDefaultCurveMapperYRangeValue(lookAtType);
            var lookAt = new LookAt
            {
                Type = lookAtType,
                RangeMapHorizontalInner = MigrateLookAtRangeMap(vrm0, "lookAtHorizontalInner", defaultXRangeValue, defaultYRangeValue),
                RangeMapHorizontalOuter = MigrateLookAtRangeMap(vrm0, "lookAtHorizontalOuter", defaultXRangeValue, defaultYRangeValue),
                RangeMapVerticalDown = MigrateLookAtRangeMap(vrm0, "lookAtVerticalDown", defaultXRangeValue, defaultYRangeValue),
                RangeMapVerticalUp = MigrateLookAtRangeMap(vrm0, "lookAtVerticalUp", defaultXRangeValue, defaultYRangeValue),
                OffsetFromHeadBone = MigrateVector3.Migrate(vrm0, "firstPersonBoneOffset"),
            };

            var firstPerson = new FirstPerson
            {
                // VRM1
                // firstPersonBoneOffset は廃止されます。LookAt.OffsetFromHeadBone を使ってください。
                // firstPersonBone は廃止されます。Head 固定です。
                MeshAnnotations = new List<MeshAnnotation>(),
            };
            if (vrm0.TryGet("meshAnnotations", out var meshAnnotationArrayNode))
            {
                foreach (var x in meshAnnotationArrayNode.ArrayItems())
                {
                    var renderNodeIndex = MigrateFirstPersonMeshIndex(x, "mesh", gltf);
                    if (renderNodeIndex.HasValue)
                    {
                        firstPerson.MeshAnnotations.Add(new MeshAnnotation
                        {
                            Node = renderNodeIndex.Value,
                            Type = MigrateFirstPersonType(x, "firstPersonFlag"),
                        });
                    }
                }
            };

            return (lookAt, firstPerson);
        }

        private static int? FindRenderNodeIndexFromMeshIndex(glTF gltf, int meshIndex)
        {
            for (var i = 0; i < gltf.nodes.Count; ++i)
            {
                var node = gltf.nodes[i];
                if (node.mesh == meshIndex)
                {
                    return i;
                }
            }

            // NOTE: VRM をベースに改造した VRM モデルなど、Renderer の増減に対して FirstPerson の設定が追従しないまま null が出力されていることが多い.
            return default;
        }

        private static float GetDefaultCurveMapperYRangeValue(LookAtType type)
        {
            switch (type)
            {
                case LookAtType.bone:
                    return 10f;
                case LookAtType.expression:
                    return 1f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}

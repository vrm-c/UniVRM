using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmLookAtAndFirstPerson
    {
        private static LookAtRangeMap MigrateLookAtRangeMap(JsonNode firstPersonJsonNode, string key, float defaultXRange, float defaultYRange)
        {
            // NOTE: Curve は VRM 1.0 では廃止されるため, 考慮しません.
            if (firstPersonJsonNode.TryGet(key, out var curveMapperJsonNode) &&
                curveMapperJsonNode.TryGet("xRange", out var xRangeJsonNode) &&
                curveMapperJsonNode.TryGet("yRange", out var yRangeJsonNode))
            {
                return new LookAtRangeMap
                {
                    InputMaxValue = xRangeJsonNode.GetSingle(),
                    OutputScale = yRangeJsonNode.GetSingle(),
                };
            }

            return new LookAtRangeMap
            {
                InputMaxValue = defaultXRange,
                OutputScale = defaultYRange,
            };
        }

        private static LookAtType MigrateLookAtType(JsonNode firstPersonJsonNode, string key)
        {
            if (firstPersonJsonNode.TryGet(key, out var lookAtTypeStringJsonNode))
            {
                switch (lookAtTypeStringJsonNode.GetString().ToLowerInvariant())
                {
                    case "bone":
                        return LookAtType.bone;
                    case "blendshape":
                        return LookAtType.expression;
                }
            }

            return LookAtType.bone;
        }

        private static FirstPersonType MigrateFirstPersonType(JsonNode meshAnnotationJsonNode, string key)
        {
            if (meshAnnotationJsonNode.TryGet(key, out var firstPersonTypeStringJsonNode))
            {
                switch (firstPersonTypeStringJsonNode.GetString().ToLowerInvariant())
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

        private static int? MigrateFirstPersonMeshIndex(JsonNode meshAnnotationJsonNode, string key, glTF gltf)
        {
            if (meshAnnotationJsonNode.TryGet(key, out var meshIndexJsonNode))
            {
                var meshIndex = meshIndexJsonNode.GetInt32();

                // NOTE: VRM 1.0 では glTF の Node Index を記録するため、それに変換する.
                // TODO: mesh が共有されたノードの場合はどうなる？ 0x の場合はどうなっていたかを調べて挙動を追従する.
                for (var gltfNodeIndex = 0; gltfNodeIndex < gltf.nodes.Count; ++gltfNodeIndex)
                {
                    var node = gltf.nodes[gltfNodeIndex];
                    if (node.mesh == meshIndex)
                    {
                        return gltfNodeIndex;
                    }
                }
            }

            // NOTE: VRM をベースに改造した VRM モデルなど、Renderer の増減に対して FirstPerson の設定が追従しないまま null が出力されていることが多い.
            return default;
        }

        public static (LookAt, FirstPerson) Migrate(glTF gltf, JsonNode firstPersonJsonNode)
        {
            // NOTE: VRM 1.0 では, LookAt の情報は FirstPerson から独立した型に保存されます.
            var lookAtType = MigrateLookAtType(firstPersonJsonNode, "lookAtTypeName");
            var defaultXRangeValue = 90f;
            var defaultYRangeValue = GetDefaultCurveMapperYRangeValue(lookAtType);
            var lookAt = new LookAt
            {
                Type = lookAtType,
                RangeMapHorizontalInner = MigrateLookAtRangeMap(firstPersonJsonNode, "lookAtHorizontalInner", defaultXRangeValue, defaultYRangeValue),
                RangeMapHorizontalOuter = MigrateLookAtRangeMap(firstPersonJsonNode, "lookAtHorizontalOuter", defaultXRangeValue, defaultYRangeValue),
                RangeMapVerticalDown = MigrateLookAtRangeMap(firstPersonJsonNode, "lookAtVerticalDown", defaultXRangeValue, defaultYRangeValue),
                RangeMapVerticalUp = MigrateLookAtRangeMap(firstPersonJsonNode, "lookAtVerticalUp", defaultXRangeValue, defaultYRangeValue),
                OffsetFromHeadBone = MigrateVector3.Migrate(firstPersonJsonNode, "firstPersonBoneOffset"),
            };

            var firstPerson = new FirstPerson
            {
                // NOTE: VRM 1.0 では firstPersonBone は廃止され, Head Bone 固定になります.
                // NOTE: VRM 1.0 では firstPersonBoneOffset は FirstPerson 拡張ではなく LookAt 拡張の OffsetFromHeadBone に移行します.
                MeshAnnotations = new List<MeshAnnotation>(),
            };
            if (firstPersonJsonNode.TryGet("meshAnnotations", out var meshAnnotationArrayJsonNode))
            {
                foreach (var meshAnnotationJsonNode in meshAnnotationArrayJsonNode.ArrayItems())
                {
                    var renderNodeIndex = MigrateFirstPersonMeshIndex(meshAnnotationJsonNode, "mesh", gltf);
                    if (renderNodeIndex.HasValue)
                    {
                        firstPerson.MeshAnnotations.Add(new MeshAnnotation
                        {
                            Node = renderNodeIndex.Value,
                            Type = MigrateFirstPersonType(meshAnnotationJsonNode, "firstPersonFlag"),
                        });
                    }
                }
            };

            return (lookAt, firstPerson);
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

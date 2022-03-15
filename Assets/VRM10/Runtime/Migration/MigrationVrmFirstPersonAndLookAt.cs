using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    public static class MigrationVrmLookAtAndFirstPerson
    {
        static LookAtRangeMap MigrateLookAtRangeMap(JsonNode vrm0)
        {
            // VRM1
            // curve は廃止されます
            return new LookAtRangeMap
            {
                InputMaxValue = vrm0["xRange"].GetSingle(),
                OutputScale = vrm0["yRange"].GetSingle(),
            };
        }

        private static LookAtType MigrateLookAtType(JsonNode vrm0)
        {
            var typeString = vrm0.GetString().ToLowerInvariant();
            switch (typeString)
            {
                case "bone":
                    return LookAtType.bone;
                case "blendshape":
                    return LookAtType.expression;
                default:
                    Debug.LogWarning($"Unknown {nameof(LookAtType)}: {typeString}");
                    return LookAtType.bone;
            }
        }

        private static FirstPersonType MigrateFirstPersonType(JsonNode vrm0)
        {
            switch (vrm0.GetString().ToLowerInvariant())
            {
                case "auto":
                    return FirstPersonType.auto;
                case "both":
                    return FirstPersonType.both;
                case "thirdpersononly":
                    return FirstPersonType.thirdPersonOnly;
                case "firstpersononly":
                    return FirstPersonType.firstPersonOnly;
                default:
                    return FirstPersonType.auto;
            }
        }

        public static (LookAt, FirstPerson) Migrate(glTF gltf, JsonNode vrm0)
        {
            // VRM1
            // firstPerson に同居していた LookAt は独立します
            LookAt lookAt = default;
            lookAt = new LookAt
            {
                RangeMapHorizontalInner = MigrateLookAtRangeMap(vrm0["lookAtHorizontalInner"]),
                RangeMapHorizontalOuter = MigrateLookAtRangeMap(vrm0["lookAtHorizontalOuter"]),
                RangeMapVerticalDown = MigrateLookAtRangeMap(vrm0["lookAtVerticalDown"]),
                RangeMapVerticalUp = MigrateLookAtRangeMap(vrm0["lookAtVerticalUp"]),
                Type = MigrateLookAtType(vrm0["lookAtTypeName"]),
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
                    if (!x.TryGet("mesh", out var meshIndexNode)) continue;
                    if (!x.TryGet("firstPersonFlag", out var firstPersonFlagNode)) continue;

                    var renderNodeIndex = FindRenderNodeIndexFromMeshIndex(gltf, meshIndexNode.GetInt32());
                    if (renderNodeIndex.HasValue)
                    {
                        firstPerson.MeshAnnotations.Add(new MeshAnnotation
                        {
                            Node = renderNodeIndex.Value,
                            Type = MigrateFirstPersonType(firstPersonFlagNode),
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
    }
}

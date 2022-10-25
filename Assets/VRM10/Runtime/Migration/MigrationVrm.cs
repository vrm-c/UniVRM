using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;
using Unity.Collections;

[assembly: InternalsVisibleTo("VRM10.Tests")]
[assembly: InternalsVisibleTo("VRM10.Tests.PlayMode")]

namespace UniVRM10
{
    /// <summary>
    /// Convert vrm0 binary to vrm1 binary. Json processing
    /// </summary>
    static internal class MigrationVrm
    {
        public static byte[] Migrate(byte[] vrm0bytes, VRM10ObjectMeta meta = null, Action<UniGLTF.glTF> modGltf = null)
        {
            using (var data = new GlbBinaryParser(vrm0bytes, "migration").Parse())
            {
                return Migrate(data, meta, modGltf);
            }
        }

        static (int, int) GetVertexRange(NativeArray<int> indices)
        {
            var min = int.MaxValue; ;
            var max = 0;
            foreach (var i in indices)
            {
                if (i < min) min = i;
                if (i > max) max = i;
            }
            return (min, max);
        }

        /// <param name="data">vrm0 をパースしたデータ</param>
        /// <param name="meta">migration 時に合成するライセンス情報</param>
        /// <returns></returns>
        public static byte[] Migrate(GltfData data, VRM10ObjectMeta meta = null, Action<UniGLTF.glTF> modGltf = null)
        {
            // VRM0 -> Unity
            var model = ModelReader.Read(data, VrmLib.Coordinates.Vrm0);
            // Unity -> VRM1
            model.ConvertCoordinate(VrmLib.Coordinates.Vrm1);

            var (gltf, bin) = MeshUpdater.Execute(data, model);

            // remove existing VRM0 extension
            gltf.extensions = null;
            if (gltf.extensionsUsed.Contains("VRM"))
            {
                gltf.extensionsUsed.Remove("VRM");
            }

            MigrateVrm(gltf, data.Json.ParseAsJson()["extensions"]["VRM"], meta);

            if (modGltf != null)
            {
                modGltf(gltf);
            }

            // Serialize whole glTF
            ArraySegment<byte> vrm1Json = default;
            {
                var f = new JsonFormatter();
                GltfSerializer.Serialize(f, gltf);
                vrm1Json = f.GetStoreBytes();
            }

            return Glb.Create(vrm1Json, bin).ToBytes();
        }

        /// <summary>
        /// dst が null の場合だけ代入する。
        /// 先に来た方を有効にしたい。
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        static void SetIfNull(ref UniGLTF.Extensions.VRMC_vrm.Expression dst, UniGLTF.Extensions.VRMC_vrm.Expression src)
        {
            if (dst == null)
            {
                dst = src;
            }
        }

        static void MigrateVrm(glTF gltf, JsonNode vrm0, VRM10ObjectMeta meta)
        {
            var meshToNode = CreateMeshToNode(gltf);

            {
                // vrm
                var vrm1 = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm
                {
                    SpecVersion = Vrm10Exporter.VRM_SPEC_VERSION
                };
                gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionName);

                if (meta == null)
                {
                    // migrate from vrm-0.x
                    vrm1.Meta = MigrationVrmMeta.Migrate(gltf, vrm0["meta"]);
                }
                else
                {
                    // inject from arg
                    vrm1.Meta = new UniGLTF.Extensions.VRMC_vrm.Meta
                    {
                        LicenseUrl = Vrm10Exporter.LICENSE_URL_JA,
                        AllowExcessivelySexualUsage = false,
                        AllowExcessivelyViolentUsage = false,
                        AllowPoliticalOrReligiousUsage = false,
                        AllowRedistribution = false,
                    };
                    Vrm10Exporter.ExportMeta(vrm1, meta, null);
                }
                // humanoid (required)
                vrm1.Humanoid = MigrationVrmHumanoid.Migrate(vrm0["humanoid"]);

                // blendshape (optional)
                if (vrm0.TryGet("blendShapeMaster", out JsonNode vrm0BlendShape))
                {
                    vrm1.Expressions = new UniGLTF.Extensions.VRMC_vrm.Expressions
                    {
                        Preset = new UniGLTF.Extensions.VRMC_vrm.Preset(),
                        Custom = new Dictionary<string, UniGLTF.Extensions.VRMC_vrm.Expression>(),
                    };
                    foreach (var (preset, customName, expression) in MigrationVrmExpression.Migrate(gltf, vrm0BlendShape, meshToNode))
                    {
                        switch (preset)
                        {
                            case ExpressionPreset.happy: SetIfNull(ref vrm1.Expressions.Preset.Happy, expression); break;
                            case ExpressionPreset.angry: SetIfNull(ref vrm1.Expressions.Preset.Angry, expression); break;
                            case ExpressionPreset.sad: SetIfNull(ref vrm1.Expressions.Preset.Sad, expression); break;
                            case ExpressionPreset.relaxed: SetIfNull(ref vrm1.Expressions.Preset.Relaxed, expression); break;
                            case ExpressionPreset.surprised: SetIfNull(ref vrm1.Expressions.Preset.Surprised, expression); break;
                            case ExpressionPreset.aa: SetIfNull(ref vrm1.Expressions.Preset.Aa, expression); break;
                            case ExpressionPreset.ih: SetIfNull(ref vrm1.Expressions.Preset.Ih, expression); break;
                            case ExpressionPreset.ou: SetIfNull(ref vrm1.Expressions.Preset.Ou, expression); break;
                            case ExpressionPreset.ee: SetIfNull(ref vrm1.Expressions.Preset.Ee, expression); break;
                            case ExpressionPreset.oh: SetIfNull(ref vrm1.Expressions.Preset.Oh, expression); break;
                            case ExpressionPreset.blink: SetIfNull(ref vrm1.Expressions.Preset.Blink, expression); break;
                            case ExpressionPreset.blinkLeft: SetIfNull(ref vrm1.Expressions.Preset.BlinkLeft, expression); break;
                            case ExpressionPreset.blinkRight: SetIfNull(ref vrm1.Expressions.Preset.BlinkRight, expression); break;
                            case ExpressionPreset.lookUp: SetIfNull(ref vrm1.Expressions.Preset.LookUp, expression); break;
                            case ExpressionPreset.lookDown: SetIfNull(ref vrm1.Expressions.Preset.LookDown, expression); break;
                            case ExpressionPreset.lookLeft: SetIfNull(ref vrm1.Expressions.Preset.LookLeft, expression); break;
                            case ExpressionPreset.lookRight: SetIfNull(ref vrm1.Expressions.Preset.LookRight, expression); break;
                            case ExpressionPreset.neutral: SetIfNull(ref vrm1.Expressions.Preset.Neutral, expression); break;
                            case ExpressionPreset.custom:
                                if (vrm1.Expressions.Custom.ContainsKey(customName))
                                {
                                    // 同名が既存。先着を有効とする
                                }
                                else
                                {
                                    vrm1.Expressions.Custom[customName] = expression;
                                }
                                break;
                            default: throw new NotImplementedException();
                        }
                    }
                }

                // lookat & firstperson (optional)
                if (vrm0.TryGet("firstPerson", out JsonNode vrm0FirstPerson))
                {
                    (vrm1.LookAt, vrm1.FirstPerson) = MigrationVrmLookAtAndFirstPerson.Migrate(gltf, vrm0FirstPerson);
                }

                UniGLTF.Extensions.VRMC_vrm.GltfSerializer.SerializeTo(ref gltf.extensions, vrm1);
            }

            // springBone & collider (optional)
            if (vrm0.TryGet("secondaryAnimation", out JsonNode vrm0SpringBone))
            {
                var springBone = MigrationVrmSpringBone.Migrate(gltf, vrm0SpringBone);
                UniGLTF.Extensions.VRMC_springBone.GltfSerializer.SerializeTo(ref gltf.extensions, springBone);
                gltf.extensionsUsed.Add(UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionName);
            }

            // Material
            {
                MigrationMaterials.Migrate(gltf, vrm0);
            }
        }

        public delegate int MeshIndexToNodeIndexFunc(int meshIndex);

        public static MeshIndexToNodeIndexFunc CreateMeshToNode(UniGLTF.glTF gltf)
        {
            return (int mesh) =>
            {
                for (int i = 0; i < gltf.nodes.Count; ++i)
                {
                    var node = gltf.nodes[i];
                    if (node.mesh == mesh)
                    {
                        return i;
                    }
                }
                return -1;
            };
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm1, MeshIndexToNodeIndexFunc meshToNode)
        {
            MigrationVrmMeta.Check(vrm0["meta"], vrm1.Meta);
            MigrationVrmHumanoid.Check(vrm0["humanoid"], vrm1.Humanoid);
            MigrationVrmExpression.Check(vrm0["blendShapeMaster"], vrm1.Expressions, meshToNode);
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone vrm1, List<glTFNode> nodes)
        {
            // Migration.CheckSpringBone(vrm0["secondaryAnimation"], vrm1.sp)
        }
    }

}
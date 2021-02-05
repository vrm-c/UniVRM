using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// Convert vrm0 binary to vrm1 binary. Json processing
    /// </summary>
    public static class MigrationVrm
    {
        public static byte[] Migrate(byte[] src)
        {
            var glb = UniGLTF.Glb.Parse(src);
            var json = glb.Json.Bytes.ParseAsJson();
            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);

            // https://github.com/vrm-c/vrm-specification/issues/205
            SceneRotator.Rotate(gltf);

            var extensions = new UniGLTF.glTFExtensionExport();
            {
                var vrm0 = json["extensions"]["VRM"];

                {
                    // vrm
                    var vrm1 = new UniGLTF.Extensions.VRMC_vrm.VRMC_vrm();
                    vrm1.Meta = MigrationVrmMeta.Migrate(vrm0["meta"]);
                    vrm1.Humanoid = MigrationVrmHumanoid.Migrate(vrm0["humanoid"]);
                    vrm1.Expressions = MigrationVrmExpression.Migrate(gltf, vrm0["blendShapeMaster"]).ToList();
                    // lookat
                    // firstperson

                    var f = new JsonFormatter();
                    UniGLTF.Extensions.VRMC_vrm.GltfSerializer.Serialize(f, vrm1);
                    extensions.Add(UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionName, f.GetStoreBytes());
                }
                {
                    // springBone & collider
                    var vrm1 = MigrationVrmSpringBone.Migrate(gltf, json["extensions"]["VRM"]["secondaryAnimation"]);

                    var f = new JsonFormatter();
                    UniGLTF.Extensions.VRMC_springBone.GltfSerializer.Serialize(f, vrm1);
                    extensions.Add(UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionName, f.GetStoreBytes());
                }
                {
                    // MToon
                }
                {
                    // constraint
                }
            }

            ArraySegment<byte> vrm1Json = default;
            {
                gltf.extensions = extensions;

                var f = new JsonFormatter();
                UniGLTF.GltfSerializer.Serialize(f, gltf);
                vrm1Json = f.GetStoreBytes();
            }

            return UniGLTF.Glb.Create(vrm1Json, glb.Binary.Bytes).ToBytes();
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm1)
        {
            MigrationVrmMeta.Check(vrm0["meta"], vrm1.Meta);
            MigrationVrmHumanoid.Check(vrm0["humanoid"], vrm1.Humanoid);
        }

        public static void Check(JsonNode vrm0, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone vrm1, List<UniGLTF.glTFNode> nodes)
        {
            // Migration.CheckSpringBone(vrm0["secondaryAnimation"], vrm1.sp)
        }
    }
}

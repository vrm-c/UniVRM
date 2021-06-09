using System;
using System.IO;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public static class Vrm10Parser
    {
        public readonly struct Result
        {
            public readonly GltfParser Parser;
            public readonly VRMC_vrm Vrm;

            public Result(GltfParser parser, VRMC_vrm vrm)
            {
                Parser = parser;
                Vrm = vrm;
            }
        }

        public static bool TryParseOrMigrate(string path, bool doMigrate, out Result result, out string error)
        {
            return TryParseOrMigrate(path, File.ReadAllBytes(path), doMigrate, out result, out error);
        }

        /// <summary>
        /// VRM1 で　パースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doMigrate"></param>
        /// <returns></returns>
        public static bool TryParseOrMigrate(string path, byte[] bytes, bool doMigrate, out Result result, out string error)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            {
                var parser = new GltfParser();
                parser.Parse(path, bytes);
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
                {
                    // success
                    error = default;
                    result = new Result(parser, vrm);
                    return true;
                }
            }

            if (!doMigrate)
            {
                error = "vrm1 not found";
                result = default;
                return false;
            }

            // try migrateion
            byte[] migrated = default;
            try
            {
                var glb = UniGLTF.Glb.Parse(bytes);
                var json = glb.Json.Bytes.ParseAsJson();
                if (!json.TryGet("extensions", out JsonNode extensions))
                {
                    error = "no gltf.extensions";
                    result = default;
                    return false;
                }
                if (!extensions.TryGet("VRM", out JsonNode vrm0))
                {
                    error = "vrm0 not found";
                    result = default;
                    return false;
                }

                migrated = MigrationVrm.Migrate(json, glb.Binary.Bytes);
                if (migrated == null)
                {
                    error = "cannot migrate";
                    result = default;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = $"migration error: {ex}";
                result = default;
                return false;
            }

            {
                var parser = new GltfParser();
                parser.Parse(path, migrated);
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out VRMC_vrm vrm))
                {
                    // success
                    error = default;
                    result = new Result(parser, vrm);
                    return true;
                }
            }

            error = "migrate but no vrm1. unknown";
            result = default;
            return false;
        }
    }
}

using System;
using System.IO;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public enum Vrm10FileType
    {
        Vrm1,
        Vrm0,
        Other,
    }

    public static class Vrm10Parser
    {
        public readonly struct Result
        {
            public readonly IGltfData Data;
            public readonly VRMC_vrm Vrm;
            public readonly Vrm10FileType FileType;
            public readonly String Message;
            public Result(IGltfData data, VRMC_vrm vrm, Vrm10FileType fileType, string message)
            {
                Data = data;
                Vrm = vrm;
                FileType = fileType;
                Message = message;
            }
        }

        public static bool TryParseOrMigrate(string path, bool doMigrate, out Result result)
        {
            return TryParseOrMigrate(path, File.ReadAllBytes(path), doMigrate, out result);
        }

        /// <summary>
        /// VRM1 でパースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doMigrate"></param>
        /// <returns></returns>
        public static bool TryParseOrMigrate(string path, byte[] bytes, bool doMigrate, out Result result)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            {
                var data = new GlbLowLevelParser(path, bytes).Parse();
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
                {
                    // success
                    result = new Result(data, vrm, Vrm10FileType.Vrm1, "vrm1: loaded");
                    return true;
                }
            }

            // try migrateion
            byte[] migrated = default;
            try
            {
                var glb = UniGLTF.Glb.Parse(bytes);
                var json = glb.Json.Bytes.ParseAsJson();

                try
                {
                    if (!json.TryGet("extensions", out JsonNode extensions))
                    {
                        result = new Result(default, default, Vrm10FileType.Other, "gltf: no extensions");
                        return false;
                    }
                    if (!extensions.TryGet("VRM", out JsonNode vrm0))
                    {
                        result = new Result(default, default, Vrm10FileType.Other, "gltf: no vrm0");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    result = new Result(default, default, Vrm10FileType.Other, $"error: {ex}");
                    return false;
                }

                if (!doMigrate)
                {
                    result = new Result(default, default, Vrm10FileType.Vrm0, "vrm0: not migrated");
                    return false;
                }

                migrated = MigrationVrm.Migrate(json, glb.Binary.Bytes);
                if (migrated == null)
                {
                    result = new Result(default, default, Vrm10FileType.Vrm0, "vrm0: cannot migrate");
                    return false;
                }
            }
            catch (Exception ex)
            {
                result = new Result(default, default, Vrm10FileType.Vrm0, $"vrm0: migration error: {ex}");
                return false;
            }

            {
                var data = new GlbLowLevelParser(path, migrated).Parse();
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out VRMC_vrm vrm))
                {
                    // success
                    result = new Result(data, vrm, Vrm10FileType.Vrm0, "vrm0: migrated");
                    return true;
                }

                result = new Result(default, default, Vrm10FileType.Vrm0, "vrm0: migrate but error ?");
                return false;
            }
        }
    }
}

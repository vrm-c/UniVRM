using System;
using System.IO;
using System.Linq;
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

    public class Vrm10Data
    {
        public GltfData Data { get; }
        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm VrmExtension { get; }
        public readonly Migration.Vrm0Meta OriginalMetaBeforeMigration;
        public readonly Vrm10FileType FileType;
        public readonly String Message;

        public Vrm10Data(GltfData data, VRMC_vrm vrm, Vrm10FileType fileType, string message, Migration.Vrm0Meta oldMeta = null)
        {
            Data = data;
            VrmExtension = vrm;
            FileType = fileType;
            Message = message;
            OriginalMetaBeforeMigration = oldMeta;
        }

        public static bool TryParseOrMigrate(string path, bool doMigrate, out Vrm10Data result)
        {
            return TryParseOrMigrate(path, File.ReadAllBytes(path), doMigrate, out result);
        }

        public static bool TryParseOrMigrate(string path, byte[] bytes, bool doMigrate, out Vrm10Data result)
        {
            var data = new GlbLowLevelParser(path, bytes).Parse();
            return TryParseOrMigrate(data, doMigrate, out result);
        }

        /// <summary>
        /// VRM1 でパースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doMigrate"></param>
        /// <returns></returns>
        public static bool TryParseOrMigrate(GltfData data, bool doMigrate, out Vrm10Data result)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            {
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
                {
                    // success
                    result = new Vrm10Data(data, vrm, Vrm10FileType.Vrm1, "vrm1: loaded");
                    return true;
                }
            }

            // try migrateion
            byte[] migrated = default;
            Migration.Vrm0Meta oldMeta = default;
            try
            {
                var json = data.Json.ParseAsJson();
                try
                {
                    if (!json.TryGet("extensions", out JsonNode extensions))
                    {
                        result = new Vrm10Data(default, default, Vrm10FileType.Other, "gltf: no extensions");
                        return false;
                    }
                    if (!extensions.TryGet("VRM", out JsonNode vrm0))
                    {
                        result = new Vrm10Data(default, default, Vrm10FileType.Other, "gltf: no vrm0");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    result = new Vrm10Data(default, default, Vrm10FileType.Other, $"error: {ex}");
                    return false;
                }

                if (!doMigrate)
                {
                    result = new Vrm10Data(default, default, Vrm10FileType.Vrm0, "vrm0: not migrated");
                    return false;
                }

                migrated = MigrationVrm.Migrate(data);
                if (migrated == null)
                {
                    result = new Vrm10Data(default, default, Vrm10FileType.Vrm0, "vrm0: cannot migrate");
                    return false;
                }

                oldMeta = Migration.Vrm0Meta.FromJsonBytes(json);
            }
            catch (Exception ex)
            {
                result = new Vrm10Data(default, default, Vrm10FileType.Vrm0, $"vrm0: migration error: {ex}");
                return false;
            }

            {
                var migratedData = new GlbLowLevelParser(data.TargetPath, migrated).Parse();
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(migratedData.GLTF.extensions, out VRMC_vrm vrm))
                {
                    // success
                    if (oldMeta == null)
                    {
                        throw new NullReferenceException("oldMeta");
                    }
                    result = new Vrm10Data(migratedData, vrm, Vrm10FileType.Vrm0, "vrm0: migrated", oldMeta);
                    return true;
                }

                result = new Vrm10Data(default, default, Vrm10FileType.Vrm0, "vrm0: migrate but error ?");
                return false;
            }
        }
    }
}

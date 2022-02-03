using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public class MigrationData
    {
        /// <summary>
        /// マイグレーション失敗など
        /// </summary>
        public readonly String Message;

        /// <summary>
        /// vrm0 からマイグレーションした場合に、vrm0 版の meta 情報
        /// </summary>
        public readonly Migration.Vrm0Meta OriginalMetaBeforeMigration;

        /// <summary>
        /// Migration した結果のバイト列(デバッグ用)
        /// </summary>
        public readonly byte[] MigratedBytes;

        public MigrationData(string msg, Migration.Vrm0Meta meta = default, byte[] bytes = default)
        {
            Message = msg;
            OriginalMetaBeforeMigration = meta;
            MigratedBytes = bytes;
        }
    }

    public class Vrm10Data
    {
        public GltfData Data { get; }
        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm VrmExtension { get; }

        Vrm10Data(GltfData data, VRMC_vrm vrm)
        {
            Data = data;
            VrmExtension = vrm;
        }

        public static GltfData ParseOrMigrate(string path, bool doMigrate, out Vrm10Data vrm1Data, out MigrationData migration)
        {
            return ParseOrMigrate(path, File.ReadAllBytes(path), doMigrate, out vrm1Data, out migration);
        }

        /// <summary>
        /// vrm1 をパースする。vrm0 からのマイグレートもできる。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        /// <param name="doMigrate"></param>
        /// <param name="vrm1Data">成功した場合非 null</param>
        /// <param name="migration">doMigrate==true の場合、関連情報が入る</param>
        /// <returns>GltfDataを作成できたときは Return するのでDisposeすること</returns>
        public static GltfData ParseOrMigrate(string path, byte[] bytes, bool doMigrate, out Vrm10Data vrm1Data, out MigrationData migration)
        {
            var data = new GlbLowLevelParser(path, bytes).Parse();
            byte[] migrated = default;
            byte[] migratedBytes = null;
            Migration.Vrm0Meta oldMeta = default;
            try
            {
                if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
                {
                    // success
                    vrm1Data = new Vrm10Data(data, vrm);
                    migration = default;
                    return data;
                }

                if (!doMigrate)
                {
                    vrm1Data = default;
                    migration = new MigrationData("Not vrm1 and no migration");
                    return data;
                }

                // try migrateion
                // Migration.Vrm0Meta oldMeta = default;
                JsonNode json = data.Json.ParseAsJson();
                if (!json.TryGet("extensions", out JsonNode extensions))
                {
                    vrm1Data = default;
                    migration = new MigrationData("gltf: no extensions");
                    return data;
                }

                if (!extensions.TryGet("VRM", out JsonNode vrm0))
                {
                    vrm1Data = default;
                    migration = new MigrationData("gltf: no vrm0");
                    return data;
                }

                // found vrm0
                oldMeta = Migration.Vrm0Meta.FromJsonBytes(json);
                if (oldMeta == null)
                {
                    throw new NullReferenceException("oldMeta");
                }

                // try migrate...
                migrated = MigrationVrm.Migrate(data);
                if (migrated == null)
                {
                    vrm1Data = default;
                    migration = new MigrationData("Found vrm0. But fail to migrate", oldMeta);
                    return data;
                }

                if (VRMShaders.Symbols.VRM_DEVELOP)
                {
                    // load 時の右手左手座標変換でバッファが破壊的変更されるので、コピーを作っている        
                    migratedBytes = migrated.Select(x => x).ToArray();
                }
            }
            catch (Exception ex)
            {
                // 何か起きた。Dispose は頼む
                vrm1Data = default;
                migration = new MigrationData(ex.Message);
                return data;
            }

            // マイグレーション前を破棄
            data.Dispose();
            // マイグレーション結果をパースする
            var migratedData = new GlbLowLevelParser(data.TargetPath, migrated).Parse();
            try
            {
                if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(migratedData.GLTF.extensions, out VRMC_vrm vrm))
                {
                    // migration した結果のパースに失敗した !
                    vrm1Data = default;
                    migration = new MigrationData("vrm0: migrate but error ?", oldMeta, migrated);
                    return migratedData;
                }

                {
                    // success
                    vrm1Data = new Vrm10Data(migratedData, vrm);
                    migration = new MigrationData("vrm0: migrated", oldMeta, migratedBytes);
                    return migratedData;
                }
            }
            catch (Exception ex)
            {
                // 何か起きた。Dispose は頼む
                vrm1Data = default;
                migration = new MigrationData(ex.Message);
                return migratedData;
            }
        }
    }
}

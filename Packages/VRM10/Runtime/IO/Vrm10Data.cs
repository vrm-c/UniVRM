using System;
using System.Linq;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    public class Vrm10Data
    {
        public GltfData Data { get; }
        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm VrmExtension { get; }

        Vrm10Data(GltfData data, VRMC_vrm vrm)
        {
            Data = data;
            VrmExtension = vrm;

            // ヒューマノイド向け
            ForceGltfNodeUniqueName.Process(Data.GLTF.nodes);
        }

        /// <summary>
        /// VRM-1.0 拡張を取得する。
        /// </summary>
        /// <param name="data"></param>
        /// <returns>失敗したら null が返る</returns>
        public static Vrm10Data Parse(GltfData data)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out var vrm))
            {
                return null;
            }
            return new Vrm10Data(data, vrm);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vrm1Data"></param>
        /// <param name="migration"></param>
        /// <returns>Migrated GltfData if succeeded. Must Dispose</returns>
        public static GltfData Migrate(GltfData data, out Vrm10Data vrm1Data, out MigrationData migration)
        {
            var json = data.Json.ParseAsJson();
            if (!json.TryGet("extensions", out JsonNode extensions))
            {
                vrm1Data = default;
                migration = new MigrationData("gltf: no extensions");
                return null;
            }

            if (!extensions.TryGet("VRM", out JsonNode vrm0))
            {
                vrm1Data = default;
                migration = new MigrationData("gltf: no vrm0");
                return null;
            }

            // found vrm0
            var oldMeta = Migration.Vrm0Meta.FromJsonBytes(json);
            if (oldMeta == null)
            {
                throw new NullReferenceException("oldMeta");
            }

            // try migrate...
            byte[] migrated = null;
            try
            {
                migrated = MigrationVrm.Migrate(data);
                if (migrated == null)
                {
                    vrm1Data = default;
                    migration = new MigrationData("Found vrm0. But fail to migrate", oldMeta);
                    return null;
                }
            }
            catch (MigrationException ex)
            {
                // migration 失敗
                vrm1Data = default;
                migration = new MigrationData(ex.ToString(), oldMeta);
                return null;
            }
            catch (Exception ex)
            {
                // その他のエラー
                vrm1Data = default;
                migration = new MigrationData(ex.ToString(), oldMeta);
                return null;
            }

            byte[] debugCopy = null;
            if (Symbols.VRM_DEVELOP)
            {
                // load 時の右手左手座標変換でバッファが破壊的変更されるので、コピーを作っている
                debugCopy = migrated.Select(x => x).ToArray();
            }

            // マイグレーション結果をパースする
            var migratedData = new GlbLowLevelParser(data.TargetPath, migrated).Parse();
            migratedData.ExtensionSupportFlags.CopyValueFrom(data.ExtensionSupportFlags);
            try
            {
                if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(migratedData.GLTF.extensions, out VRMC_vrm vrm))
                {
                    // migration した結果のパースに失敗した !
                    vrm1Data = default;
                    migration = new MigrationData("vrm0: migrate but error ?", oldMeta, migrated);
                    // 破棄
                    migratedData.Dispose();
                    return null;
                }

                {
                    // success. 非null値が返るのはここだけ。
                    vrm1Data = new Vrm10Data(migratedData, vrm);
                    migration = new MigrationData("vrm0: migrated", oldMeta, debugCopy);
                    return migratedData;
                }
            }
            catch (Exception ex)
            {
                UniGLTFLogger.Warning($"{ex}");
                vrm1Data = default;
                migration = new MigrationData(ex.Message);
                // 破棄
                migratedData.Dispose();
                return null;
            }
        }
    }
}

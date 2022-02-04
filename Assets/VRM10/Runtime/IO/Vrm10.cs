using System;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    /// <summary>
    /// High-level VRM-1.0 loading API.
    /// </summary>
    public static class Vrm10
    {
        /// <summary>
        /// You can receive the thumbnail texture and the meta information.
        /// `vrm10Meta` will be available if the model was vrm-1.0.
        /// `vrm0Meta` will be available if the model was vrm-0.x.
        /// </summary>
        public delegate void VrmMetaInformationCallback(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta vrm10Meta, Migration.Vrm0Meta vrm0Meta);

        /// <summary>
        /// Load the VRM file from the path.
        /// </summary>
        /// <param name="path">vrm file path</param>
        /// <param name="canLoadVrm0X">if true, this loader can load the vrm-0.x model as vrm-1.0 model with migration.</param>
        /// <param name="normalizeTransform">if true, vrm-1.0 models' transforms are normalized. (e.g. rotation, scaling)</param>
        /// <param name="showMeshes">if true, show meshes when loaded.</param>
        /// <param name="awaitCaller">this loader use specified await strategy.</param>
        /// <param name="materialGenerator">this loader use specified material generation strategy.</param>
        /// <param name="vrmMetaInformationCallback">return callback that notify meta information before loading.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>vrm-1.0 instance</returns>
        public static async Task<Vrm10Instance> LoadPathAsync(
            string path,
            bool canLoadVrm0X = true,
            bool normalizeTransform = true,
            bool showMeshes = true,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            VrmMetaInformationCallback vrmMetaInformationCallback = null,
            CancellationToken ct = default)
        {
            return await LoadAsync(
                path,
                System.IO.File.ReadAllBytes(path),
                canLoadVrm0X,
                normalizeTransform,
                showMeshes,
                awaitCaller,
                materialGenerator,
                vrmMetaInformationCallback,
                ct);
        }

        /// <summary>
        /// Load the VRM file from the binary.
        /// </summary>
        /// <param name="bytes">vrm file data</param>
        /// <param name="canLoadVrm0X">if true, this loader can load the vrm-0.x model as vrm-1.0 model with migration.</param>
        /// <param name="normalizeTransform">if true, vrm-1.0 models' transforms are normalized. (e.g. rotation, scaling)</param>
        /// <param name="showMeshes">if true, show meshes when loaded.</param>
        /// <param name="awaitCaller">this loader use specified await strategy.</param>
        /// <param name="materialGenerator">this loader use specified material generation strategy.</param>
        /// <param name="vrmMetaInformationCallback">return callback that notify meta information before loading.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>vrm-1.0 instance</returns>
        public static async Task<Vrm10Instance> LoadBytesAsync(
            byte[] bytes,
            bool canLoadVrm0X = true,
            bool normalizeTransform = true,
            bool showMeshes = true,
            IAwaitCaller awaitCaller = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            VrmMetaInformationCallback vrmMetaInformationCallback = null,
            CancellationToken ct = default)
        {
            return await LoadAsync(
                string.Empty,
                bytes,
                canLoadVrm0X,
                normalizeTransform,
                showMeshes,
                awaitCaller,
                materialGenerator,
                vrmMetaInformationCallback,
                ct);
        }

        private static async Task<Vrm10Instance> LoadAsync(
            string name,
            byte[] bytes,
            bool canLoadVrm0X,
            bool normalizeTransform,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            using (var data = new GlbLowLevelParser(name, bytes).Parse())
            {
                // 1. Try loading as vrm-1.0
                var vrm10Data = Vrm10Data.Parse(data);
                var vrm10Instance = await LoadVrm10DataAsync(
                    vrm10Data,
                    null,
                    normalizeTransform,
                    showMeshes,
                    awaitCaller,
                    materialGenerator,
                    vrmMetaInformationCallback,
                    ct);
                if (vrm10Instance != null) return vrm10Instance;

                if (!canLoadVrm0X)
                {
                    return default;
                }

                // 2. Try migration from vrm-0.x into vrm-1.0
                MigrationData migrationData;
                using (var migrated = Vrm10Data.Migrate(data, out vrm10Data, out migrationData))
                {
                    var instance = await LoadVrm10DataAsync(
                        vrm10Data,
                        migrationData,
                        normalizeTransform,
                        showMeshes,
                        awaitCaller,
                        materialGenerator,
                        vrmMetaInformationCallback,
                        ct);
                    if (instance != null) return instance;
                }

                // 3. failed
                if (migrationData != null)
                {
                    Debug.LogWarning(migrationData.Message);
                }
                return default;
            }
        }

        private static async Task<Vrm10Instance> LoadVrm10DataAsync(
            Vrm10Data vrm10Data,
            MigrationData migrationData,
            bool normalizeTransform,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            if (vrm10Data == null)
            {
                return default;
            }

            using (var loader = new Vrm10Importer(vrm10Data, materialGenerator: materialGenerator, doNormalize: normalizeTransform))
            {
                // 1. Load meta information if callback was available.
                if (vrmMetaInformationCallback != null)
                {
                    var thumbnail = await loader.LoadVrmThumbnailAsync();
                    if (migrationData != null)
                    {
                        vrmMetaInformationCallback(thumbnail, default, migrationData.OriginalMetaBeforeMigration);
                    }
                    else
                    {
                        vrmMetaInformationCallback(thumbnail, vrm10Data.VrmExtension.Meta, default);
                    }
                }

                // 2. Load
                var gltfInstance = await loader.LoadAsync(awaitCaller);
                if (ct.IsCancellationRequested)
                {
                    gltfInstance.Dispose();
                    return default;
                }

                var vrm10Instance = gltfInstance.GetComponent<Vrm10Instance>();
                if (vrm10Instance == null)
                {
                    gltfInstance.Dispose();
                    return default;
                }

                if (showMeshes)
                {
                    gltfInstance.ShowMeshes();
                }
                return vrm10Instance;
            }
        }
    }
}

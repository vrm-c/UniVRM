﻿using System;
using System.Collections.Generic;
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
        ///
        /// You should call this on Unity main thread.
        /// This will throw Exceptions (include OperationCanceledException).
        /// </summary>
        /// <param name="path">vrm file path</param>
        /// <param name="canLoadVrm0X">if true, this loader can load the vrm-0.x model as vrm-1.0 model with migration.</param>
        /// <param name="controlRigGenerationOption">the flag of generating the control rig provides bone manipulation unified between models.</param>
        /// <param name="showMeshes">if true, show meshes when loaded.</param>
        /// <param name="awaitCaller">this loader use specified await strategy.</param>
        /// <param name="textureDeserializer">this loader use specified texture deserialization strategy.</param>
        /// <param name="materialGenerator">this loader use specified material generation strategy.</param>
        /// <param name="vrmMetaInformationCallback">return callback that notify meta information before loading.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>vrm-1.0 instance. Maybe return null if unexpected error was raised.</returns>
        public static async Task<Vrm10Instance> LoadPathAsync(
            string path,
            bool canLoadVrm0X = true,
            bool controlRigGenerationOption = true,
            bool showMeshes = true,
            IAwaitCaller awaitCaller = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            VrmMetaInformationCallback vrmMetaInformationCallback = null,
            CancellationToken ct = default)
        {
            if (awaitCaller == null)
            {
                awaitCaller = Application.isPlaying
                    ? (IAwaitCaller)new RuntimeOnlyAwaitCaller()
                    : (IAwaitCaller)new ImmediateCaller();
            }

            return await LoadAsync(
                path,
                System.IO.File.ReadAllBytes(path),
                canLoadVrm0X,
                controlRigGenerationOption,
                showMeshes,
                awaitCaller,
                textureDeserializer,
                materialGenerator,
                vrmMetaInformationCallback,
                ct);
        }

        /// <summary>
        /// Load the VRM file from the binary.
        ///
        /// You should call this on Unity main thread.
        /// This will throw Exceptions (include OperationCanceledException).
        /// </summary>
        /// <param name="bytes">vrm file data</param>
        /// <param name="canLoadVrm0X">if true, this loader can load the vrm-0.x model as vrm-1.0 model with migration.</param>
        /// <param name="controlRigGenerationOption">the flag of generating the control rig provides bone manipulation unified between models.</param>
        /// <param name="showMeshes">if true, show meshes when loaded.</param>
        /// <param name="awaitCaller">this loader use specified await strategy.</param>
        /// <param name="textureDeserializer">this loader use specified texture deserialization strategy.</param>
        /// <param name="materialGenerator">this loader use specified material generation strategy.</param>
        /// <param name="vrmMetaInformationCallback">return callback that notify meta information before loading.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>vrm-1.0 instance. Maybe return null if unexpected error was raised.</returns>
        public static async Task<Vrm10Instance> LoadBytesAsync(
            byte[] bytes,
            bool canLoadVrm0X = true,
            bool controlRigGenerationOption = true,
            bool showMeshes = true,
            IAwaitCaller awaitCaller = null,
            ITextureDeserializer textureDeserializer = null,
            IMaterialDescriptorGenerator materialGenerator = null,
            VrmMetaInformationCallback vrmMetaInformationCallback = null,
            CancellationToken ct = default)
        {
            if (awaitCaller == null)
            {
                awaitCaller = Application.isPlaying
                    ? (IAwaitCaller)new RuntimeOnlyAwaitCaller()
                    : (IAwaitCaller)new ImmediateCaller();
            }

            return await LoadAsync(
                string.Empty,
                bytes,
                canLoadVrm0X,
                controlRigGenerationOption,
                showMeshes,
                awaitCaller,
                textureDeserializer,
                materialGenerator,
                vrmMetaInformationCallback,
                ct);
        }

        private static async Task<Vrm10Instance> LoadAsync(
            string name,
            byte[] bytes,
            bool canLoadVrm0X,
            bool controlRigGenerationOption,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            ITextureDeserializer textureDeserializer,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            using (var gltfData = new GlbLowLevelParser(name, bytes).Parse())
            {
                // 1. Try loading as vrm-1.0
                var instance = await TryLoadingAsVrm10Async(
                    gltfData,
                    controlRigGenerationOption,
                    showMeshes,
                    awaitCaller,
                    textureDeserializer,
                    materialGenerator,
                    vrmMetaInformationCallback,
                    ct);
                if (instance != null)
                {
                    if (ct.IsCancellationRequested)
                    {
                        UnityObjectDestroyer.DestroyRuntimeOrEditor(instance.gameObject);
                        ct.ThrowIfCancellationRequested();
                    }
                    return instance;
                }

                // 2. Stop loading if not allowed migration.
                if (!canLoadVrm0X)
                {
                    throw new Exception($"Failed to load as VRM 1.0: {name}");
                }

                // 3. Try migration from vrm-0.x into vrm-1.0
                var migratedInstance = await TryMigratingFromVrm0XAsync(
                    gltfData,
                    controlRigGenerationOption,
                    showMeshes,
                    awaitCaller,
                    textureDeserializer,
                    materialGenerator,
                    vrmMetaInformationCallback,
                    ct);
                if (migratedInstance != null)
                {
                    if (ct.IsCancellationRequested)
                    {
                        UnityObjectDestroyer.DestroyRuntimeOrEditor(migratedInstance.gameObject);
                        ct.ThrowIfCancellationRequested();
                    }
                    return migratedInstance;
                }

                // 4. Failed loading.
                throw new Exception($"Failed to load: {name}");
            }
        }

        private static async Task<Vrm10Instance> TryLoadingAsVrm10Async(
            GltfData gltfData,
            bool controlRigGenerationOption,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            ITextureDeserializer textureDeserializer,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            var vrm10Data = await awaitCaller.Run(() => Vrm10Data.Parse(gltfData));
            ct.ThrowIfCancellationRequested();

            if (vrm10Data == null)
            {
                // NOTE: Failed to parse as VRM 1.0.
                return null;
            }

            return await LoadVrm10DataAsync(
                vrm10Data,
                null,
                controlRigGenerationOption,
                showMeshes,
                awaitCaller,
                textureDeserializer,
                materialGenerator,
                vrmMetaInformationCallback,
                ct);
        }

        private static async Task<Vrm10Instance> TryMigratingFromVrm0XAsync(
            GltfData gltfData,
            bool controlRigGenerationOption,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            ITextureDeserializer textureDeserializer,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            Vrm10Data migratedVrm10Data = default;
            MigrationData migrationData = default;
            using (var migratedGltfData = await awaitCaller.Run(() => Vrm10Data.Migrate(gltfData, out migratedVrm10Data, out migrationData)))
            {
                ct.ThrowIfCancellationRequested();

                if (migratedVrm10Data == null)
                {
                    throw new Exception(migrationData?.Message ?? "Failed to migrate.");
                }

                var migratedVrm10Instance = await LoadVrm10DataAsync(
                    migratedVrm10Data,
                    migrationData,
                    controlRigGenerationOption,
                    showMeshes,
                    awaitCaller,
                    textureDeserializer,
                    materialGenerator,
                    vrmMetaInformationCallback,
                    ct);
                if (migratedVrm10Instance == null)
                {
                    throw new Exception(migrationData?.Message ?? "Failed to load migrated.");
                }
                return migratedVrm10Instance;
            }
        }

        private static async Task<Vrm10Instance> LoadVrm10DataAsync(
            Vrm10Data vrm10Data,
            MigrationData migrationData,
            bool controlRigGenerationOption,
            bool showMeshes,
            IAwaitCaller awaitCaller,
            ITextureDeserializer textureDeserializer,
            IMaterialDescriptorGenerator materialGenerator,
            VrmMetaInformationCallback vrmMetaInformationCallback,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (awaitCaller == null)
            {
                throw new ArgumentNullException();
            }

            if (vrm10Data == null)
            {
                throw new ArgumentNullException(nameof(vrm10Data));
            }

            using (var loader = new Vrm10Importer(
                       vrm10Data,
                       textureDeserializer: textureDeserializer,
                       materialGenerator: materialGenerator,
                       controlRigInitialRotations: controlRigGenerationOption))
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
                // NOTE: Current Vrm10Importer.LoadAsync implementation CAN'T ABORT.
                var gltfInstance = await loader.LoadAsync(awaitCaller);
                if (gltfInstance == null)
                {
                    throw new Exception("Failed to load by unknown reason.");
                }

                var vrm10Instance = gltfInstance.GetComponent<Vrm10Instance>();
                if (vrm10Instance == null)
                {
                    gltfInstance.Dispose();
                    throw new Exception("Failed to load as VRM by unknown reason.");
                }

                if (ct.IsCancellationRequested)
                {
                    // NOTE: Destroy before showing meshes if canceled.
                    gltfInstance.Dispose();
                    ct.ThrowIfCancellationRequested();
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

using UniGLTF;

namespace VRM
{
    public class VRMData
    {
        public GltfData Data { get; }
        public glTF_VRM_extensions VrmExtension { get; }

        public VRMData(GltfData data)
        {
            Data = data;

            if (!glTF_VRM_extensions.TryDeserialize(data.GLTF.extensions, out VRM.glTF_VRM_extensions vrm))
            {
                throw new NotVrm0Exception();
            }
            VrmExtension = vrm;

            UpdateMigrationFlags(Data.MigrationFlags, VrmExtension.exporterVersion);
        }

        private static void UpdateMigrationFlags(MigrationFlags migrationFlags, string exportedVrmVersionString)
        {
            if (!VRMVersion.ParseVersion(exportedVrmVersionString, out var exportedVrmVersion)) return;

            migrationFlags.IsBaseColorFactorGamma = VRMVersion.IsNewer(
                new VRMVersion.Version
                {
                    Major = 0,
                    Minor = 54,
                    Patch = 0,
                    Pre = "",
                },
                exportedVrmVersion
            );

            migrationFlags.IsRoughnessTextureValueSquared = VRMVersion.IsNewer(
                new VRMVersion.Version
                {
                    Major = 0,
                    Minor = 69,
                    Patch = 0,
                    Pre = "",
                },
                exportedVrmVersion
            );
            migrationFlags.IsEmissiveFactorGamma = VRMVersion.IsNewer(
                new VRMVersion.Version
                {
                    Major = 0,
                    Minor = 107,
                    Patch = 0,
                    Pre = "",
                },
                exportedVrmVersion
            );
        }
    }
}

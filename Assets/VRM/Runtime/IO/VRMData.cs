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
            if (!PackageVersion.ParseVersion(exportedVrmVersionString, out var exportedVrmVersion)) return;

            migrationFlags.IsBaseColorFactorGamma = PackageVersion.IsNewer(
                new PackageVersion.Version
                {
                    Major = 0,
                    Minor = 54,
                    Patch = 0,
                    Pre = "",
                },
                exportedVrmVersion
            );

            migrationFlags.IsRoughnessTextureValueSquared = PackageVersion.IsNewer(
                new PackageVersion.Version
                {
                    Major = 0,
                    Minor = 69,
                    Patch = 0,
                    Pre = "",
                },
                exportedVrmVersion
            );
            migrationFlags.IsEmissiveFactorGamma = PackageVersion.IsNewer(
                new PackageVersion.Version
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

using System;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;

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

            // ヒューマノイド向け
            Func<glTFNode, glTFNode> getParent = (node) =>
            {
                var index = Data.GLTF.nodes.IndexOf(node);
                return Data.GLTF.nodes.FirstOrDefault(x => x.children != null && x.children.Contains(index));
            };

            ForceTransformUniqueName.Process(Data.GLTF.nodes,
                node => node.name,
                (node, name) => node.name = name,
                node =>
                {
                    var parent = getParent(node);
                    if (parent != null && node.children != null && node.children.Length == 0)
                    {
                        return $"{parent.name}-{node.name}";
                    }
                    return null;
                });
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

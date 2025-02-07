using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    public sealed class OrderedMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private IMaterialImporter[] _importers;

        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();

        /// <summary>
        /// 順に TryCreateParam を実行して最初に成功したら終わる。
        /// 全て失敗したら UrpGltfDefaultMaterialImporter を実行する。
        /// 通常 vrm-1.0, unlit, pbr の順に試行する。
        /// </summary>
        /// <param name="importers"></param>
        public OrderedMaterialDescriptorGenerator(params IMaterialImporter[] importers)
        {
            _importers = importers;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            foreach (var importer in _importers)
            {
                if (importer.TryCreateParam(data, i, out var param))
                {
                    return param;
                }
            }
            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}
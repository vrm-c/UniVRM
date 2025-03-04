using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// Importersに格納したを順番に試行して成功したらそれを採用する。
    /// mtoon, unlit, pbr の順に試行することを想定。
    /// </summary>
    public sealed class OrderedMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public readonly List<IMaterialImporter> Importers = new();

        public delegate MaterialDescriptor MakeDefaultMaterialDescriptor(string materialName);

        private readonly MakeDefaultMaterialDescriptor _makeDefault;

        /// <summary>
        /// 順に TryCreateParam を実行して最初に成功したら終わる。
        /// 全て失敗したら UrpGltfDefaultMaterialImporter を実行する。
        /// 通常 vrm-1.0, unlit, pbr の順に試行する。
        /// </summary>
        /// <param name="importers"></param>
        public OrderedMaterialDescriptorGenerator(MakeDefaultMaterialDescriptor makeDefault, params IMaterialImporter[] importers)
        {
            _makeDefault = makeDefault;
            Importers.AddRange(importers);
        }

        public static OrderedMaterialDescriptorGenerator CreateCustomGenerator(
            IMaterialImporter customPbr,
            IMaterialImporter customMToonImporter = null)
        {

            var generator = new OrderedMaterialDescriptorGenerator((new UrpGltfDefaultMaterialImporter()).CreateParam);

            // 最初にMToonの分岐
            // generator.Importers は前から順に処理します
            if (customMToonImporter != null)
            {
                // TinyMToon(WebGL 向け) を使う例
                generator.Importers.Add(customMToonImporter);
            }
            else
            {
                // VRM10/Universal Render Pipeline/MToon10 を使う例
                generator.Importers.Add(new UrpMToonMaterialImporter());
            }

            // 次に unlit の分岐
            generator.Importers.Add(new UnlitMaterialImporter());

            // 次に pbr の分岐
            if (customPbr != null)
            {
                // TinyPbr を使う例
                generator.Importers.Add(customPbr);
            }
            else
            {
                // Universal Render Pipeline/Lit を使う例
                // AlwaysIncludedShaders に登録すると、Variant が多すぎてビルドが終わらない問題があります。
                generator.Importers.Add(new UrpPbrMaterialImporter());
            }

            return generator;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            foreach (var importer in Importers)
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

        public MaterialDescriptor GetGltfDefault(string materialName = null) => _makeDefault(materialName);
    }
}
using UnityEngine;
using System.Linq;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    /// <summary>
    /// ScriptedImporterImpl から改め
    /// </summary>
    public abstract class GltfScriptedImporterBase : ScriptedImporter
    {
        [SerializeField]
        public ScriptedImporterAxes m_reverseAxis = default;

        [SerializeField]
        public ImporterRenderPipelineTypes m_renderPipeline;

        /// <summary>
        /// glb をパースして、UnityObject化、さらにAsset化する
        /// </summary>
        /// <param name="scriptedImporter"></param>
        /// <param name="context"></param>
        /// <param name="reverseAxis"></param>
        /// <param name="renderPipeline"></param>
        protected static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, Axes reverseAxis, ImporterRenderPipelineTypes renderPipeline)
        {
            UniGLTFLogger.Log("OnImportAsset to " + scriptedImporter.assetPath);

            //
            // Import(create unity objects)
            //

            // 2 回目以降の Asset Import において、 Importer の設定で Extract した UnityEngine.Object が入る
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .Where(x => x.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            var materialGenerator = GetMaterialDescriptorGenerator(renderPipeline);

            using (var data = new AutoGltfFileParser(scriptedImporter.assetPath).Parse())
            using (var loader = new ImporterContext(data, extractedObjects, materialGenerator: materialGenerator))
            {
                // Configure TextureImporter to Extracted Textures.
                foreach (var textureInfo in loader.TextureDescriptorGenerator.Get().GetEnumerable())
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalTextures);
                }

                loader.InvertAxis = reverseAxis;
                var loaded = loader.Load();
                loaded.ShowMeshes();

                loaded.TransferOwnership((k, o) =>
                {
                    context.AddObjectToAsset(k.Name, o);
                });
                var root = loaded.Root;
                GameObject.DestroyImmediate(loaded);

                context.AddObjectToAsset(root.name, root);
                context.SetMainObject(root);
            }
        }

        private static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(ImporterRenderPipelineTypes renderPipeline)
        {
            return renderPipeline switch
            {
                ImporterRenderPipelineTypes.Auto => MaterialDescriptorGeneratorUtility .GetValidGltfMaterialDescriptorGenerator(),
                ImporterRenderPipelineTypes.BuiltinRenderPipeline => MaterialDescriptorGeneratorUtility .GetGltfMaterialDescriptorGenerator(RenderPipelineTypes.BuiltinRenderPipeline),
                ImporterRenderPipelineTypes.UniversalRenderPipeline => MaterialDescriptorGeneratorUtility .GetGltfMaterialDescriptorGenerator(RenderPipelineTypes.UniversalRenderPipeline),
                _ => MaterialDescriptorGeneratorUtility.GetValidGltfMaterialDescriptorGenerator(),
            };
        }
    }
}

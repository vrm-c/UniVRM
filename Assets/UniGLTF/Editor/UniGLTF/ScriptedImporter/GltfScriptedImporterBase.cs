using UnityEngine;
using System.Linq;
using VRMShaders;
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
        [Header("Experimental")]
        public RenderPipelineTypes m_renderPipeline;

        void OnValidate()
        {
            if (m_renderPipeline == UniGLTF.RenderPipelineTypes.UniversalRenderPipeline)
            {
                if (Shader.Find(UniGLTF.GltfPbrUrpMaterialImporter.ShaderName) == null)
                {
                    Debug.LogWarning("URP is not installed. Force to BuiltinRenderPipeline");
                    m_renderPipeline = UniGLTF.RenderPipelineTypes.BuiltinRenderPipeline;
                }
            }
        }

        static IMaterialDescriptorGenerator GetMaterialGenerator(RenderPipelineTypes renderPipeline)
        {
            switch (renderPipeline)
            {
                case RenderPipelineTypes.BuiltinRenderPipeline:
                    return new GltfMaterialDescriptorGenerator();

                case RenderPipelineTypes.UniversalRenderPipeline:
                    return new GltfUrpMaterialDescriptorGenerator();

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// glb をパースして、UnityObject化、さらにAsset化する
        /// </summary>
        /// <param name="scriptedImporter"></param>
        /// <param name="context"></param>
        /// <param name="reverseAxis"></param>
        protected static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, Axes reverseAxis, RenderPipelineTypes renderPipeline)
        {
#if VRM_DEVELOP
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            //
            // Parse(parse glb, parser gltf json)
            //
            var data = new AutoGltfFileParser(scriptedImporter.assetPath).Parse();


            //
            // Import(create unity objects)
            //

            // 2 回目以降の Asset Import において、 Importer の設定で Extract した UnityEngine.Object が入る
            var extractedObjects = scriptedImporter.GetExternalObjectMap()
                .Where(x => x.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            IMaterialDescriptorGenerator materialGenerator = GetMaterialGenerator(renderPipeline);

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
    }
}

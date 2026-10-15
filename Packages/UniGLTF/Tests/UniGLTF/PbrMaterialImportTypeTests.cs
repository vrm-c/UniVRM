using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// PbrMaterialImportType による IMaterialDescriptorGenerator の選択と、
    /// GltfCompatible 指定でのランタイムロード (UniGltfPbr シェーダ割り当て) の検証。
    /// </summary>
    public class PbrMaterialImportTypeTests
    {
        [Test]
        public void GltfCompatible_SelectsInvariantGenerator_RegardlessOfRenderPipeline()
        {
            Assert.IsInstanceOf<InvariantGltfMaterialDescriptorGenerator>(
                MaterialDescriptorGeneratorUtility.GetGltfMaterialDescriptorGenerator(
                    RenderPipelineTypes.BuiltinRenderPipeline, PbrMaterialImportType.GltfCompatible));
            Assert.IsInstanceOf<InvariantGltfMaterialDescriptorGenerator>(
                MaterialDescriptorGeneratorUtility.GetGltfMaterialDescriptorGenerator(
                    RenderPipelineTypes.UniversalRenderPipeline, PbrMaterialImportType.GltfCompatible));
        }

        [Test]
        public void UnityStandard_SelectsRenderPipelineSpecificGenerator()
        {
            Assert.IsInstanceOf<BuiltInGltfMaterialDescriptorGenerator>(
                MaterialDescriptorGeneratorUtility.GetGltfMaterialDescriptorGenerator(
                    RenderPipelineTypes.BuiltinRenderPipeline, PbrMaterialImportType.UnityStandard));
            Assert.IsInstanceOf<UrpGltfMaterialDescriptorGenerator>(
                MaterialDescriptorGeneratorUtility.GetGltfMaterialDescriptorGenerator(
                    RenderPipelineTypes.UniversalRenderPipeline, PbrMaterialImportType.UnityStandard));
        }

        [Test]
        public void Default_IsNotInvariantGenerator()
        {
            // 引数省略時 (UnityStandard) はアクティブな RenderPipeline に応じた従来の生成器になる。
            // 具象型はプロジェクトの RenderPipeline 設定に依存するため断定しない。
            Assert.IsNotInstanceOf<InvariantGltfMaterialDescriptorGenerator>(
                MaterialDescriptorGeneratorUtility.GetValidGltfMaterialDescriptorGenerator());
        }

        [Test]
        public void LoadBytesAsync_WithGltfCompatibleGenerator_UsesUniGltfPbrShader()
        {
            if (UniGltfPbrContext.GetShader() == null)
            {
                Assert.Ignore($"shader '{UniGltfPbrContext.ShaderName}' not found");
            }

            var go = TestGltf.CreatePrimitiveAsBuiltInRP(PrimitiveType.Cube);
            try
            {
                var bytes = TestGltf.ExportAsBuiltInRP(go).ToGlbBytes();
                var instance = GltfUtility.LoadBytesAsync(
                    "",
                    bytes,
                    awaitCaller: new ImmediateCaller(),
                    materialGenerator: MaterialDescriptorGeneratorUtility.GetValidGltfMaterialDescriptorGenerator(PbrMaterialImportType.GltfCompatible)).Result;
                try
                {
                    var material = instance.GetComponentInChildren<Renderer>().sharedMaterial;
                    Assert.AreEqual(UniGltfPbrContext.ShaderName, material.shader.name);
                }
                finally
                {
                    Object.DestroyImmediate(instance.gameObject);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LoadBytesAsync_Default_DoesNotUseUniGltfPbrShader()
        {
            var go = TestGltf.CreatePrimitiveAsBuiltInRP(PrimitiveType.Cube);
            try
            {
                var bytes = TestGltf.ExportAsBuiltInRP(go).ToGlbBytes();
                var instance = GltfUtility.LoadBytesAsync(
                    "",
                    bytes,
                    awaitCaller: new ImmediateCaller()).Result;
                try
                {
                    var material = instance.GetComponentInChildren<Renderer>().sharedMaterial;
                    Assert.AreNotEqual(UniGltfPbrContext.ShaderName, material.shader.name);
                }
                finally
                {
                    Object.DestroyImmediate(instance.gameObject);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

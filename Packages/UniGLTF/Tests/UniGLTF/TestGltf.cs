using UnityEngine;

namespace UniGLTF
{
    public static class TestGltf
    {
        public static RuntimeGltfInstance LoadBytesAsBuiltInRP(byte[] bytes)
        {
            return GltfUtility.LoadBytesAsync(
                "",
                bytes,
                awaitCaller: new ImmediateCaller(),
                materialGenerator: new BuiltInGltfMaterialDescriptorGenerator()
            ).Result;
        }

        public static RuntimeGltfInstance LoadPathAsBuiltInRP(string path)
        {
            return GltfUtility.LoadAsync(
                path,
                awaitCaller: new ImmediateCaller(),
                materialGenerator: new BuiltInGltfMaterialDescriptorGenerator()
            ).Result;
        }

        public static ExportingGltfData ExportAsBuiltInRP(GameObject gameObject, GltfExportSettings exportSettings = null)
        {
            var data = new ExportingGltfData();
            using var exporter = new gltfExporter(
                data,
                exportSettings ?? new GltfExportSettings(),
                progress: new EditorProgress(),
                animationExporter: new EditorAnimationExporter(),
                materialExporter: new BuiltInGltfMaterialExporter(),
                textureSerializer: new EditorTextureSerializer()
            );
            exporter.Prepare(gameObject);
            exporter.Export();

            return data;
        }

        public static GameObject CreatePrimitiveAsBuiltInRP(PrimitiveType primitiveType)
        {
            var go = GameObject.CreatePrimitive(primitiveType);
            var shader = Shader.Find("Standard");
            var material = new Material(shader);
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
    }
}
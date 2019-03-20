using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UniGLTF
{
    public class ExportTest
    {
        [TestCase("Standard", "Standard")]
        [TestCase("Unlit/Color", "UniGLTF/UniUnlit")]
        [TestCase("Unlit/Texture", "UniGLTF/UniUnlit")]
        [TestCase("Unlit/Transparent", "UniGLTF/UniUnlit")]
        [TestCase("Unlit/Transparent Cutout", "UniGLTF/UniUnlit")]
        [TestCase("UniGLTF/UniUnlit", "UniGLTF/UniUnlit")]
        public void RuntimeExportShaderTest(string srcShaderName, string dstShaderName)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var srcRenderer = gameObject.GetComponent<Renderer>();
            var material = new Material(Shader.Find(srcShaderName));
            srcRenderer.material = material;

            var gltf = new glTF();
            var exporter = new UniGLTF.gltfExporter(gltf);
            exporter.Prepare(gameObject);
            exporter.Export();
            var bytes = gltf.ToGlbBytes();

            var importer = new UniGLTF.ImporterContext();
            importer.ParseGlb(bytes);
            importer.Load();
            var dstRenderer = importer.Root.GetComponentInChildren<Renderer>();

            Assert.AreEqual(dstShaderName, dstRenderer.material.shader.name);

        }
    }
}


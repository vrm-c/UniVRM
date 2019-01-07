using NUnit.Framework;
using System.IO;
using UniJSON;
using UnityEngine;


namespace VRM
{
    public static class JsonExtensions
    {
        public static void SetValue<T>(this ListTreeNode<JsonValue> node, string key, T value)
        {
            var f = new JsonFormatter();
            f.Serialize(value);
            var p = Utf8String.From(key);
            var bytes = f.GetStoreBytes();
            node.SetValue(p, bytes);
        }
    }
    
    public class VRMImportExportTests
    {
        [Test]
        public void ImportExportTest()
        {
            var path = UniGLTF.UnityPath.FromUnityPath("Models/Alicia_vrm-0.40/AliciaSolid_vrm-0.40.vrm");
            var context = new VRMImporterContext();
            context.ParseGlb(File.ReadAllBytes(path.FullPath));
            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();

            using (new ActionDisposer(() => { GameObject.DestroyImmediate(context.Root); }))
            {
                var importJson = JsonParser.Parse(context.Json);
                importJson.SetValue("/extensions/VRM/exporterVersion", VRMVersion.VRM_VERSION);
                importJson.SetValue("/asset/generator", UniGLTF.UniGLTFVersion.UNIGLTF_VERSION);
                importJson.SetValue("/scene", 0);
                importJson.SetValue("/materials/*/doubleSided", false);
                //importJson.SetValue("/materials/*/pbrMetallicRoughness/roughnessFactor", 0);
                //importJson.SetValue("/materials/*/pbrMetallicRoughness/baseColorFactor", new float[] { 1, 1, 1, 1 });
                importJson.SetValue("/accessors/*/normalized", false);
                importJson.RemoveValue(Utf8String.From("/nodes/*/extras"));
                /*
                importJson.SetValue("/bufferViews/12/byteStride", 4);
                importJson.SetValue("/bufferViews/13/byteStride", 4);
                importJson.SetValue("/bufferViews/14/byteStride", 4);
                importJson.SetValue("/bufferViews/15/byteStride", 4);
                importJson.SetValue("/bufferViews/22/byteStride", 4);
                importJson.SetValue("/bufferViews/29/byteStride", 4);
                importJson.SetValue("/bufferViews/45/byteStride", 4);
                importJson.SetValue("/bufferViews/46/byteStride", 4);
                importJson.SetValue("/bufferViews/47/byteStride", 4);
                importJson.SetValue("/bufferViews/201/byteStride", 4);
                importJson.SetValue("/bufferViews/202/byteStride", 4);
                importJson.SetValue("/bufferViews/203/byteStride", 4);
                importJson.SetValue("/bufferViews/204/byteStride", 4);
                importJson.SetValue("/bufferViews/211/byteStride", 4);
                importJson.SetValue("/bufferViews/212/byteStride", 4);
                importJson.SetValue("/bufferViews/213/byteStride", 4);
                importJson.SetValue("/bufferViews/214/byteStride", 4);
                importJson.SetValue("/bufferViews/215/byteStride", 4);
                importJson.SetValue("/bufferViews/243/byteStride", 4);
                importJson.SetValue("/bufferViews/247/byteStride", 64);
                importJson.SetValue("/bufferViews/248/byteStride", 64);
                importJson.SetValue("/bufferViews/249/byteStride", 64);
                importJson.SetValue("/bufferViews/250/byteStride", 64);
                importJson.SetValue("/bufferViews/251/byteStride", 64);
                importJson.SetValue("/bufferViews/252/byteStride", 64);
                importJson.SetValue("/bufferViews/253/byteStride", 64);
                */
                importJson.RemoveValue(Utf8String.From("/bufferViews/*/byteStride"));

                var vrm = VRMExporter.Export(context.Root);
                var exportJson = JsonParser.Parse(vrm.ToJson());

                /*
                foreach (var kv in importJson.Diff(exportJson))
                {
                    Debug.Log(kv);
                }

                Assert.AreEqual(importJson, exportJson);
                */
            }
        }

        [Test]
        public void MeshCoyTest()
        {
            var path = UniGLTF.UnityPath.FromUnityPath("Models/Alicia_vrm-0.40/AliciaSolid_vrm-0.40.vrm");
            var context = new VRMImporterContext();
            context.ParseGlb(File.ReadAllBytes(path.FullPath));
            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();

            foreach (var mesh in context.Meshes)
            {
                var src = mesh.Mesh;
                var dst = src.Copy(true);
                MeshTests.MeshEquals(src, dst);
            }
        }
    }
}

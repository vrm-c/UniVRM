
using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public class VRMLookAtTests
    {
        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        [Test]
        public void VRMLookAtTest()
        {
            var parser = new GltfParser();
            parser.ParsePath(AliciaPath);
            byte[] bytes = default;
            using (var loader = new VRMImporterContext(parser))
            {
                loader.Load();
                loader.ShowMeshes();

                var go = loader.Root;
                var fp = go.GetComponent<VRMFirstPerson>();
                GameObject.DestroyImmediate(go.GetComponent<VRMLookAtBoneApplyer>());
                var lookAt = go.AddComponent<VRMLookAtBlendShapeApplyer>();
                bytes = VRMEditorExporter.Export(go, null, new VRMExportSettings
                {
                    PoseFreeze = true,
                });
            }

            var parser2 = new GltfParser();
            parser2.Parse(AliciaPath, bytes);
            using (var loader2 = new VRMImporterContext(parser2))
            {
                Assert.AreEqual(LookAtType.BlendShape, loader2.VRM.firstPerson.lookAtType);
            }
        }

        [Test]
        public void VRMLookAtCurveMapWithFreezeTest()
        {
            var parser = new GltfParser();
            parser.ParsePath(AliciaPath);
            byte[] bytes = default;
            CurveMapper horizontalInner = default;
            using (var loader = new VRMImporterContext(parser))
            {
                loader.Load();
                loader.ShowMeshes();

                var go = loader.Root;
                var fp = go.GetComponent<VRMFirstPerson>();
                var lookAt = go.GetComponent<VRMLookAtBoneApplyer>();
                horizontalInner = lookAt.HorizontalInner;
                bytes = VRMEditorExporter.Export(go, null, new VRMExportSettings
                {
                    PoseFreeze = true,
                });
            }

            var parser2 = new GltfParser();
            parser2.Parse(AliciaPath, bytes);
            using (var loader = new VRMImporterContext(parser2))
            {
                loader.Load();
                loader.ShowMeshes();

                var lookAt = loader.Root.GetComponent<VRMLookAtBoneApplyer>();
                Assert.AreEqual(horizontalInner.CurveXRangeDegree, lookAt.HorizontalInner.CurveXRangeDegree);
                Assert.AreEqual(horizontalInner.CurveYRangeDegree, lookAt.HorizontalInner.CurveYRangeDegree);
            }
        }

        [Test]
        public void VRMLookAtCurveMapTest()
        {
            var parser = new GltfParser();
            parser.ParsePath(AliciaPath);
            byte[] bytes = default;
            CurveMapper horizontalInner = default;
            using (var loader = new VRMImporterContext(parser))
            {
                loader.Load();
                loader.ShowMeshes();

                var go = loader.Root;
                var fp = go.GetComponent<VRMFirstPerson>();
                var lookAt = go.GetComponent<VRMLookAtBoneApplyer>();
                horizontalInner = lookAt.HorizontalInner;
                bytes = VRMEditorExporter.Export(go, null, new VRMExportSettings
                {
                    PoseFreeze = false,
                });
            }

            var parser2 = new GltfParser();
            parser2.Parse(AliciaPath, bytes);
            using (var loader = new VRMImporterContext(parser2))
            {
                loader.Load();
                loader.ShowMeshes();

                var lookAt = loader.Root.GetComponent<VRMLookAtBoneApplyer>();
                Assert.AreEqual(horizontalInner.CurveXRangeDegree, lookAt.HorizontalInner.CurveXRangeDegree);
                Assert.AreEqual(horizontalInner.CurveYRangeDegree, lookAt.HorizontalInner.CurveYRangeDegree);
            }
        }
    }
}

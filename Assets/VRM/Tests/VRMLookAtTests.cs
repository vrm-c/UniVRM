
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
                go.AddComponent<VRMLookAtBlendShapeApplyer>();
                bytes = VRMEditorExporter.Export(go, null, new VRMExportSettings
                {
                    PoseFreeze = true,
                });
            }

            var parser2 = new GltfParser();
            parser2.Parse(AliciaPath, bytes);
            var loader2 = new VRMImporterContext(parser2);

            Assert.AreEqual(LookAtType.BlendShape, loader2.VRM.firstPerson.lookAtType);
        }
    }
}

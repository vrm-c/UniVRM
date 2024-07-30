
using System;
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
            byte[] bytes = default;
            using var loaded = TestVrm0X.LoadPathAsBuiltInRP(AliciaPath);
            {
                loaded.ShowMeshes();

                var go = loaded.gameObject;
                var fp = go.GetComponent<VRMFirstPerson>();
                GameObject.DestroyImmediate(go.GetComponent<VRMLookAtBoneApplyer>());
                var lookAt = go.AddComponent<VRMLookAtBlendShapeApplyer>();
                var settings = (VRMExportSettings)ScriptableObject.CreateInstance<VRMExportSettings>();
                settings.PoseFreeze = true;
                bytes = TestVrm0X.ExportAsBuiltInRP(go, settings);
            }

            {
                using var data2 = new GlbLowLevelParser(AliciaPath, bytes).Parse();
                using var loader2 = new VRMImporterContext(new VRMData(data2));
                Assert.AreEqual(LookAtType.BlendShape, loader2.VRM.firstPerson.lookAtType);
            }
        }

        [Test]
        public void VRMLookAtCurveMapWithFreezeTest()
        {
            byte[] bytes;
            CurveMapper horizontalInner = default;
            {
                using var loaded = TestVrm0X.LoadPathAsBuiltInRP(AliciaPath);
                loaded.ShowMeshes();

                var go = loaded.gameObject;
                var fp = go.GetComponent<VRMFirstPerson>();
                var lookAt = go.GetComponent<VRMLookAtBoneApplyer>();
                horizontalInner = lookAt.HorizontalInner;
                var settings = ScriptableObject.CreateInstance<VRMExportSettings>();
                settings.PoseFreeze = true;
                bytes = TestVrm0X.ExportAsBuiltInRP(go, settings);
            }

            {
                using var loaded = TestVrm0X.LoadBytesAsBuiltInRP(bytes);
                loaded.ShowMeshes();

                var lookAt = loaded.GetComponent<VRMLookAtBoneApplyer>();
                Assert.AreEqual(horizontalInner.CurveXRangeDegree, lookAt.HorizontalInner.CurveXRangeDegree);
                Assert.AreEqual(horizontalInner.CurveYRangeDegree, lookAt.HorizontalInner.CurveYRangeDegree);
            }
        }

        [Test]
        public void VRMLookAtCurveMapTest()
        {
            byte[] bytes;
            CurveMapper horizontalInner = default;
            {
                using var loaded = TestVrm0X.LoadPathAsBuiltInRP(AliciaPath);
                loaded.ShowMeshes();

                var go = loaded.gameObject;
                var fp = go.GetComponent<VRMFirstPerson>();
                var lookAt = go.GetComponent<VRMLookAtBoneApplyer>();
                horizontalInner = lookAt.HorizontalInner;
                var settings = (VRMExportSettings)ScriptableObject.CreateInstance<VRMExportSettings>();
                settings.PoseFreeze = false;
                bytes = TestVrm0X.ExportAsBuiltInRP(go, settings);
            }

            {
                using var loaded = TestVrm0X.LoadBytesAsBuiltInRP(bytes);
                loaded.ShowMeshes();

                var lookAt = loaded.GetComponent<VRMLookAtBoneApplyer>();
                Assert.AreEqual(horizontalInner.CurveXRangeDegree, lookAt.HorizontalInner.CurveXRangeDegree);
                Assert.AreEqual(horizontalInner.CurveYRangeDegree, lookAt.HorizontalInner.CurveYRangeDegree);
            }
        }
    }
}

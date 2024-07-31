using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.Test
{
    public class ApiSampleTests
    {
        [Test]
        public void Sample()
        {
            var path = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";
            Debug.Log($"load: {path}");

            var instance = TestVrm10.LoadPathAsBuiltInRP(path, canLoadVrm0X: true);
            Assert.NotNull(instance);

            var go = instance.gameObject;
            Debug.Log(go);

            var vrmBytes = TestVrm10.ExportAsBuiltInRP(go);
            Debug.Log($"export {vrmBytes.Length} bytes");
        }
    }
}

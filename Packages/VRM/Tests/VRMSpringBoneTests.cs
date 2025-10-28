using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace VRM
{
    public class VRMSpringBoneTests
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
        public void DuplicatedJoints()
        {
            using var loaded = TestVrm0X.LoadPathAsBuiltInRP(AliciaPath);
            var sb = loaded.GetComponentInChildren<VRMSpringBone>();

            // make duplicate #2617
            sb.RootBones.Add(sb.RootBones[0].GetChild(0));

            Assert.DoesNotThrow(() => sb.Setup());
        }
    }
}
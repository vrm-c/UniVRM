using System.Linq;
using NUnit.Framework;
using UniGLTF.Utils;
using UnityEngine;


namespace UniGLTF
{
    public class CacheEnumTest
    {
        [Test]
        public void CacheEnumTestSimplePasses()
        {
            Assert.AreEqual(default(HumanBodyBones), CachedEnum.ParseOrDefault<HumanBodyBones>("xxx"));
            Assert.AreEqual(HumanBodyBones.UpperChest, CachedEnum.ParseOrDefault<HumanBodyBones>("upperchest", true));
            Assert.AreEqual(CachedEnum.GetValues<HumanBodyBones>().First(x => x == HumanBodyBones.Hips), HumanBodyBones.Hips);
        }
    }
}

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
            Assert.AreEqual(default(HumanBodyBones), CachedEnum.TryParseOrDefault<HumanBodyBones>("xxx"));
            Assert.AreEqual(HumanBodyBones.UpperChest, CachedEnum.TryParseOrDefault<HumanBodyBones>("upperchest", true));
        }
    }
}

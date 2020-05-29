using NUnit.Framework;
using UnityEngine;


namespace UniGLTF
{
    public class CacheEnumTest
    {
        [Test]
        public void CacheEnumTestSimplePasses()
        {
            Assert.AreEqual(default(HumanBodyBones), CacheEnum.TryParseOrDefault<HumanBodyBones>("xxx"));

#if UNITY_5_6_OR_NEWER
            Assert.AreEqual(HumanBodyBones.UpperChest, CacheEnum.TryParseOrDefault<HumanBodyBones>("upperchest", true));
#else
            Assert.AreEqual(default(HumanBodyBones), CacheEnum.TryParseOrDefault<HumanBodyBones>("upperchest"));
#endif
        }
    }
}

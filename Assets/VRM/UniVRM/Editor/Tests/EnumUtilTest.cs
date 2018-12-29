using NUnit.Framework;
using UnityEngine;


namespace VRM
{
    public class EnumUtilTest
    {
        [Test]
        public void EnumUtilTestSimplePasses()
        {
            Assert.AreEqual(default(HumanBodyBones), EnumUtil.TryParseOrDefault<HumanBodyBones>("xxx"));

#if UNITY_5_6_OR_NEWER
            Assert.AreEqual(HumanBodyBones.UpperChest, EnumUtil.TryParseOrDefault<HumanBodyBones>("upperchest"));
#else
            Assert.AreEqual(default(HumanBodyBones), EnumUtil.TryParseOrDefault<HumanBodyBones>("upperchest"));
#endif
        }
    }
}

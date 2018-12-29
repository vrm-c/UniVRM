using NUnit.Framework;
using System.Collections.Generic;
using VRM;


namespace VRM
{
    public class VRMBlendShapeKeyTest
    {
        [Test]
        public void KeyTest()
        {
            var key = new BlendShapeKey("Blink", BlendShapePreset.Blink);

            Assert.AreEqual(key, new BlendShapeKey("blink"));
            Assert.AreEqual(key, new BlendShapeKey(BlendShapePreset.Blink));
            Assert.AreEqual(key, new BlendShapeKey("xxx", BlendShapePreset.Blink));

            var dict = new Dictionary<BlendShapeKey, float>();
            dict[new BlendShapeKey("xxx", BlendShapePreset.Blink)] = 1.0f;

            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey("blink")));
            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey(BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey("xxx", BlendShapePreset.Blink)));
        }
    }
}

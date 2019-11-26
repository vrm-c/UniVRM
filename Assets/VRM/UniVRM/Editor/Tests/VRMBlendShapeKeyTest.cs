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

            Assert.AreEqual(key, new BlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreEqual(key, new BlendShapeKey(BlendShapePreset.Blink));
            Assert.AreEqual(key, new BlendShapeKey("xxx", BlendShapePreset.Blink));

            var dict = new Dictionary<BlendShapeKey, float>();
            dict[key] = 1.0f;

            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey("Blink",BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey(BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(new BlendShapeKey("xxx", BlendShapePreset.Blink)));
            
            dict.Clear();
            
            var key2 = new BlendShapeKey("Blink"); // name: Blink, Preset: Unknown
            dict[key2] = 1.0f;
            
            Assert.AreEqual( key2, new BlendShapeKey("Blink", BlendShapePreset.Unknown));
            Assert.AreNotEqual(key2, new BlendShapeKey("blink"));
            Assert.AreNotEqual(key2, new BlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreNotEqual(key2, new BlendShapeKey(BlendShapePreset.Blink));
            
            Assert.IsFalse(dict.ContainsKey(new BlendShapeKey("blink")));
            Assert.IsFalse(dict.ContainsKey(new BlendShapeKey("Blink",BlendShapePreset.Blink)));
            Assert.IsFalse(dict.ContainsKey(new BlendShapeKey(BlendShapePreset.Blink)));
            
        }
    }
}
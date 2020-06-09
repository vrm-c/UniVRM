using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace VRM
{
    public class VRMBlendShapeKeyTest
    {
        static BlendShapeKey CreateBlendShapeKey(string name, BlendShapePreset preset)
        {
            var argTypes = new Type[] { typeof(string), typeof(BlendShapePreset) };
            // private constructor
            var constructor = typeof(BlendShapeKey).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, argTypes, null);
            return (BlendShapeKey)constructor.Invoke(new object[] { name, preset });
        }

        [Test]
        public void KeyTest()
        {
            var key = CreateBlendShapeKey("Blink", BlendShapePreset.Blink);
            Assert.AreEqual(key, CreateBlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreEqual(key, BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink));
            Assert.AreEqual(key, CreateBlendShapeKey("xxx", BlendShapePreset.Blink));

            var dict = new Dictionary<BlendShapeKey, float>();
            dict[key] = 1.0f;

            Assert.IsTrue(dict.ContainsKey(CreateBlendShapeKey("Blink", BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(CreateBlendShapeKey("xxx", BlendShapePreset.Blink)));

            dict.Clear();

            var key2 = BlendShapeKey.CreateUnknown("Blink"); // name: Blink, Preset: Unknown
            dict[key2] = 1.0f;

            Assert.AreEqual(key2, CreateBlendShapeKey("Blink", BlendShapePreset.Unknown));
            Assert.AreNotEqual(key2, BlendShapeKey.CreateUnknown("blink"));
            Assert.AreNotEqual(key2, CreateBlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreNotEqual(key2, BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink));

            Assert.IsFalse(dict.ContainsKey(BlendShapeKey.CreateUnknown("blink")));
            Assert.IsFalse(dict.ContainsKey(CreateBlendShapeKey("Blink", BlendShapePreset.Blink)));
            Assert.IsFalse(dict.ContainsKey(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink)));
        }
    }
}

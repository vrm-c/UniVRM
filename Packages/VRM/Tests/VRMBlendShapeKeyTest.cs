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
            var argTypes = new Type[] {typeof(string), typeof(BlendShapePreset)};
            // private constructor
            var constructor = typeof(BlendShapeKey).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, argTypes, null);
            return (BlendShapeKey) constructor.Invoke(new object[] {name, preset});
        }

        [Test]
        public void KeyTest()
        {
            var key = BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink);
            Assert.AreEqual(key, BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink));
            Assert.AreEqual(key, CreateBlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreEqual(key, CreateBlendShapeKey("xxx", BlendShapePreset.Blink));
            Assert.AreEqual(key.Name, "Blink");

            var dict = new Dictionary<BlendShapeKey, float>();
            dict[key] = 1.0f;

            Assert.IsTrue(dict.ContainsKey(CreateBlendShapeKey("Blink", BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink)));
            Assert.IsTrue(dict.ContainsKey(CreateBlendShapeKey("xxx", BlendShapePreset.Blink)));

            dict.Clear();

            var key2 = BlendShapeKey.CreateUnknown("Blink"); // Name: Blink, Preset: Unknown
            dict[key2] = 1.0f;

            Assert.AreEqual(key2, CreateBlendShapeKey("Blink", BlendShapePreset.Unknown));
            Assert.AreNotEqual(key2, BlendShapeKey.CreateUnknown("blink"));
            Assert.AreNotEqual(key2, CreateBlendShapeKey("Blink", BlendShapePreset.Blink));
            Assert.AreNotEqual(key2, BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink));
            Assert.AreEqual(key2.Name, "Blink");

            Assert.IsFalse(dict.ContainsKey(BlendShapeKey.CreateUnknown("blink")));
            Assert.IsFalse(dict.ContainsKey(CreateBlendShapeKey("Blink", BlendShapePreset.Blink)));
            Assert.IsFalse(dict.ContainsKey(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink)));

            var key3 = CreateBlendShapeKey("xxx", BlendShapePreset.Blink); // Unknown 以外は独自の名前を持てない
            Assert.AreEqual(key3, BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink));
            Assert.AreNotEqual(key3, CreateBlendShapeKey("xxx", BlendShapePreset.Unknown));
            Assert.AreEqual(key3.Name, "Blink");
        }
    }
}

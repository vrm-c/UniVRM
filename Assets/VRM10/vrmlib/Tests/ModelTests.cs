using System.Numerics;
using VrmLib;
using NUnit.Framework;

namespace VrmLibTests
{
    public class ModelTests
    {
        [Test]
        public void NodeTest()
        {
            var root = new Node("root");
            var child = new Node("child")
            {
                LocalTranslation = Vector3.UnitX
            };
            root.Add(child);
            root.LocalTranslation = Vector3.UnitX;

            Assert.AreEqual(new Vector3(2, 0, 0), child.Translation);
        }
    }
}
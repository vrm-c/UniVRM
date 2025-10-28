using VrmLib;
using NUnit.Framework;
using UnityEngine;

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
                LocalTranslation = Vector3.right
            };
            root.Add(child);
            root.LocalTranslation = Vector3.right;

            Assert.AreEqual(new Vector3(2, 0, 0), child.Translation);
        }
    }
}
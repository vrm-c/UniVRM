using System.Numerics;
using VrmLib;
using NUnit.Framework;
using System.IO;

namespace VrmLibTests
{
    public class ModelModifierTests
    {
        [Test]
        public void ModifierTest()
        {
            var model = new Model(Coordinates.Vrm1);

            var node0 = new Node("node0");
            model.NodeAdd(node0);
            Assert.AreEqual(1, model.Nodes.Count);

            var node1 = new Node("node1");
            model.NodeAdd(node1, node0);
            Assert.AreEqual(2, model.Nodes.Count);

            var node2 = new Node("node2");
            model.NodeReplace(node0, node2);
            Assert.AreEqual(2, model.Nodes.Count);
            Assert.AreEqual(node2, model.Nodes[1]);
            Assert.AreEqual(1, node2.Children.Count);

            model.NodeRemove(node1);
            Assert.AreEqual(1, model.Nodes.Count);
            Assert.AreEqual(0, node2.Children.Count);
        }
    }
}

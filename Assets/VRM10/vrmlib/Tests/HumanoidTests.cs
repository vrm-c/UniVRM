using System;
using System.IO;
using VrmLib;
using NUnit.Framework;

namespace VrmLibTests
{
    public class HumanoidTests
    {
        DirectoryInfo RootPath
        {
            get
            {
                return new FileInfo(GetType().Assembly.Location).Directory.Parent.Parent.Parent.Parent;
            }
        }

        [Test]
        [TestCase("Assets/UniVrm.bvh")]
        public void HumanoidTest(string filename)
        {
            var humanoid = new Humanoid();
            Assert.False(humanoid.HasRequiredBones);

            Assert.Throws<ArgumentException>(() => humanoid.Add(HumanoidBones.unknown, null));
        }
    }
}

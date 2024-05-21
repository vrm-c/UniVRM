using System.Threading.Tasks;
using NUnit.Framework;
using UniVRM10;
using VRMShaders;

namespace VRM10.Tests
{
    public sealed class Vrm10ApiTests
    {
        [Test]
        public void LoadImmediately()
        {
            var loadTask = Vrm10.LoadPathAsync(
                TestAsset.AliciaPath,
                canLoadVrm0X: true,
                awaitCaller: new ImmediateCaller()
            );

            Assert.AreEqual(true, loadTask.IsCompleted);
        }
    }
}
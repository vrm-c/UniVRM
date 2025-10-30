using NUnit.Framework;
using UniGLTF;

namespace UniVRM10.Test
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
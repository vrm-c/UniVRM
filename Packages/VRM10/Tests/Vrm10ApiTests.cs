using NUnit.Framework;
using UniGLTF;
using UnityEngine;

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

        [Test]
        public void DefaultTransformStatesOfInstantiatedCopy()
        {
            // A RuntimeGltfInstance copied by Object.Instantiate has empty InitialTransformStates.
            var root = new GameObject("root");
            var child = new GameObject("child");
            child.transform.SetParent(root.transform);
            root.AddComponent<RuntimeGltfInstance>();
            try
            {
                var vrm = root.AddComponent<Vrm10Instance>();
                Assert.True(vrm.DefaultTransformStates.ContainsKey(child.transform));
                Assert.True(RuntimeGltfInstance.SafeGetInitialPose(root.transform).ContainsKey(child.transform));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
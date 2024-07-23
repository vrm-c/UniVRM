using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public sealed class PathObjectTests
    {
        [Test]
        public void Test()
        {
            var dataPath = PathObject.FromFullPath(Application.dataPath);

            Assert.AreEqual("Assets", dataPath.Stem);

            // UnityRoot
            Assert.True(dataPath.IsDescendantOf(PathObject.UnityRoot));
            // UnityRoot/Assets
            Assert.False(dataPath.IsDescendantOf(PathObject.UnityAssets));
            Assert.AreEqual(dataPath, PathObject.UnityAssets);

            Assert.AreEqual(PathObject.UnityRoot.Child("Assets"), PathObject.UnityAssets);
            Assert.AreEqual(PathObject.UnityAssets.Parent, PathObject.UnityRoot);
            Assert.AreEqual("Assets", PathObject.UnityAssets.UnityAssetPath);
        }
    }
}

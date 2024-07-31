using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public class AssetTests
    {
        [Test]
        public void TestAsset()
        {
            var mesh = new Mesh();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/test_mesh.asset");
            var loaded = default(Mesh);

            try
            {
                Assert.IsFalse(AssetDatabase.IsMainAsset(mesh));
                Assert.IsFalse(AssetDatabase.IsSubAsset(mesh));

                AssetDatabase.CreateAsset(mesh, assetPath);

                Assert.IsTrue(AssetDatabase.IsMainAsset(mesh));
                Assert.IsFalse(AssetDatabase.IsSubAsset(mesh));

                loaded = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                Assert.AreEqual(mesh, loaded);
            }
            finally
            {
                // remove assetPath
                UnityEngine.Object.DestroyImmediate(loaded, true);

                var tmp = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                Assert.Null(tmp);

                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.Refresh();
        }
    }
}

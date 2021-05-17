using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public class MeshExportValidatorTests
    {
        [Test]
        public void NoMaterialTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            var root = new GameObject("root");

            try
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(root.transform);

                var renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterials = new Material[0];

                validator.SetRoot(root, MeshExportSettings.Default);
                var vs = validator.Validate(root);
                Assert.False(vs.All(x => x.CanExport));
            }
            finally
            {
                GameObject.DestroyImmediate(root);
                ScriptableObject.DestroyImmediate(validator);
            }
        }

        [Test]
        public void NullMaterialTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            var root = new GameObject("root");

            try
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(root.transform);

                var renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterials = new Material[] { null };

                validator.SetRoot(root, MeshExportSettings.Default);
                var vs = validator.Validate(root);
                Assert.False(vs.All(x => x.CanExport));
            }
            finally
            {
                GameObject.DestroyImmediate(root);
                ScriptableObject.DestroyImmediate(validator);
            }
        }

        [Test]
        public void NullMaterialsTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            var root = new GameObject("root");

            try
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(root.transform);

                var renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterials = new Material[] { null, null };

                validator.SetRoot(root, MeshExportSettings.Default);
                var vs = validator.Validate(root);
                Assert.False(vs.All(x => x.CanExport));
            }
            finally
            {
                GameObject.DestroyImmediate(root);
                ScriptableObject.DestroyImmediate(validator);
            }
        }
    }
}

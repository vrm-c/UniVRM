using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public class MeshExportValidatorTests
    {
        static GameObject CreateTestObject(Material[] materials)
        {
            var root = new GameObject("root");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform);

            var renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterials = materials;
            return root;
        }

        [Test]
        public void NoMaterialTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            // 0 material
            var root = CreateTestObject(new Material[0]);

            try
            {
                validator.SetRoot(root, new GltfExportSettings(), new DefualtBlendShapeExportFilter());
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
            // null を含む
            var root = CreateTestObject(new Material[] { null });

            try
            {
                validator.SetRoot(root, new GltfExportSettings(), new DefualtBlendShapeExportFilter());
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
            // null を含むかつ submeshCount より多い
            var root = CreateTestObject(new Material[] { null, null });

            try
            {
                validator.SetRoot(root, new GltfExportSettings(), new DefualtBlendShapeExportFilter());
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
        public void NoMeshTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            var root = new GameObject("root");

            try
            {
                var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.transform.SetParent(root.transform);
                // remove MeshFilter
                Component.DestroyImmediate(child.GetComponent<MeshFilter>());

                validator.SetRoot(root, new GltfExportSettings(), new DefualtBlendShapeExportFilter());
                var vs = validator.Validate(root);
                Assert.True(vs.All(x => x.CanExport));
            }
            finally
            {
                GameObject.DestroyImmediate(root);
                ScriptableObject.DestroyImmediate(validator);
            }
        }

        [Test]
        public void NullMeshTest()
        {
            var validator = ScriptableObject.CreateInstance<MeshExportValidator>();
            var root = new GameObject("root");

            try
            {
                var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.transform.SetParent(root.transform);
                // set null
                child.GetComponent<MeshFilter>().sharedMesh = null;

                validator.SetRoot(root, new GltfExportSettings(), new DefualtBlendShapeExportFilter());
                var vs = validator.Validate(root);
                Assert.True(vs.All(x => x.CanExport));
            }
            finally
            {
                GameObject.DestroyImmediate(root);
                ScriptableObject.DestroyImmediate(validator);
            }
        }
    }
}

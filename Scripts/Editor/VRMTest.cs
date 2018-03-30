using NUnit.Framework;
using System;
using UniGLTF;
using UnityEngine;
using VRM;


public class VRMTest
{
    static GameObject CreateSimpelScene()
    {
        var root = new GameObject("gltfRoot").transform;

        var scene = new GameObject("scene0").transform;
        scene.SetParent(root, false);
        scene.localPosition = new Vector3(1, 2, 3);

        return root.gameObject;
    }

    void AssertAreEqual(Transform go, Transform other)
    {
        var lt = go.Traverse().GetEnumerator();
        var rt = go.Traverse().GetEnumerator();

        while (lt.MoveNext())
        {
            if (!rt.MoveNext())
            {
                throw new Exception("rt shorter");
            }

            MonoBehaviourComparator.
            AssertAreEquals(lt.Current.gameObject, rt.Current.gameObject);
            VRMMonoBehaviourComparator.
            AssertAreEquals(lt.Current.gameObject, rt.Current.gameObject);
        }

        if (rt.MoveNext())
        {
            throw new Exception("rt longer");
        }
    }

    [Test]
    public void VRMSimpleSceneTest()
    {
        var go = CreateSimpelScene();
        var context = new VRMImporterContext(null);

        try
        {
            // export
            var gltf = VRMExporter.Export(go);

            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export();

                // import
                context.Json = gltf.ToJson();
                Debug.LogFormat("{0}", context.Json);
                gltfImporter.Import<glTF>(context, new ArraySegment<byte>());

                AssertAreEqual(go.transform, context.Root.transform);
            }
        }
        finally
        {
            //Debug.LogFormat("Destory, {0}", go.name);
            GameObject.DestroyImmediate(go);
            context.Destroy(true);
        }
    }
}

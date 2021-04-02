using System;
using System.IO;
using UnityEngine;
using VrmLib;
using UniVRM10;
using UniGLTF;

public class Sample : MonoBehaviour
{
    [SerializeField]
    string m_vrmPath = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";

    static GameObject Import(byte[] bytes, FileInfo path)
    {
        var parser = new GltfParser();
        parser.Parse(path.FullName, bytes);

        using (var loader = new RuntimeUnityBuilder(parser))
        {
            loader.Load();
            loader.ShowMeshes();
            return loader.DisposeOnGameObjectDestroyed().gameObject;
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        var src = new FileInfo(m_vrmPath);
        var vrm0x = Import(File.ReadAllBytes(m_vrmPath), src);

        // Export 1.0
        var exporter = new UniVRM10.RuntimeVrmConverter();
        var model = exporter.ToModelFrom10(vrm0x);
        // 右手系に変換
        model.ConvertCoordinate(VrmLib.Coordinates.Vrm1);
        var exportedBytes = model.ToGlb();

        // Import 1.0
        var vrm10 = Import(exportedBytes, src);
        var pos = vrm10.transform.position;
        pos.x += 1.5f;
        vrm10.transform.position = pos;
        vrm10.name = vrm10.name + "_Imported_v1_0";

        // write
        var path = Path.GetFullPath("vrm10.vrm");
        Debug.Log($"write : {path}");
        File.WriteAllBytes(path, exportedBytes);
    }

    static void Printmatrices(Model model)
    {
        var matrices = model.Skins[0].InverseMatrices.GetSpan<System.Numerics.Matrix4x4>();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < matrices.Length; ++i)
        {
            var m = matrices[i];
            sb.AppendLine($"#{i:00}[{m.M11:.00}, {m.M12:.00}, {m.M13:.00}, {m.M14:.00}][{m.M21:.00}, {m.M22:.00}, {m.M23:.00}, {m.M24:.00}][{m.M31:.00}, {m.M32:.00}, {m.M33:.00}, {m.M34:.00}][{m.M41:.00}, {m.M42:.00}, {m.M43:.00}, {m.M44:.00}]");
        }
        Debug.Log(sb.ToString());
    }
}

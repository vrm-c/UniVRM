using System;
using System.IO;
using UnityEngine;
using VrmLib;
using UniGLTF;


namespace UniVRM10.Sample
{

    public class Sample : MonoBehaviour
    {
        [SerializeField]
        string m_vrmPath = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";

        static GameObject Import(byte[] bytes, FileInfo path)
        {
            if (!Vrm10Data.TryParseOrMigrate(path.FullName, bytes, doMigrate: true, out Vrm10Data result))
            {
                return null;
            }

            using (var loader = new Vrm10Importer(result))
            {
                var loaded = loader.Load();
                loaded.ShowMeshes();
                return loaded.gameObject;
            }
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            var src = new FileInfo(m_vrmPath);
            var go = Import(File.ReadAllBytes(m_vrmPath), src);

            var exportedBytes = Vrm10Exporter.Export(go);

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

}
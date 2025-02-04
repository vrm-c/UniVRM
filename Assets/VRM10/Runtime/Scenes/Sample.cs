using System.IO;
using UniGLTF;
using UnityEngine;


namespace UniVRM10.Sample
{

    public class Sample : MonoBehaviour
    {
        [SerializeField]
        string m_vrmPath = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";

        [SerializeField]
        UniGLTF.GltfExportSettings m_settings = new();

        // Start is called before the first frame update
        void OnEnable()
        {
            Run();
        }

        async void Run()
        {
            var src = new FileInfo(m_vrmPath);
            var instance = await Vrm10.LoadPathAsync(m_vrmPath, true);

            var exportedBytes = Vrm10Exporter.Export(m_settings, instance.gameObject);

            // Import 1.0
            var vrm10 = await Vrm10.LoadBytesAsync(exportedBytes, false);
            var pos = vrm10.transform.position;
            pos.x += 1.5f;
            vrm10.transform.position = pos;
            vrm10.name = vrm10.name + "_Imported_v1_0";

            // write
            var path = Path.GetFullPath("vrm10.vrm");
            UniGLTFLogger.Log($"write : {path}");
            File.WriteAllBytes(path, exportedBytes);
        }
    }
}
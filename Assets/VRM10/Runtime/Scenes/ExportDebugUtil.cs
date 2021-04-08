using UniVRM10;
using VrmLib;

public static class ExportDebugUtil
{
    public static void SaveJson(VrmLib.Model model, string path)
    {
        using (var stream = new System.IO.StreamWriter(path))
        {
            stream.Write(GetJsonString(model));
        }
    }

    public static void SaveVrm(VrmLib.Model model, string path)
    {
        using (var stream = new System.IO.StreamWriter(path))
        {
            stream.Write(model.ToGlb());
        }
    }

    public static string GetJsonString(VrmLib.Model model)
    {
        // export vrm-1.0
        var exporter10 = new Vrm10Exporter(_ => false);
        var option = new VrmLib.ExportArgs
        {
            // vrm = false
        };
        exporter10.Export(null, model, null, option);
        var glbBytes10 = exporter10.Storage.ToBytes();
        var glb10 = UniGLTF.Glb.Parse(glbBytes10);
        return System.Text.Encoding.UTF8.GetString(glb10.Json.Bytes.Array, glb10.Json.Bytes.Offset, glb10.Json.Bytes.Count);
    }
}

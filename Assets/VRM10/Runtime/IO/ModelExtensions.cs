using VrmLib;

namespace UniVRM10
{
    public static class ModelExtensions
    {
        public static byte[] ToGlb(this VrmLib.Model model)
        {
            // export vrm-1.0
            var exporter10 = new Vrm10Exporter(_ => false);
            var option = new VrmLib.ExportArgs
            {
                // vrm = false
            };
            exporter10.Export(null, model, null, option);
            var glb10 = UniGLTF.Glb.Parse(exporter10.Storage.ToBytes());
            return glb10.ToBytes();
        }
    }
}

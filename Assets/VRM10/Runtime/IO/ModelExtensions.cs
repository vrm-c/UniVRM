using System;
using UnityEngine;


namespace UniVRM10
{
    public static class ModelExtensions
    {
        public static byte[] ToGlb(this VrmLib.Model model, GameObject root, RuntimeVrmConverter converter, Func<Texture2D, (byte[], string)> getTextureBytes)
        {
            // export vrm-1.0
            var exporter10 = new Vrm10Exporter(_ => false);
            var option = new VrmLib.ExportArgs
            {
                // vrm = false
            };

            exporter10.Export(root, model, converter, option, getTextureBytes);
            var glb10 = UniGLTF.Glb.Parse(exporter10.Storage.ToBytes());
            return glb10.ToBytes();
        }
    }
}

using System.IO;
using UnityEditor;

namespace UniVRM10
{
    internal static class VrmAnimationMenu
    {
        public static void BvhToVrmAnimationMenu()
        {
            var path = EditorUtility.OpenFilePanel("select bvh", null, "bvh");
            if (!string.IsNullOrEmpty(path))
            {
                var bytes = VrmAnimationExporter.BvhToVrmAnimation(path);
                var dst = EditorUtility.SaveFilePanel("write vrma",
                        Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path),
                        "vrma");
                if (!string.IsNullOrEmpty(dst))
                {
                    File.WriteAllBytes(dst, bytes);
                }
            }
        }
    }
}

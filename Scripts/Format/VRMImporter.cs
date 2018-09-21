using System;
using System.IO;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public static class VRMImporter
    {
        [Obsolete("use VRMImporterContext.Load(path)")]
        public static GameObject LoadFromPath(string path)
        {
            return VRMImporterContext.Load(path).Root;
        }

        [Obsolete("use VRMImporterContext.Load(bytes)")]
        public static GameObject LoadFromBytes(Byte[] bytes)
        {
            return VRMImporterContext.Load(bytes).Root;
        }

        [Obsolete("use VRMImporterContext.Load()")]
        public static void LoadFromBytes(VRMImporterContext context)
        {
            context.Load();
            context.ShowMeshes();
        }

        #region LoadVrmAsync
        [Obsolete("use VVRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            var context = new VRMImporterContext(UnityPath.FromFullpath(path));
            LoadVrmAsync(File.ReadAllBytes(path), onLoaded, onError, show);
        }

        [Obsolete("use VVRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(Byte[] bytes, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            var context = new VRMImporterContext();
            using (context.MeasureTime("ParseGlb"))
            {
                context.ParseGlb(bytes);
            }
            context.LoadAsync(onLoaded, onError, show);
        }

        [Obsolete("use VVRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(VRMImporterContext context, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            context.LoadAsync(onLoaded, onError, show);
        }
        #endregion
    }
}

using System;
using System.IO;
using UniGLTF;
using UnityEngine;
#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER)
using System.Threading.Tasks;
#endif


namespace VRM
{
    public static class VRMImporter
    {
        [Obsolete("use VRMImporterContext.Load(path)")]
        public static GameObject LoadFromPath(string path)
        {
            var context = new VRMImporterContext();
            context.Parse(path, File.ReadAllBytes(path));
            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();
            return context.Root;
        }

        [Obsolete("use VRMImporterContext.Load(bytes)")]
        public static GameObject LoadFromBytes(Byte[] bytes)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();
            return context.Root;
        }

        [Obsolete("use VRMImporterContext.Load()")]
        public static void LoadFromBytes(VRMImporterContext context)
        {
            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();
        }

        #region LoadVrmAsync
        [Obsolete("use VRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            LoadVrmAsync(File.ReadAllBytes(path), onLoaded, onError, show);
        }

        [Obsolete("use VRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(Byte[] bytes, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            var context = new VRMImporterContext();
            using (context.MeasureTime("ParseGlb"))
            {
                context.ParseGlb(bytes);
            }
            context.LoadAsync(_ =>
            {
                if (show)
                {
                    context.ShowMeshes();
                }
                onLoaded(context.Root);
            }, 
            onError);
        }

        [Obsolete("use VRMImporterContext.LoadAsync")]
        public static void LoadVrmAsync(VRMImporterContext context, Action<GameObject> onLoaded, Action<Exception> onError = null, bool show = true)
        {
            context.LoadAsync(_ =>
            {
                if (show)
                {
                    context.ShowMeshes();
                }
                onLoaded(context.Root);
            },
            onError);
        }
        #endregion

#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER && !UNITY_WEBGL)

        [Obsolete("use VRMImporterContext.LoadAsync()")]
        public static Task<GameObject> LoadVrmAsync(string path, bool show = true)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(File.ReadAllBytes(path));
            return LoadVrmAsync(context, show);
        }

        [Obsolete("use VRMImporterContext.LoadAsync()")]
        public static Task<GameObject> LoadVrmAsync(Byte[] bytes, bool show = true)
        {
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            return LoadVrmAsync(context, show);
        }

        [Obsolete("use VRMImporterContext.LoadAsync()")]
        public async static Task<GameObject> LoadVrmAsync(VRMImporterContext ctx, bool show = true)
        {
            await ctx.LoadAsyncTask();
            if (show)
            {
                ctx.ShowMeshes();
            }
            return ctx.Root;
        }

#endif
    }
}

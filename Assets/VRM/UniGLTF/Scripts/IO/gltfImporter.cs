using System;
using System.Threading.Tasks;
using UnityEngine;


namespace UniGLTF
{
    public static class gltfImporter
    {
        public static async Task LoadVrmAsync(string path, Byte[] bytes, Action<GameObject> onLoaded, bool show = true)
        {
            var context = new ImporterContext();
            context.Parse(path, bytes);
            await context.LoadAsync();
            if (show)
            {
                context.ShowMeshes();
            }
            onLoaded(context.Root);
        }
    }
}

using UnityEngine;

namespace UniGLTF.GltfViewer
{
    public static class OpenFileDialog
    {
        public static PathObject Show(string title, params string[] extensions)
        {
            Debug.Log(typeof(UniGLTFLogger));
#if UNITY_STANDALONE_WIN
            return PathObject.FromFullPath(FileDialogForWindows.FileDialog(title, extensions));
#else
            UniGLTFLogger.Warning("Non-Windows runtime file dialogs are not yet implemented.");
            return default;
#endif
        }
    }
}

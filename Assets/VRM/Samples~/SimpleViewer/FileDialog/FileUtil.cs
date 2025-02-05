namespace VRM.SimpleViewer
{
    public static class FileUtil
    {
        public static string OpenFileDialog(string title, params string[] extensions)
        {
#if UNITY_EDITOR
            // EditorUtility.OpenFilePanel
            // TODO: How to specify multiple extensions on OSX?
            // https://github.com/vrm-c/UniVRM/issues/1837
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", extensions[0]);
#elif UNITY_STANDALONE_WIN
            return FileDialogForWindows.FileDialog(title, extensions);
#elif UNITY_WEBGL
            // Open WebGL_VRM0X_SimpleViewer_FileDialog
            // see: Assets/UniGLTF/Runtime/Utils/Plugins/OpenFile.jslib
            WebGLUtil.WebGL_VRM0X_SimpleViewer_FileDialog("Canvas", "FileSelected");
            // Control flow does not return here. return empty string with dummy
            return "";
#else
            // fall back constant path
            UnityEngine.UniGLTFLogger.Warning("Non-Windows runtime file dialogs are not yet implemented.");
            return UnityEngine.Application.dataPath + "/default.vrm";
#endif
        }
    }
}

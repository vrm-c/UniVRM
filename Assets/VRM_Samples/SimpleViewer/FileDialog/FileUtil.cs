namespace VRM.SimpleViewer
{
    public static class FileUtil
    {
        public static string OpenFileDialog(string title, params string[] extensions)
        {
#if UNITY_STANDALONE_WIN
            return FileDialogForWindows.FileDialog(title, extensions);
#elif UNITY_WEBGL
            // Open UniVRM_Sample_WebGLFileDialog
            // see: Assets/UniGLTF/Runtime/Utils/Plugins/OpenFile.jslib
            WebGLUtil.UniVRM_Sample_WebGLFileDialog("Canvas", "FileSelected");
            // Control flow does not return here. return empty string with dummy
            return "";
#elif UNITY_EDITOR
            // EditorUtility.OpenFilePanel
            // TODO: How to specify multiple extensions on OSX?
            // https://github.com/vrm-c/UniVRM/issues/1837
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", extensions[0]);
#else
            // fall back constant path
            UnityEngine.Debug.LogWarning("Non-Windows runtime file dialogs are not yet implemented.");
            return UnityEngine.Application.dataPath + "/default.vrm";
#endif
        }
    }
}

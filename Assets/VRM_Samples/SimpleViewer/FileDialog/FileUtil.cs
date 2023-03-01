namespace VRM.SimpleViewer
{
    public static class FileUtil
    {
        public static string OpenFileDialog(string title, params string[] extensions)
        {
#if UNITY_STANDALONE_WIN
            return FileDialogForWindows.FileDialog(title, extensions);
#elif UNITY_WEBGL
            // Open WebGLFileDialog
            // see: Assets\VRM_Samples\SimpleViewer\Plugins\OpenFile.jslib
            WebGLFileDialog();
            // Control flow does not return here. return empty string with dummy
            return "";
#elif UNITY_EDITOR
            // EditorUtility.OpenFilePanel
            // TODO: How to specify multiple extensions on OSX?
            // https://github.com/vrm-c/UniVRM/issues/1837
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", extensions[0]);
#else
            // fall back constant path
            Debug.LogWarning("Non-Windows runtime file dialogs are not yet implemented.");
            return Application.dataPath + "/default.vrm";
#endif
        }
    }
}

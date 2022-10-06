namespace VRM.SimpleViewer
{
    public static class FileUtil
    {
        public static string OpenFileDialog(string title, params string[] extensions)
        {
#if UNITY_STANDALONE_WIN
            return FileDialogForWindows.FileDialog("open VRM", "vrm", "bvh");
#elif UNITY_WEBGL
            // Open WebGLFileDialog
            // see: Assets\VRM_Samples\SimpleViewer\Plugins\OpenFile.jslib
            WebGLFileDialog();
            // Control flow does not return here. return empty string with dummy
            return "";
#elif UNITY_EDITOR
            // EditorUtility.OpenFilePanel
            // TODO: How to specify multiple extensions on OSX?
            return UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            // fall back constant path
            return Application.dataPath + "/default.vrm";
#endif
        }
    }
}

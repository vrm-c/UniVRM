namespace UniGLTF.GltfViewer
{
    public static class OpenFileDialog
    {
        public static VRMShaders.PathObject Show(string title, params string[] extensions)
        {
#if UNITY_STANDALONE_WIN
            return VRMShaders.PathObject.FromFullPath(FileDialogForWindows.FileDialog(title, extensions));
#else
            Debug.LogWarning("Non-Windows runtime file dialogs are not yet implemented.");
            return default;
#endif
        }
    }
}

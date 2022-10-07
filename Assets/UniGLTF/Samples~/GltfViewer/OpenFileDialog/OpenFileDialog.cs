namespace UniGLTF.GltfViewer
{
    public static class OpenFileDialog
    {
        public static VRMShaders.PathObject Show(string title, params string[] extensions)
        {
#if UNITY_STANDALONE_WIN
            return VRMShaders.PathObject.FromFullPath(FileDialogForWindows.FileDialog(title, extensions));
#else
            return default;
#endif
        }
    }
}

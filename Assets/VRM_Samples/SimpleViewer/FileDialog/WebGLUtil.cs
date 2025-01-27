#if UNITY_WEBGL
using System.Runtime.InteropServices;

namespace VRM.SimpleViewer
{
    public static class WebGLUtil
    {
        [DllImport("__Internal")]
        public static extern void UniVRM_Sample_WebGLFileDialog(string target, string message);
    }
}
#endif

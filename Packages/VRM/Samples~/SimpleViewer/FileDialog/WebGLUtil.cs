#if UNITY_WEBGL
using System.Runtime.InteropServices;

namespace VRM.SimpleViewer
{
    public static class WebGLUtil
    {
        [DllImport("__Internal")]
        public static extern void WebGL_VRM0X_SimpleViewer_FileDialog(string target, string message);
    }
}
#endif

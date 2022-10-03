#if UNITY_WEBGL
using System.Runtime.InteropServices;

namespace VRM.SimpleViewer
{
    public static class WebGLUtil
    {
        [DllImport("__Internal")]
        public static extern void WebGLFileDialog();
    }
}
#endif
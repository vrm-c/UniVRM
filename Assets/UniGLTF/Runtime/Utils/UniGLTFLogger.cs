using UnityEngine;

namespace UniGLTF
{
    public static class UniGLTFLogger
    {
        [System.Diagnostics.Conditional("VRM_DEVELOP")]
        [HideInCallstack]
        public static void Log(string msg) => Debug.Log(msg);
    }
}
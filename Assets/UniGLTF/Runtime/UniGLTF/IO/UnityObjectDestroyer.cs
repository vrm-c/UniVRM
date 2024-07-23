using UnityEngine;

namespace UniGLTF
{
    public static class UnityObjectDestroyer
    {
        public static void DestroyRuntimeOrEditor(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(o);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(o);
            }
        }
    }
}

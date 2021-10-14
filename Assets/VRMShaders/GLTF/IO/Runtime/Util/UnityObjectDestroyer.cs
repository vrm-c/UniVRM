using UnityEngine;

namespace VRMShaders
{
    public static class UnityObjectDestoyer
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

using UnityEngine;

namespace UniVRM10
{
    public static class Vrm10FirstPersonLayerSettings
    {
        public const int DEFAULT_FIRSTPERSON_ONLY_LAYER = 9;
        public const string FIRSTPERSON_ONLY_LAYER_NAME = "VRMFirstPersonOnly";

        public const int DEFAULT_THIRDPERSON_ONLY_LAYER = 10;
        public const string THIRDPERSON_ONLY_LAYER_NAME = "VRMThirdPersonOnly";

        public static int GetLayer(int? arg, string name, int fallback)
        {
            if (arg.HasValue)
            {
                return arg.Value;
            }
            var layer = LayerMask.NameToLayer(name);
            if (layer != -1)
            {
                return layer;
            }
            return fallback;
        }

        public static int GetFirstPersonOnlyLayer(int? arg)
        {
            return GetLayer(arg, FIRSTPERSON_ONLY_LAYER_NAME, DEFAULT_FIRSTPERSON_ONLY_LAYER);
        }

        public static int GetThirdPersonOnlyLayer(int? arg)
        {
            return GetLayer(arg, THIRDPERSON_ONLY_LAYER_NAME, DEFAULT_THIRDPERSON_ONLY_LAYER);
        }
    }
}

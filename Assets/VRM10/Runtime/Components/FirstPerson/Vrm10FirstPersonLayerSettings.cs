using UnityEngine;

namespace UniVRM10
{
    public static class Vrm10FirstPersonLayerSettings
    {
        public const int DEFAULT_FIRSTPERSON_ONLY_LAYER = 9;
        public const string FIRSTPERSON_ONLY_LAYER_NAME = "VRMFirstPersonOnly";
        public static int FIRSTPERSON_ONLY_LAYER = DEFAULT_FIRSTPERSON_ONLY_LAYER;

        public const int DEFAULT_THIRDPERSON_ONLY_LAYER = 10;
        public const string THIRDPERSON_ONLY_LAYER_NAME = "VRMThirdPersonOnly";
        public static int THIRDPERSON_ONLY_LAYER = DEFAULT_THIRDPERSON_ONLY_LAYER;

        // If no layer names are set, use the default layer IDs.
        // Otherwise use the two Unity layers called "VRMFirstPersonOnly" and "VRMThirdPersonOnly".
        public static bool TriedSetupLayer = false;

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

        /// <summary>
        /// Set the VRM first person layer globally.
        /// 
        /// argument > name > constant
        /// 
        /// Only run first.
        /// </summary>
        /// <param name="firstPersonOnlyLayer"></param>
        /// <param name="thirdPersonOnlyLayer"></param>
        public static void SetupLayers(int? firstPersonOnlyLayer = default, int? thirdPersonOnlyLayer = default)
        {
            if (TriedSetupLayer)
            {
                return;
            }
            TriedSetupLayer = true;

            FIRSTPERSON_ONLY_LAYER = GetLayer(firstPersonOnlyLayer, FIRSTPERSON_ONLY_LAYER_NAME, DEFAULT_FIRSTPERSON_ONLY_LAYER);
            THIRDPERSON_ONLY_LAYER = GetLayer(thirdPersonOnlyLayer, THIRDPERSON_ONLY_LAYER_NAME, DEFAULT_THIRDPERSON_ONLY_LAYER);
        }
    }
}

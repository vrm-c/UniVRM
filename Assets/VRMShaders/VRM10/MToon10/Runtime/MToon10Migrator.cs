using UnityEngine;

namespace VRMShaders.VRM10.MToon10.Runtime
{
    /// <summary>
    /// Migrate from VRM 0.x MToon to VRM 1.0 vrmc_materials_mtoon
    /// </summary>
    public static class MToon10Migrator
    {
        /// <summary>
        /// mtoon.shadingToonyFactor
        /// </summary>
        public static float MigrateToShadingToony(float shadingToony0X, float shadingShift0X)
        {
            var (rangeMin, rangeMax) = GetShadingRange0X(shadingToony0X, shadingShift0X);

            // new shadingToony is the margin of range.
            return Mathf.Clamp((2.0f - (rangeMax - rangeMin)) * 0.5f, 0, 1);
        }

        /// <summary>
        /// mtoon.shadingShiftFactor
        /// </summary>
        public static float MigrateToShadingShift(float shadingToony0X, float shadingShift0X)
        {
            var (rangeMin, rangeMax) = GetShadingRange0X(shadingToony0X, shadingShift0X);

            // new shadingShift is the center of range inverted.
            return Mathf.Clamp((rangeMax + rangeMin) * 0.5f * -1f, -1, +1);
        }

        /// <summary>
        /// mtoon.giEqualizationFactor
        /// </summary>
        public static float MigrateToGiEqualization(float giIntensity0X)
        {
            return Mathf.Clamp01(1 - giIntensity0X);
        }

        private static (float min, float max) GetShadingRange0X(float shadingToony0X, float shadingShift0X)
        {
            var rangeMin = shadingShift0X;
            var rangeMax = Mathf.Lerp(1, shadingShift0X, shadingToony0X);

            return (rangeMin, rangeMax);
        }
    }
}
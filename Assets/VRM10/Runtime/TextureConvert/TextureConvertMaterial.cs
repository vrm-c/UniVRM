using UnityEngine;

namespace UniVRM10
{
    public static class TextureConvertMaterial
    {

        #region Normalmap
        // GLTF data to Unity texture
        // ConvertToNormalValueFromRawColorWhenCompressionIsRequired
        public static Material GetNormalMapConvertGltfToUnity()
        {
            return new Material(Shader.Find("UniVRM/NormalMapGltfToUnity"));
        }

        // Unity texture to GLTF data
        // ConvertToRawColorWhenNormalValueIsCompressed
        public static Material GetNormalMapConvertUnityToGltf()
        {
            return new Material(Shader.Find("UniVRM/NormalMapUnityToGltf"));
        }
        #endregion

        #region MetallicRoughness
        // GLTF data to Unity texture
        public static Material GetMetallicRoughnessGltfToUnity(float roughnessFactor)
        {
            var material = new Material(Shader.Find("UniVRM/MetallicRoughnessGltfToUnity"));
            material.SetFloat("_Roughness", roughnessFactor);
            return material;
        }

        // Unity texture to GLTF data
        public static Material GetMetallicRoughnessUnityToGltf(float smoothness)
        {
            var material = new Material(Shader.Find("UniVRM/MetallicRoughnessUnityToGltf"));
            material.SetFloat("_Smoothness", smoothness);
            return material;
        }
        #endregion

        #region Occlusion
        // GLTF data to Unity texture
        public static Material GetOcclusionGltfToUnity()
        {
            return new Material(Shader.Find("UniVRM/OcclusionGltfToUnity"));
        }

        // Unity texture to GLTF data
        public static Material GetOcclusionUnityToGltf()
        {
            return new Material(Shader.Find("UniVRM/OcclusionUnityToGltf"));
        }
        #endregion
    }
}

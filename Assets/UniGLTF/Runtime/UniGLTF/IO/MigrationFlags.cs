namespace UniGLTF
{
    public sealed class MigrationFlags
    {
        /// <summary>
        /// Before UniGLTF v0.54.0, Built-in RP Standard shader and Unlit shaders' albedo color is exported in gamma color space.
        /// https://github.com/vrm-c/UniVRM/pull/339
        /// </summary>
        public bool IsBaseColorFactorGamma { get; set; }

        /// <summary>
        /// Before UniGLTF v0.69, roughness value in the texture was invalid squared value.
        /// https://github.com/vrm-c/UniVRM/pull/780
        /// </summary>
        public bool IsRoughnessTextureValueSquared { get; set; } = false;

        /// <summary>
        /// Before UniGLTF v0.107.0, Built-in RP Standard shader's emission color is exported in gamma color space.
        /// https://github.com/vrm-c/UniVRM/pull/1909
        /// </summary>
        public bool IsEmissiveFactorGamma { get; set; } = false;
    }
}
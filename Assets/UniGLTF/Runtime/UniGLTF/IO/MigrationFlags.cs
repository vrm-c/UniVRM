namespace UniGLTF
{
    public sealed class MigrationFlags
    {
        /// <summary>
        /// Before UniGLTF v0.69, roughness value in the texture was invalid squared value.
        /// </summary>
        public bool IsRoughnessTextureValueSquared { get; set; } = false;

        /// <summary>
        /// Before UniGLTF v0.107.0, Built-in RP Standard shader's emission color is exported in gamma color space.
        /// </summary>
        public bool IsEmissiveFactorGamma { get; set; } = false;
    }
}